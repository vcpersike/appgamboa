using AppGamboa.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGamboa.Shared.Services
{
    public interface IContactService
    {
        Task SendContactAsync(ContactFormModel form);
    }

}
