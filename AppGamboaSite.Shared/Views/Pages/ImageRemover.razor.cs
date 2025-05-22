using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

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

            var normalizedColor = NormalizeColorHex(colorToReplaceHex);

            isLoading = true;
            loadingMessage = "A processar imagem...";
            processedImageDataUrl = null;
            hasProcessingStarted = false;
            StateHasChanged();

            try
            {
                await JSRuntime.InvokeVoidAsync("imageUtils.loadImageOnCanvas", CanvasId, originalImageDataUrl, MaxCanvasWidth, MaxCanvasHeight);
                await Task.Delay(200);

                var jsResult = await JSRuntime.InvokeAsync<string>("imageUtils.processImageDirect", CanvasId, normalizedColor, tolerance);

                if (jsResult == "SUCCESS")
                {
                    hasProcessingStarted = true;
                    loadingMessage = "Processamento iniciado! Aguarde alguns segundos e clique em 'Verificar Resultado'.";
                    Snackbar.Add("Processamento iniciado com sucesso! Aguarde e depois clique em 'Verificar Resultado'.", Severity.Info);
                }
                else if (!string.IsNullOrEmpty(jsResult) && jsResult.StartsWith("data:image"))
                {
                    processedImageDataUrl = jsResult;
                    Snackbar.Add("Fundo removido com sucesso!", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Falha ao iniciar processamento.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
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

        private async Task Download()
        {

            try
            {
                var result = await JSRuntime.InvokeAsync<string>("imageUtils.downloadFromLocalStorage");

                Snackbar.Add(result, result.Contains("realizado") ? Severity.Success : Severity.Warning);
            }
            catch
            {
            }
        }

        private void GoToNotLoadingState()
        {
            isLoading = false;
            loadingMessage = "A processar...";
            hasProcessingStarted = false;
            StateHasChanged();
        }

        private string NormalizeColorHex(string colorHex)
        {
            if (string.IsNullOrWhiteSpace(colorHex))
                return "#FFFFFF";

            var normalized = colorHex.Trim();

            if (!normalized.StartsWith("#"))
                normalized = "#" + normalized;

            if (normalized.Length == 9)
            {
                normalized = normalized.Substring(0, 7);
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(normalized, @"^#[0-9A-Fa-f]{6}$"))
            {
                return "#FFFFFF";
            }

            return normalized.ToUpperInvariant();
        }
    }
}