using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGamboaSite.Shared.Models
{
    public class CompanyDetails
    {
        public string Title { get; set; } = string.Empty;
        public string FullDescription { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new List<string>();
    }
}
