using AppGamboaSite.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppGamboaSite.Shared.Services
{
    public interface IMenuService
    {
        List<MenuItemModel> GetMenuItems();
    }
}
