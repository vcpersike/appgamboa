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
        public ButtonModel GetButtonData()
        {
            return new ButtonModel
            {
                Label = "Button AppGamboaSite",
                CssClass = "btn-primary"
            };
        }
    }
}
