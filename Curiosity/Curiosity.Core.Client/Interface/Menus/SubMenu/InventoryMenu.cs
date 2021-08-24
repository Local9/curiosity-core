using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NativeUI;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class InventoryMenu
    {
        UIMenu baseMenu;

        /*
         * Health Kits
         * Armor Kits
         * Scuba Kit
         * Repair Kits
         * 
         * */

        public void CreateMenu(UIMenu menu)
        {
            baseMenu = menu;
        }
    }
}
