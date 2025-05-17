using AppGamboaSite.Shared.Models;
using AppGamboaSite.Shared.Services;

namespace AppGamboaSite.Web.Services
{
    public class MenuService : IMenuService
    {
        public List<MenuItemModel> GetMenuItems()
        {
            return new List<MenuItemModel>
        {
            new MenuItemModel { Icon = "home", Text = "Home", Route = "/home" },
            new MenuItemModel { Icon = "settings", Text = "Configurações", Route = "/settings" },
            new MenuItemModel { Icon = "info", Text = "Sobre", Route = "/about" }
        };
        }
    }
}
