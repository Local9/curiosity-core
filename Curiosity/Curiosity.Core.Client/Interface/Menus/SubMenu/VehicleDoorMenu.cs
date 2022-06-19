using Curiosity.Systems.Library.Enums;
using NativeUI;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu
{
    class VehicleDoorMenu
    {
        UIMenu baseMenu;
        int baseIndex = 0;

        UIMenuItem openAll = new UIMenuItem("Open All Doors", "Open all vehicle doors.");
        UIMenuItem closeAll = new UIMenuItem("Close All Doors", "Close all vehicle doors.");

        UIMenuItem LF = new UIMenuItem("Left Front Door", "Open/close the left front door.");
        UIMenuItem RF = new UIMenuItem("Right Front Door", "Open/close the right front door.");
        UIMenuItem LR = new UIMenuItem("Left Rear Door", "Open/close the left rear door.");
        UIMenuItem RR = new UIMenuItem("Right Rear Door", "Open/close the right rear door.");

        UIMenuItem HD = new UIMenuItem("Hood", "Open/close the hood.");
        UIMenuItem TR = new UIMenuItem("Trunk", "Open/close the trunk.");

        UIMenuItem E1 = new UIMenuItem("Extra 1", "Open/close the extra door (#1). Note this door is not present on most vehicles.");
        UIMenuItem E2 = new UIMenuItem("Extra 2", "Open/close the extra door (#2). Note this door is not present on most vehicles.");
        UIMenuItem BB = new UIMenuItem("Bomb Bay", "Open/close the bomb bay. Only available on some planes.");
        //var doors = new List<string>() { "Front Left", "Front Right", "Rear Left", "Rear Right", "Hood", "Trunk", "Extra 1", "Extra 2" };
        //MenuListItem removeDoorList = new MenuListItem("Remove Door", doors, 0, "Remove a specific vehicle door completely.");
        //MenuCheckboxItem deleteDoors = new MenuCheckboxItem("Delete Removed Doors", "When enabled, doors that you remove using the list above will be deleted from the world. If disabled, then the doors will just fall on the ground.", false);

        internal void CreateMenu(UIMenu menuVehicleDoors)
        {
            baseMenu = menuVehicleDoors;

            baseMenu.AddItem(LF);
            baseMenu.AddItem(RF);
            baseMenu.AddItem(LR);
            baseMenu.AddItem(RR);

            baseMenu.AddItem(HD);
            baseMenu.AddItem(TR);

            baseMenu.AddItem(E1);
            baseMenu.AddItem(E2);
            baseMenu.AddItem(BB);

            baseMenu.AddItem(openAll);
            baseMenu.AddItem(closeAll);

            baseMenu.OnItemSelect += BaseMenu_OnItemSelect;
            baseMenu.OnIndexChange += BaseMenu_OnIndexChange;
        }

        private void BaseMenu_OnIndexChange(UIMenu sender, int newIndex)
        {
            baseIndex = newIndex;
        }

        private void BaseMenu_OnItemSelect(UIMenu sender, UIMenuItem item, int index)
        {
            // Get the vehicle.
            Vehicle veh = Game.PlayerPed.CurrentVehicle;
            // If the player is in a vehicle, it's not dead and the player is the driver, continue.
            if (veh != null && veh.Exists() && !veh.IsDead && veh.Driver == Game.PlayerPed)
            {
                // If button 0-5 are pressed, then open/close that specific index/door.
                if (baseIndex < 8)
                {
                    // If the door is open.
                    bool open = GetVehicleDoorAngleRatio(veh.Handle, baseIndex) > 0.1f ? true : false;

                    if (open)
                    {
                        // Close the door.
                        SetVehicleDoorShut(veh.Handle, baseIndex, false);
                    }
                    else
                    {
                        // Open the door.
                        SetVehicleDoorOpen(veh.Handle, baseIndex, false, false);
                    }
                }
                // If the index >= 8, and the button is "openAll": open all doors.
                else if (item == openAll)
                {
                    // Loop through all doors and open them.
                    for (var door = 0; door < 8; door++)
                    {
                        SetVehicleDoorOpen(veh.Handle, door, false, false);
                    }
                    if (veh.HasBombBay) veh.OpenBombBay();
                }
                // If the index >= 8, and the button is "closeAll": close all doors.
                else if (item == closeAll)
                {
                    // Close all doors.
                    SetVehicleDoorsShut(veh.Handle, false);
                    if (veh.HasBombBay) veh.CloseBombBay();
                }
                // If bomb bay doors button is pressed and the vehicle has bomb bay doors.
                else if (item == BB && veh.HasBombBay)
                {
                    bool bombBayOpen = AreBombBayDoorsOpen(veh.Handle);
                    // If open, close them.
                    if (bombBayOpen)
                        veh.CloseBombBay();
                    // Otherwise, open them.
                    else
                        veh.OpenBombBay();
                }
            }
            else
            {
                Notify.Alert(CommonErrors.NoVehicle, placeholderValue: "to open/close a vehicle door");
            }
        }
    }
}
