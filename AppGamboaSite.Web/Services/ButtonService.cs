using AppGamboaSite.Shared.Models;
using AppGamboaSite.Shared.Services;

namespace AppGamboaSite.Web.Services
{
    public class ButtonService : IButtonService
    {
        public async Task<string> GetButtonTextAsync()
        {
            await Task.Delay(500);
            return "Texto do Botão - MudBlazor";
        }
    }
}
