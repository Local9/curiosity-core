using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleMenu
    {
        private UIMenu menuVehicleRemote;
        private VehicleRemoteMenu _VehicleRemoteMenu = new VehicleRemoteMenu();

        static List<dynamic> lockList = new List<dynamic>() { "Allow Everyone", "Lock for Everyone", "Passengers Only" };
        UIMenuListItem uiVehicleLock = new UIMenuListItem("Lock", lockList, 0);

        public UIMenu CreateMenu(UIMenu menu)
        {
            menuVehicleRemote = InteractionMenu.MenuPool.AddSubMenu(menu, "Vehicle Remote Functions");
            _VehicleRemoteMenu.CreateMenu(menuVehicleRemote);

            menu.OnListChange += Menu_OnListChange;

            menu.MouseControlsEnabled = false;
            menu.MouseEdgeEnabled = false;

            return menu;
        }

        private async void Menu_OnListChange(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            if (listItem == uiVehicleLock)
            {
                string type = lockList[newIndex];

                if (type == "Allow Everyone")
                    UnlockAllVehicles();

                if (type == "Lock for Everyone")
                    LockAllVehicles();

                if (type == "Passengers Only")
                    PassengersOnly();

                await BaseScript.Delay(150);
            }
        }

        private void UnlockAllVehicles()
        {
            if (Cache.PersonalVehicle is not null)
            {
                Cache.PersonalVehicle.Vehicle.LockStatus = VehicleLockStatus.Unlocked;
                SetVehicleExclusiveDriver_2(Cache.PersonalVehicle.Vehicle.Handle, Game.PlayerPed.Handle, 0);
            }

            if (Cache.PersonalPlane is not null)
            {
                Cache.PersonalPlane.Vehicle.LockStatus = VehicleLockStatus.Unlocked;
                SetVehicleExclusiveDriver_2(Cache.PersonalPlane.Vehicle.Handle, Game.PlayerPed.Handle, 0);
            }

            if (Cache.PersonalHelicopter is not null)
            {
                Cache.PersonalHelicopter.Vehicle.LockStatus = VehicleLockStatus.Unlocked;
                SetVehicleExclusiveDriver_2(Cache.PersonalHelicopter.Vehicle.Handle, Game.PlayerPed.Handle, 0);
            }

            if (Cache.PersonalBoat is not null)
            {
                Cache.PersonalBoat.Vehicle.LockStatus = VehicleLockStatus.Unlocked;
                SetVehicleExclusiveDriver_2(Cache.PersonalBoat.Vehicle.Handle, Game.PlayerPed.Handle, 0);
            }
        }

        private void LockAllVehicles()
        {
            if (Cache.PersonalVehicle is not null)
                Cache.PersonalVehicle.Vehicle.LockStatus = VehicleLockStatus.Locked;

            if (Cache.PersonalPlane is not null)
                Cache.PersonalPlane.Vehicle.LockStatus = VehicleLockStatus.Locked;

            if (Cache.PersonalHelicopter is not null)
                Cache.PersonalHelicopter.Vehicle.LockStatus = VehicleLockStatus.Locked;

            if (Cache.PersonalBoat is not null)
                Cache.PersonalBoat.Vehicle.LockStatus = VehicleLockStatus.Locked;
        }

        private void PassengersOnly()
        {
            if (Cache.PersonalVehicle is not null)
                SetVehicleExclusiveDriver_2(Cache.PersonalVehicle.Vehicle.Handle, Game.PlayerPed.Handle, 1);

            if (Cache.PersonalPlane is not null)
                SetVehicleExclusiveDriver_2(Cache.PersonalPlane.Vehicle.Handle, Game.PlayerPed.Handle, 1);

            if (Cache.PersonalHelicopter is not null)
                SetVehicleExclusiveDriver_2(Cache.PersonalHelicopter.Vehicle.Handle, Game.PlayerPed.Handle, 1);

            if (Cache.PersonalBoat is not null)
                SetVehicleExclusiveDriver_2(Cache.PersonalBoat.Vehicle.Handle, Game.PlayerPed.Handle, 1);
        }
    }
}
