using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AppGamboaSite.Shared.Views.Pages
{
    public partial class ImageRemover
    {
        private string? fileName;
        private IBrowserFile? selectedFile;
        private string? originalImageDataUrl;
        private string? processedImageDataUrl;
        private int tolerance = 20;
        private string colorToReplaceHex = "#FFFFFF";
        private bool isLoading = false;
        private string loadingMessage = "A processar...";
        private bool isPickingColor = false;
        private bool hasProcessingStarted = false;

        private const string CanvasId = "imageCanvasBlazorMud";
        private const int MaxCanvasWidth = 600;
        private const int MaxCanvasHeight = 450;

        protected override async Task OnInitializedAsync()
        {
            await Task.CompletedTask;
        }

        private async Task HandleFileSelected(IBrowserFile file)
        {
            if (file == null) return;

            selectedFile = file;
            isLoading = true;
            loadingMessage = "A carregar imagem...";
            processedImageDataUrl = null;
            originalImageDataUrl = null;
            fileName = null;
            hasProcessingStarted = false;
            StateHasChanged();

            try
            {
                fileName = selectedFile.Name;
                long maxFileSize = 10 * 1024 * 1024;
                if (selectedFile.Size > maxFileSize)
                {
                    Snackbar.Add($"Arquivo muito grande. O tamanho máximo é {maxFileSize / (1024 * 1024)}MB.", Severity.Error);
                    fileName = null;
                    GoToNotLoadingState();
                    return;
                }

                using var memoryStream = new MemoryStream();
                await selectedFile.OpenReadStream(maxAllowedSize: maxFileSize).CopyToAsync(memoryStream);

                var buffer = memoryStream.ToArray();
                originalImageDataUrl = $"data:{selectedFile.ContentType};base64,{Convert.ToBase64String(buffer)}";
                StateHasChanged();

                await Task.Delay(100);

                await JSRuntime.InvokeVoidAsync("imageUtils.loadImageOnCanvas", CanvasId, originalImageDataUrl, MaxCanvasWidth, MaxCanvasHeight);
                Snackbar.Add("Imagem carregada. Pode ajustar as opções e remover o fundo.", Severity.Info);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"C#: Erro ao carregar arquivo: {ex.Message}");
                Snackbar.Add($"Erro ao carregar imagem: {ex.Message}", Severity.Error);
                originalImageDataUrl = null;
                fileName = null;
            }
            finally
            {
                GoToNotLoadingState();
            }
        }

        private void ToggleColorPickingMode()
        {
            isPickingColor = !isPickingColor;
            var message = isPickingColor ? "Modo de seleção ativado. Clique na imagem." : "Modo de seleção desativado.";
            Snackbar.Add(message, Severity.Info);
        }

        private async Task HandleCanvasClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
        {
            if (isPickingColor && !string.IsNullOrEmpty(originalImageDataUrl))
            {
                try
                {
                    var pickedColor = await JSRuntime.InvokeAsync<string?>("imageUtils.pickColorFromCanvas", CanvasId, e.ClientX, e.ClientY);
                    if (!string.IsNullOrEmpty(pickedColor))
                    {
                        colorToReplaceHex = pickedColor;
                        Snackbar.Add($"Cor selecionada: {pickedColor}", Severity.Success);
                    }
                    else
                    {
                        Snackbar.Add("Não foi possível selecionar a cor. Tente clicar mais ao centro de uma área colorida.", Severity.Warning);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"C#: Erro ao selecionar cor: {ex.Message}");
                    Snackbar.Add("Erro ao selecionar cor da imagem.", Severity.Error);
                }
                finally
                {
                    isPickingColor = false;
                    StateHasChanged();
                }
            }
        }

        private async Task ProcessImage()
        {
            if (string.IsNullOrEmpty(originalImageDataUrl))
            {
                Snackbar.Add("Por favor, carregue uma imagem primeiro.", Severity.Warning);
                return;
            }

            // Normalizar cor antes de processar
            var normalizedColor = NormalizeColorHex(colorToReplaceHex);

            Console.WriteLine($"C#: Iniciando processamento. Cor original: {colorToReplaceHex}, Normalizada: {normalizedColor}, Tolerância: {tolerance}");

            isLoading = true;
            loadingMessage = "A processar imagem...";
            processedImageDataUrl = null;
            hasProcessingStarted = false;
            StateHasChanged();

            try
            {
                // Recarregar a imagem no canvas
                await JSRuntime.InvokeVoidAsync("imageUtils.loadImageOnCanvas", CanvasId, originalImageDataUrl, MaxCanvasWidth, MaxCanvasHeight);
                await Task.Delay(200);

                Console.WriteLine("C#: Iniciando processamento JavaScript...");

                // Usar cor normalizada
                var jsResult = await JSRuntime.InvokeAsync<string>("imageUtils.processImageDirect", CanvasId, normalizedColor, tolerance);

                Console.WriteLine($"C#: JavaScript retornou: {jsResult}");

                if (jsResult == "SUCCESS")
                {
                    hasProcessingStarted = true;
                    loadingMessage = "Processamento iniciado! Aguarde alguns segundos e clique em 'Verificar Resultado'.";
                    Snackbar.Add("Processamento iniciado com sucesso! Aguarde e depois clique em 'Verificar Resultado'.", Severity.Info);
                    Console.WriteLine("C#: Processamento JavaScript iniciado com sucesso");
                }
                else if (!string.IsNullOrEmpty(jsResult) && jsResult.StartsWith("data:image"))
                {
                    // Método direto funcionou
                    processedImageDataUrl = jsResult;
                    Snackbar.Add("Fundo removido com sucesso!", Severity.Success);
                    Console.WriteLine("C#: Processamento direto concluído com sucesso");
                }
                else
                {
                    Snackbar.Add("Falha ao iniciar processamento.", Severity.Error);
                    Console.WriteLine($"C#: Resultado inesperado: {jsResult}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"C#: Erro geral: {ex.Message}");
                Snackbar.Add($"Erro ao processar: {ex.Message}", Severity.Error);
            }
            finally
            {
                if (!hasProcessingStarted)
                {
                    GoToNotLoadingState();
                }
                else
                {
                    isLoading = false;
                    StateHasChanged();
                }
            }
        }

        private async Task CheckResult()
        {
            Console.WriteLine("C#: Verificando resultado do localStorage...");

            try
            {
                // Usar eval direto para evitar problemas de serialização
                var resultStr = await JSRuntime.InvokeAsync<string>("eval", @"
            (function() {
                try {
                    if (typeof localStorage === 'undefined') {
                        return JSON.stringify({ error: 'localStorage not supported' });
                    }
                    
                    const status = localStorage.getItem('imageProcessingStatus');
                    const result = localStorage.getItem('imageProcessingResult');
                    const error = localStorage.getItem('imageProcessingError');
                    const pixels = localStorage.getItem('imageProcessingPixels') || '0';
                    
                    return JSON.stringify({
                        status: status,
                        hasResult: result !== null && result.length > 0,
                        resultLength: result ? result.length : 0,
                        pixels: pixels,
                        error: error
                    });
                } catch (e) {
                    return JSON.stringify({ error: e.message });
                }
            })()
        ");

                Console.WriteLine($"C#: resultStr = {resultStr}");

                if (!string.IsNullOrEmpty(resultStr))
                {
                    var info = System.Text.Json.JsonSerializer.Deserialize<LocalStorageInfo>(resultStr);

                    Console.WriteLine($"C#: Status: {info.status}, HasResult: {info.hasResult}, Length: {info.resultLength}");

                    if (info.status == "completed" && info.hasResult)
                    {
                        // Recuperar o resultado diretamente
                        var result = await JSRuntime.InvokeAsync<string>("eval", "localStorage.getItem('imageProcessingResult')");

                        if (!string.IsNullOrEmpty(result))
                        {
                            processedImageDataUrl = result;
                            hasProcessingStarted = false;

                            Console.WriteLine($"C#: Resultado recuperado! Tamanho: {result.Length}");

                            // Limpar localStorage
                            await JSRuntime.InvokeVoidAsync("eval", @"
                        localStorage.removeItem('imageProcessingResult');
                        localStorage.removeItem('imageProcessingError');
                        localStorage.removeItem('imageProcessingStatus');
                        localStorage.removeItem('imageProcessingPixels');
                    ");

                            Snackbar.Add($"Resultado recuperado! {info.pixels} pixels processados.", Severity.Success);
                            StateHasChanged();
                            return;
                        }
                    }
                    else if (info.status == "error")
                    {
                        hasProcessingStarted = false;
                        Snackbar.Add($"Erro no processamento: {info.error}", Severity.Error);
                        StateHasChanged();
                        return;
                    }
                    else if (info.status == "processing")
                    {
                        Snackbar.Add("Ainda processando... Aguarde mais um pouco e tente novamente.", Severity.Info);
                        return;
                    }
                }

                Snackbar.Add("Nenhum resultado encontrado. Tente processar a imagem novamente.", Severity.Warning);
                hasProcessingStarted = false;
                StateHasChanged();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"C#: Erro ao verificar resultado: {ex.Message}");
                Snackbar.Add($"Erro ao verificar resultado: {ex.Message}", Severity.Error);
            }
        }

        // Método alternativo que força a recuperação
        private async Task ForceRetrieveResult()
        {
            Console.WriteLine("C#: Forçando recuperação do resultado...");

            try
            {
                // Tentar múltiplas vezes se necessário
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        var result = await JSRuntime.InvokeAsync<string>("eval", @"
                    (function() {
                        try {
                            return localStorage.getItem('imageProcessingResult') || '';
                        } catch (e) {
                            return '';
                        }
                    })()
                ");

                        Console.WriteLine($"C#: Tentativa {i + 1} - Resultado length: {result?.Length ?? 0}");

                        if (!string.IsNullOrEmpty(result) && result.StartsWith("data:image"))
                        {
                            processedImageDataUrl = result;
                            hasProcessingStarted = false;

                            // Limpar localStorage
                            await JSRuntime.InvokeVoidAsync("eval", @"
                        localStorage.removeItem('imageProcessingResult');
                        localStorage.removeItem('imageProcessingError');
                        localStorage.removeItem('imageProcessingStatus');
                        localStorage.removeItem('imageProcessingPixels');
                    ");

                            Snackbar.Add("Resultado recuperado com sucesso!", Severity.Success);
                            StateHasChanged();
                            return;
                        }

                        await Task.Delay(500); // Aguardar meio segundo antes de tentar novamente
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"C#: Erro na tentativa {i + 1}: {ex.Message}");
                        if (i == 2) throw; // Re-throw na última tentativa
                    }
                }

                Snackbar.Add("Não foi possível recuperar o resultado. Tente processar novamente.", Severity.Warning);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"C#: Erro na recuperação forçada: {ex.Message}");
                Snackbar.Add($"Erro: {ex.Message}", Severity.Error);
            }
        }

        // Classe para deserializar info do localStorage
        private class LocalStorageInfo
        {
            public string status { get; set; } = "";
            public bool hasResult { get; set; }
            public int resultLength { get; set; }
            public string pixels { get; set; } = "0";
            public string error { get; set; } = "";
        }

        private async Task DownloadImage()
        {
            if (string.IsNullOrEmpty(processedImageDataUrl))
            {
                Snackbar.Add("Nenhuma imagem processada disponível para download.", Severity.Warning);
                return;
            }

            try
            {
                Console.WriteLine("C#: Iniciando download...");

                var originalName = fileName ?? "imagem";
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalName);
                var downloadName = $"{nameWithoutExtension}_sem_fundo.png";

                await JSRuntime.InvokeVoidAsync("eval", $@"
            (function() {{
                try {{
                    const link = document.createElement('a');
                    link.href = '{processedImageDataUrl}';
                    link.download = '{downloadName}';
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    console.log('Download iniciado: {downloadName}');
                }} catch (e) {{
                    console.error('Erro no download:', e);
                    throw e;
                }}
            }})()
        ");

                Snackbar.Add("Download iniciado!", Severity.Success);
                Console.WriteLine($"C#: Download iniciado: {downloadName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"C#: Erro no download: {ex.Message}");
                Snackbar.Add($"Erro ao baixar imagem: {ex.Message}", Severity.Error);
            }
        }

        private async Task DownloadImageDirectly()
        {
            try
            {
                Console.WriteLine("C#: Download direto...");

                await JSRuntime.InvokeVoidAsync("eval", $@"
            (function() {{
                const dataUrl = '{processedImageDataUrl}';
                if (dataUrl && dataUrl.startsWith('data:image')) {{
                    const link = document.createElement('a');
                    link.href = dataUrl;
                    link.download = 'imagem_sem_fundo.png';
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                    console.log('Download direto realizado');
                }} else {{
                    throw new Error('DataURL inválido');
                }}
            }})()
        ");

                Snackbar.Add("Download direto realizado!", Severity.Success);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"C#: Erro no download direto: {ex.Message}");
                Snackbar.Add($"Erro: {ex.Message}", Severity.Error);
            }
        }

        private async Task EmergencyDownload()
        {
            Console.WriteLine("C#: Download de emergência...");

            try
            {
                // Verificar se há algo no localStorage
                var result = await JSRuntime.InvokeAsync<string>("eval", @"
            (function() {
                try {
                    const stored = localStorage.getItem('imageProcessingResult');
                    if (stored && stored.startsWith('data:image')) {
                        const link = document.createElement('a');
                        link.href = stored;
                        link.download = 'emergencia_imagem_sem_fundo.png';
                        document.body.appendChild(link);
                        link.click();
                        document.body.removeChild(link);
                        return 'Download de emergência realizado';
                    } else {
                        return 'Nenhum resultado encontrado no localStorage';
                    }
                } catch (e) {
                    return 'Erro: ' + e.message;
                }
            })()
        ");

                Snackbar.Add(result, result.Contains("realizado") ? Severity.Success : Severity.Warning);
                Console.WriteLine($"C#: Emergency download result: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"C#: Erro no download de emergência: {ex.Message}");
                Snackbar.Add($"Erro: {ex.Message}", Severity.Error);
            }
        }

        

        private void GoToNotLoadingState()
        {
            isLoading = false;
            loadingMessage = "A processar...";
            hasProcessingStarted = false;
            StateHasChanged();
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("eval", @"
                    if (typeof localStorage !== 'undefined') {
                        localStorage.removeItem('imageProcessingResult');
                        localStorage.removeItem('imageProcessingError');
                        localStorage.removeItem('imageProcessingStatus');
                        localStorage.removeItem('imageProcessingPixels');
                    }
                ");
            }
            catch
            {
                // Ignorar erros durante dispose
            }
        }

        private string NormalizeColorHex(string colorHex)
        {
            if (string.IsNullOrWhiteSpace(colorHex))
                return "#FFFFFF";

            var normalized = colorHex.Trim();

            // Garantir que comece com #
            if (!normalized.StartsWith("#"))
                normalized = "#" + normalized;

            // Se tem 8 caracteres (inclui alpha), remover os 2 últimos
            if (normalized.Length == 9)
            {
                normalized = normalized.Substring(0, 7);
                Console.WriteLine($"C#: Cor normalizada de {colorHex} para {normalized}");
            }

            // Validar formato final
            if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^#[0-9A-Fa-f]{6}$"))
            {
                Console.WriteLine($"C#: Cor inválida {colorHex}, usando padrão #FFFFFF");
                return "#FFFFFF";
            }

            return normalized.ToUpperInvariant();
        }
    }
}