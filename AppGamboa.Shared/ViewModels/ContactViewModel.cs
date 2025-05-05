using AppGamboa.Shared.Models;
using AppGamboa.Shared.Services;

namespace AppGamboa.Shared.ViewModels
{
    public class ContactViewModel
    {
        private readonly IContactService _contactService;

        public ContactViewModel(IContactService contactService)
        {
            _contactService = contactService;
            Form = new ContactFormModel();
        }

        public ContactFormModel Form { get; set; }

        public string StatusMessage { get; set; } = string.Empty;

        public async Task SendAsync()
        {
            await _contactService.SendContactAsync(Form);
            StatusMessage = "Mensagem enviada com sucesso!";
        }
    }
}
