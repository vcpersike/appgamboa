using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Implementation;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private DotNetObjectReference<ImageRemover>? dotNetObjectReference;

        private const string CanvasId = "imageCanvasBlazorMud";
        private const int MaxCanvasWidth = 600;
        private const int MaxCanvasHeight = 450;

        protected override async Task OnInitializedAsync()
        {
            dotNetObjectReference = DotNetObjectReference.Create(this);
            bool workerInitialized = false;
            try
            {
                workerInitialized = await JSRuntime.InvokeAsync<bool>("blazorImageUtils.initImageProcessor", dotNetObjectReference);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao inicializar worker: {ex.Message}");
                Snackbar.Add("Falha crítica ao configurar o processador de imagem em segundo plano.", Severity.Error);
            }

            if (!workerInitialized && !IsDisposed) // Verifica IsDisposed para evitar erros durante o dispose
            {
                Snackbar.Add("Web Workers não são suportados ou falharam ao inicializar.", Severity.Error);
            }
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

                await using var memoryStream = new MemoryStream();
                await selectedFile.OpenReadStream(maxAllowedSize: maxFileSize).CopyToAsync(memoryStream);

                var buffer = memoryStream.ToArray();
                originalImageDataUrl = $"data:{selectedFile.ContentType};base64,{Convert.ToBase64String(buffer)}";
                StateHasChanged();

                await Task.Delay(100);

                await JSRuntime.InvokeVoidAsync("blazorImageUtils.loadImageOnCanvas", CanvasId, originalImageDataUrl, MaxCanvasWidth, MaxCanvasHeight);
                Snackbar.Add("Imagem carregada. Pode ajustar as opções e remover o fundo.", Severity.Info);
            }
            catch (JSException jsEx)
            {
                Console.WriteLine($"JavaScript error in loadImageOnCanvas: {jsEx.Message}");
                Snackbar.Add($"Erro ao exibir a imagem no canvas: {jsEx.Message}", Severity.Error);
                originalImageDataUrl = null;
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"IO Error loading file: {ioEx.Message}");
                Snackbar.Add($"Erro de leitura do arquivo: {ioEx.Message}.", Severity.Error);
                originalImageDataUrl = null;
                fileName = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading file: {ex.Message}");
                if (ex is TaskCanceledException || ex.InnerException is TimeoutException)
                {
                    Snackbar.Add("Operação de carregamento de arquivo cancelada ou excedeu o tempo limite.", Severity.Error);
                }
                else
                {
                    Snackbar.Add($"Erro ao carregar imagem: {ex.Message}", Severity.Error);
                }
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
            if (isPickingColor)
            {
                Snackbar.Add("Modo de seleção de cor ativado. Clique na imagem.", Severity.Info, config => { config.DuplicatesBehavior = SnackbarDuplicatesBehavior.Allow; });
            }
            else
            {
                Snackbar.Add("Modo de seleção de cor desativado.", Severity.Info, config => { config.DuplicatesBehavior = SnackbarDuplicatesBehavior.Allow; });
            }
        }

        private async Task HandleCanvasClick(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
        {
            if (isPickingColor && !string.IsNullOrEmpty(originalImageDataUrl))
            {
                try
                {
                    string? pickedColor = await JSRuntime.InvokeAsync<string?>("blazorImageUtils.pickColorFromCanvas", CanvasId, e.ClientX, e.ClientY);
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
                catch (JSException jsEx)
                {
                    Console.WriteLine($"Error picking color from canvas (JS): {jsEx.Message}");
                    Snackbar.Add("Erro ao comunicar com o canvas para selecionar cor.", Severity.Error);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error picking color from canvas: {ex.Message}");
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

            isLoading = true;
            loadingMessage = "A processar imagem em segundo plano...";
            processedImageDataUrl = null;
            StateHasChanged();

            try
            {
                await JSRuntime.InvokeVoidAsync("blazorImageUtils.loadImageOnCanvas", CanvasId, originalImageDataUrl, MaxCanvasWidth, MaxCanvasHeight);
                await Task.Delay(50);

                await JSRuntime.InvokeVoidAsync("blazorImageUtils.processImageWithWorker", CanvasId, colorToReplaceHex, tolerance);
                // A Snackbar de "iniciado" é opcional, pois o resultado virá do worker
                // Snackbar.Add("Processamento iniciado...", Severity.Info);
            }
            catch (JSException jsEx)
            {
                Console.WriteLine($"JS Error starting worker task: {jsEx.Message}");
                Snackbar.Add($"Erro ao iniciar processamento em segundo plano: {jsEx.Message}", Severity.Error);
                GoToNotLoadingState();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting image processing with worker: {ex.Message}");
                Snackbar.Add($"Erro ao iniciar processamento: {ex.Message}", Severity.Error);
                GoToNotLoadingState();
            }
        }

        [JSInvokable]
        public async Task HandleWorkerSuccess(byte[] processedData, int width, int height)
        {
            try
            {
                // Chamando JS para reconstruir ImageData no canvas
                processedImageDataUrl = await JSRuntime.InvokeAsync<string>(
                    "blazorImageUtils.putImageDataOnCanvasAndGetDataURL",
                    CanvasId,
                    processedData,
                    width,
                    height
                );

                if (!string.IsNullOrEmpty(processedImageDataUrl))
                {
                    Snackbar.Add("Fundo removido com sucesso! Pode baixar a imagem.", Severity.Success);
                }
                else
                {
                    Snackbar.Add("Falha ao obter URL da imagem processada após o worker.", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleWorkerSuccess: {ex.Message}");
                Snackbar.Add($"Erro ao finalizar processamento da imagem: {ex.Message}", Severity.Error);
            }
            finally
            {
                GoToNotLoadingState();
            }
        }

        [JSInvokable]
        public Task HandleWorkerError(string errorMessage)
        {
            Console.WriteLine($"Worker Error: {errorMessage}");
            Snackbar.Add($"Erro no processamento em segundo plano: {errorMessage}", Severity.Error);
            GoToNotLoadingState();
            return Task.CompletedTask;
        }

        private void GoToNotLoadingState()
        {
            isLoading = false;
            loadingMessage = "A processar..."; // Reset default message
            StateHasChanged();
        }

        private bool IsDisposed = false;
        public async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            if (dotNetObjectReference != null)
            {
                dotNetObjectReference.Dispose();
            }
            try
            {
                await JSRuntime.InvokeVoidAsync("blazorImageUtils.terminateWorker");
            }
            catch (JSDisconnectedException)
            {
                // Ocorre se o utilizador navegar para fora da página antes do dispose ser chamado
                // ou se a conexão JS for perdida. Pode ser ignorado com segurança.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao terminar worker: {ex.Message}");
            }
        }
    }
}
