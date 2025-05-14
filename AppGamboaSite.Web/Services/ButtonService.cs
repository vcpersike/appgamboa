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
    }
}
