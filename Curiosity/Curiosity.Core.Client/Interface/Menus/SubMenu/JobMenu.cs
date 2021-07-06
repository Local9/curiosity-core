using NativeUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class JobMenu
    {
        private UIMenu jobMenu;

        public UIMenu CreateMenu(UIMenu menu)
        {
            menu.MouseControlsEnabled = false;
            menu.MouseEdgeEnabled = false;

            return menu;
        }
    }
}
