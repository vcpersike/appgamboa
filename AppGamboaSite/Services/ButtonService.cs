using AppGamboaSite.Shared.Models;
using AppGamboaSite.Shared.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGamboaSite.Services
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
