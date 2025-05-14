using AppGamboaSite.Shared.Models;
using AppGamboaSite.Shared.Services;
using Microsoft.AspNetCore.Components;

namespace AppGamboa.Shared.ViewModels
{
    public class ButtonViewModel : ComponentBase
    {
        [Inject]
        public IButtonService ButtonService { get; set; }

        public ButtonModel ButtonData { get; set; }

        protected override void OnInitialized()
        {
            ButtonData = ButtonService.GetButtonData();
        }
    }
}
