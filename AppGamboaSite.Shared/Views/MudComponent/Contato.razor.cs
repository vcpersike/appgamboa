using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGamboaSite.Shared.Views.MudComponent
{
    public partial class  Contato
    {
        private MudForm form;
        private ContatoModel model = new ContatoModel();

        private async Task Submit()
        {
            await form.Validate();
            if (form.IsValid)
            {
                Console.WriteLine($"Nome: {model.Name}");
                Console.WriteLine($"Email: {model.Email}");
                Console.WriteLine($"Telefone: {model.Phone}");
            }
        }

        public class ContatoModel
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
        }
    }
}
