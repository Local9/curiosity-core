using CitizenFX.Core;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Utils;
using NativeUI;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    internal class PoliceMenu
    {
        private UIMenu menu;
        EventSystem EventSystem => EventSystem.GetModule();

        private UIMenu playerListMenu;
        private PoliceSubMenu.PolicePlayerListMenu _playerListMenu = new PoliceSubMenu.PolicePlayerListMenu();
        JobManager jobManager => JobManager.GetModule();

        public UIMenu CreateMenu(UIMenu m)
        {
            menu = m;

            playerListMenu = InteractionMenu.MenuPool.AddSubMenu(m, "Nearby Players");
            _playerListMenu.CreateMenu(playerListMenu);

            return menu;
        }

        public void Init()
        {
            PluginManager.Instance.AttachTickHandler(CheckControls);
        }

        public void Dispose()
        {
            PluginManager.Instance.DetachTickHandler(CheckControls);
        }

        private async Task CheckControls()
        {
            if (!jobManager.IsOfficer)
            {
                Dispose();
            }

            if (!Game.IsPaused
                && !IsPauseMenuRestarting()
                && IsScreenFadedIn()
                && !IsPlayerSwitchInProgress()
                && Cache.Character.MarkedAsRegistered)
            {
                if (InteractionMenu.MenuPool.IsAnyMenuOpen() && menu.Visible)
                {
                    if (ControlHelper.IsControlJustPressed(Control.Context, true, ControlModifier.Alt))
                    {
                        menu.Visible = false;
                        InteractionMenu.MenuPool.CloseAllMenus();
                    }
                }
                else if (!menu.Visible)
                {
                    if (ControlHelper.IsControlJustPressed(Control.Context, true, ControlModifier.Alt))
                    {
                        menu.Visible = true;
                    }
                }
            }
        }
    }
}
