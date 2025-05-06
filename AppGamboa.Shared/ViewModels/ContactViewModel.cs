using System.Threading.Tasks;
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
            ContactForm = new ContactFormModel();
            IsSubmitting = false;
        }

        public ContactFormModel ContactForm { get; set; }

        public bool IsSubmitting { get; set; }

        public async Task SubmitContactForm()
        {
            if (ContactForm != null)
            {
                await _contactService.SendContactMessage(ContactForm);
                ContactForm = new ContactFormModel();
            }
        }
    }
}