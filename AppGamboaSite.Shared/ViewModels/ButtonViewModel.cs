
using AppGamboaSite.Shared.Models;
using AppGamboaSite.Shared.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AppGamboaSite.ViewModels
{
    public class ButtonViewModel : INotifyPropertyChanged
    {
        private readonly IButtonService _buttonService;

        public ButtonModel Button { get; private set; }

        public ButtonViewModel(IButtonService buttonService)
        {
            _buttonService = buttonService;
            Button = new ButtonModel { Text = "Carregando..." };
        }

        public async Task LoadButtonText()
        {
            var text = await _buttonService.GetButtonTextAsync();
            Button.Text = text;
            OnPropertyChanged(nameof(Button));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
