using AppGamboaSite.Shared.Models;
using AppGamboaSite.Shared.Services;

namespace AppGamboaSite.Web.Services
{
    public class ButtonService : IButtonService
    {
        public ButtonModel GetButtonData()
        {
            return new ButtonModel
            {
                Label = "Button AppGamboaSite",
                CssClass = "btn-primary"
            };
        }

        public async Task<string> GetButtonTextAsync()
        {
            // Simulação de chamada assíncrona (pode ser API, Database, etc.)
            await Task.Delay(500);
            return "Texto do Botão - MudBlazor";
        }
    }
}
