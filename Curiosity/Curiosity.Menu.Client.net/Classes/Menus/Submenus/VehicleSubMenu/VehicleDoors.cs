using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Extensions;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Menus.Client.net.Classes.Menus.Submenus.VehicleSubMenu
{
    class VehicleDoors
    {
        static Client client = Client.GetInstance();

        static Menu VehicleDoorMenu;

        static bool ToggleBackDoorState = false;

        static Vehicle AttachedVehicle;

        static List<VehicleDoorIndex> VehicleDoorValues = Enum.GetValues(typeof(VehicleDoorIndex)).OfType<VehicleDoorIndex>().ToList();
        static List<string> VehicleDoorNames = Enum.GetNames(typeof(VehicleDoorIndex)).Select(d => d.AddSpacesToCamelCase()).ToList();

        static public void SetupMenu()
        {
            if (VehicleDoorMenu == null)
            {
                VehicleDoorMenu = new Menu("Doors");

                VehicleDoorMenu.OnMenuOpen += VehicleDoorMenu_OnMenuOpen;
                VehicleDoorMenu.OnMenuClose += VehicleDoorMenu_OnMenuClose;
                VehicleDoorMenu.OnCheckboxChange += VehicleDoorMenu_OnCheckboxChange;
            }
            VehicleMenu.AddSubMenu(VehicleDoorMenu);
        }

        private static void VehicleDoorMenu_OnCheckboxChange(Menu menu, MenuCheckboxItem menuItem, int itemIndex, bool newCheckedState)
        {
            VehicleDoor door = null;

            if (menuItem.ItemData.Type == 1)
                door = Client.CurrentVehicle.Doors[menuItem.ItemData.index];

            if (menuItem.ItemData.Type == 2)
                door = AttachedVehicle.Doors[menuItem.ItemData.index];

            if (menuItem.ItemData.Type == 3)
            {
                OnToggleBackDoors();
            }
            else
            {
                if (menuItem.Checked) door.Open(); else door.Close((menuItem.ItemData.Type == 2));
            }
        }

        private static void VehicleDoorMenu_OnMenuClose(Menu menu)
        {
            MenuBase.MenuOpen(false);
        }

        private static void VehicleDoorMenu_OnMenuOpen(Menu menu)
        {
            MenuBase.MenuOpen(true);
            VehicleDoorMenu.ClearMenuItems();

            VehicleDoor[] attachedVehicleDoors = null;
            bool addBothBackDoors = false;

            VehicleDoor[] doors = Client.CurrentVehicle.Doors.GetAll();

            doors.ToList().ForEach(door =>
            {
                if (!door.IsBroken)
                {
                    VehicleDoorMenu.AddMenuItem(new MenuCheckboxItem($"Open {door.Index.ToString().AddSpacesToCamelCase()}")
                    {
                        Checked = door.IsOpen,
                        ItemData = new MyDoor() { Type = 1, index = door.Index }
                    });

                    if (door.Index == VehicleDoorIndex.BackLeftDoor || door.Index == VehicleDoorIndex.BackRightDoor)
                    {
                        addBothBackDoors = true;
                    }
                }
            });

            if (addBothBackDoors)
            {
                VehicleDoorMenu.AddMenuItem(new MenuCheckboxItem($"Open Both Back Doors")
                {
                    Checked = ToggleBackDoorState,
                    ItemData = new MyDoor() { Type = 3, index = VehicleDoorIndex.BackRightDoor }
                });
            }

            if (Client.CurrentVehicle != null)
            {
                int trailerHandle = 0;
                API.GetVehicleTrailerVehicle(Client.CurrentVehicle.Handle, ref trailerHandle);

                if (trailerHandle != 0)
                {
                    AttachedVehicle = new Vehicle(trailerHandle);
                    attachedVehicleDoors = AttachedVehicle.Doors.GetAll();

                    attachedVehicleDoors.ToList().ForEach(door =>
                    {
                        if (!door.IsBroken)
                            VehicleDoorMenu.AddMenuItem(new MenuCheckboxItem($"Open {door.Index.ToString().AddSpacesToCamelCase()}")
                            {
                                Checked = door.IsOpen,
                                ItemData = new MyDoor() { Type = 2, index = door.Index }
                            });
                    });
                }
                else
                {
                    AttachedVehicle = null;
                }
            }
        }

        static void OnToggleBackDoors()
        {
            if (Client.CurrentVehicle == null) return;

            ToggleBackDoorState = !ToggleBackDoorState;

            if (ToggleBackDoorState)
            {
                API.SetVehicleDoorOpen(Client.CurrentVehicle.Handle, 2, false, false);
                API.SetVehicleDoorOpen(Client.CurrentVehicle.Handle, 3, false, false);
            }
            else
            {
                API.SetVehicleDoorShut(Client.CurrentVehicle.Handle, 2, false);
                API.SetVehicleDoorShut(Client.CurrentVehicle.Handle, 3, false);
            }
        }
    }
    class MyDoor
    {
        public int Type;
        public VehicleDoorIndex index;
    }
}
