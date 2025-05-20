using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGamboaSite.Shared.Views.Pages
{
    public partial class Landpage
    {
        public async Task ScrollToElement(string elementId)
        {
            await JS.InvokeVoidAsync("scrollToElement", elementId);
        }
    }
}
