using AppGamboaSite.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGamboaSite.Shared.ViewModels
{
    public class SideMenuViewModel
    {
        public bool IsOpen { get; set; } = true;
        public List<MenuItemModel> MenuItems { get; set; } = new List<MenuItemModel>();

        public void SelectItem(MenuItemModel item)
        {
            Console.WriteLine($"Item Selecionado: {item.Text}");
        }
    }
}
