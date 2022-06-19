using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleWindowMenu
    {
        UIMenu baseMenu;

        UIMenuItem fwu = new UIMenuItem("~y~↑~s~ Roll Front Windows Up", "Roll both front windows up.");
        UIMenuItem fwd = new UIMenuItem("~o~↓~s~ Roll Front Windows Down", "Roll both front windows down.");
        UIMenuItem rwu = new UIMenuItem("~y~↑~s~ Roll Rear Windows Up", "Roll both rear windows up.");
        UIMenuItem rwd = new UIMenuItem("~o~↓~s~ Roll Rear Windows Down", "Roll both rear windows down.");

        internal void CreateMenu(UIMenu menuVehicleWindows)
        {
            baseMenu = menuVehicleWindows;

            baseMenu.AddItem(fwu);
            baseMenu.AddItem(fwd);
            baseMenu.AddItem(rwu);
            baseMenu.AddItem(rwd);

            baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
        }

        private void BaseMenu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            Vehicle veh = Game.PlayerPed.CurrentVehicle;
            if (veh != null && veh.Exists() && !veh.IsDead)
            {
                if (selectedItem == fwu)
                {
                    RollUpWindow(veh.Handle, 0);
                    RollUpWindow(veh.Handle, 1);
                }
                else if (selectedItem == fwd)
                {
                    RollDownWindow(veh.Handle, 0);
                    RollDownWindow(veh.Handle, 1);
                }
                else if (selectedItem == rwu)
                {
                    RollUpWindow(veh.Handle, 2);
                    RollUpWindow(veh.Handle, 3);
                }
                else if (selectedItem == rwd)
                {
                    RollDownWindow(veh.Handle, 2);
                    RollDownWindow(veh.Handle, 3);
                }
            }
        }
    }
}
