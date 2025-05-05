using AppGamboa.Shared.Models;
using AppGamboa.Shared.Services;

namespace AppGamboa.Web.Services
{
    public class ContactService : IContactService
    {
        public Task SendContactAsync(ContactFormModel form)
        {
            // Simula envio com delay e exibe dados no console
            Console.WriteLine("=== Contato recebido ===");
            Console.WriteLine($"Nome: {form.Name}");
            Console.WriteLine($"Email: {form.Email}");
            Console.WriteLine($"Mensagem: {form.Message}");
            Console.WriteLine("========================");

            // Simula operação assíncrona
            return Task.Delay(500);
        }
    }
}
