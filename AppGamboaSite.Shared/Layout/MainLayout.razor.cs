using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGamboaSite.Shared.Layout
{
    public partial class MainLayout
    {
        private bool _drawerOpen = false;
        private Anchor _anchor;
        private bool _overlayAutoClose = true;

        private List<BreadcrumbItem> _items = new()
    {
        new("Sobre", href: "#sobre", icon: Icons.Material.Filled.Info),
        new("Serviços", href: "#servicos", icon: Icons.Material.Filled.Build),
        new("Contato", href: "#contato", icon: Icons.Material.Filled.Email),
        new("Image", href: "/image-remover", icon: Icons.Material.Filled.Image)
    };

        private void ToggleDrawer(Anchor anchor)
        {
            _anchor = anchor;
            _drawerOpen = !_drawerOpen;
            StateHasChanged();
        }
    }
}
