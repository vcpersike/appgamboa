using AppGamboa.Shared.Models;
using AppGamboa.Shared.Services;

namespace AppGamboa.Services
{
    public class ContactService : IContactService
    {
        public async Task SendContactAsync(ContactFormModel form)
        {
            // Simula envio de contato
            await Task.Delay(500); // Pode ser substituído por envio real (ex: API, Email, etc)

            System.Diagnostics.Debug.WriteLine("=== Contato recebido ===");
            System.Diagnostics.Debug.WriteLine($"Nome: {form.Name}");
            System.Diagnostics.Debug.WriteLine($"Email: {form.Email}");
            System.Diagnostics.Debug.WriteLine($"Mensagem: {form.Message}");
            System.Diagnostics.Debug.WriteLine("========================");
        }
    }
}
