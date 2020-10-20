using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle;
using Curiosity.Vehicles.Client.net.Classes.CurPlayer;
using Curiosity.Vehicles.Client.net.Data;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicles.Client.net.Classes.Menus
{
    class Staff
    {
        static Plugin client = Plugin.GetInstance();

        static Menu staffMenu;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicle:OpenStaffVehicles", new Action(OpenMenu));
            MenuController.DontOpenAnyMenu = true;
        }

        private async static void OpenMenu()
        {
            if (!PlayerInformation.IsStaff()) return;

            Screen.ShowNotification("Open Staff Vehicles");

            MenuController.DontOpenAnyMenu = false;

            if (staffMenu == null)
            {
                staffMenu = new Menu("Staff Menu", "Some options for fun");

                staffMenu.OnMenuOpen += Menu_OnMenuOpen;
                staffMenu.OnMenuClose += Menu_OnMenuClose;
                
                MenuController.AddMenu(staffMenu);
                MenuController.EnableMenuToggleKeyOnController = false;
                MenuController.EnableManualGCs = false;
            }

            await BaseScript.Delay(500);

            staffMenu.OpenMenu();
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;
            menu.ClearMenuItems();
        }

        private async static void Menu_OnMenuOpen(Menu menu)
        {
            SetupMenu();
            await BaseScript.Delay(100);
            MenuBaseFunctions.MenuOpen();
        }

        static void SetupMenu()
        {

            for (var vehClass = 0; vehClass < 22; vehClass++)
            {
                // Get the class name.
                string className = GetLabelText($"VEH_CLASS_{vehClass}");

                // Create a button & a menu for it, add the menu to the menu pool and add & bind the button to the menu.
                MenuItem btn = new MenuItem(className, $"Spawn a vehicle from the ~o~{className} ~s~class.")
                {
                    Label = "→→→"
                };

                Menu vehicleClassMenu = new Menu("Vehicle Spawner", className);

                vehicleClassMenu.OnMenuOpen += VehicleClassMenu_OnMenuOpen;
                vehicleClassMenu.OnMenuClose += VehicleClassMenu_OnMenuClose;

                MenuController.AddSubmenu(staffMenu, vehicleClassMenu);
                staffMenu.AddMenuItem(btn);

                MenuController.BindMenuItem(staffMenu, vehicleClassMenu, btn);

                // Create a dictionary for the duplicate vehicle names (in this vehicle class).
                var duplicateVehNames = new Dictionary<string, int>();

                #region Add vehicles per class
                // Loop through all the vehicles in the vehicle class.
                foreach (var veh in VehicleData.Vehicles.VehicleClasses[className])
                {
                    // Convert the model name to start with a Capital letter, converting the other characters to lowercase. 
                    string properCasedModelName = veh[0].ToString().ToUpper() + veh.ToLower().Substring(1);

                    // Get the localized vehicle name, if it's "NULL" (no label found) then use the "properCasedModelName" created above.
                    string vehName = GetVehDisplayNameFromModel(veh) != "NULL" ? GetVehDisplayNameFromModel(veh) : properCasedModelName;
                    string vehModelName = veh;

                    // Loop through all the menu items and check each item's title/text and see if it matches the current vehicle (display) name.
                    var duplicate = false;
                    for (var itemIndex = 0; itemIndex < vehicleClassMenu.Size; itemIndex++)
                    {
                        // If it matches...
                        if (vehicleClassMenu.GetMenuItems()[itemIndex].Text.ToString() == vehName)
                        {

                            // Check if the model was marked as duplicate before.
                            if (duplicateVehNames.Keys.Contains(vehName))
                            {
                                // If so, add 1 to the duplicate counter for this model name.
                                duplicateVehNames[vehName]++;
                            }

                            // If this is the first duplicate, then set it to 2.
                            else
                            {
                                duplicateVehNames[vehName] = 2;
                            }

                            // The model name is a duplicate, so get the modelname and add the duplicate amount for this model name to the end of the vehicle name.
                            vehName += $" ({duplicateVehNames[vehName]})";

                            // Then create and add a new button for this vehicle.

                            if (DoesModelExist(veh))
                            {
                                var vehBtn = new MenuItem(vehName) { Enabled = true, Label = $"({vehModelName.ToLower()})" };
                                vehicleClassMenu.AddMenuItem(vehBtn);
                            }
                            else
                            {
                                var vehBtn = new MenuItem(vehName, "This vehicle is not available because the model could not be found in your game files. If this is a DLC vehicle, make sure the server is streaming it.") { Enabled = false, Label = $"({vehModelName.ToLower()})" };
                                vehicleClassMenu.AddMenuItem(vehBtn);
                                vehBtn.RightIcon = MenuItem.Icon.LOCK;
                            }

                            // Mark duplicate as true and break from the loop because we already found the duplicate.
                            duplicate = true;
                            break;
                        }
                    }

                    // If it's not a duplicate, add the model name.
                    if (!duplicate)
                    {
                        if (DoesModelExist(veh))
                        {
                            var vehBtn = new MenuItem(vehName) { Enabled = true, Label = $"({vehModelName.ToLower()})" };
                            vehicleClassMenu.AddMenuItem(vehBtn);
                        }
                        else
                        {
                            var vehBtn = new MenuItem(vehName, "This vehicle is not available because the model could not be found in your game files. If this is a DLC vehicle, make sure the server is streaming it.") { Enabled = false, Label = $"({vehModelName.ToLower()})" };
                            vehicleClassMenu.AddMenuItem(vehBtn);
                            vehBtn.RightIcon = MenuItem.Icon.LOCK;
                        }
                    }
                }
                #endregion

                // Handle button presses
                vehicleClassMenu.OnItemSelect += (sender2, item2, index2) =>
                {
                    Spawn.SpawnVehicle(VehicleData.Vehicles.VehicleClasses[className][index2], Game.PlayerPed.Position, Game.PlayerPed.Heading, staffSpawn: true, numberPlate: "STAFFCAR");
                };


            }
        }

        private static void VehicleClassMenu_OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;
        }

        private static void VehicleClassMenu_OnMenuOpen(Menu menu)
        {
            MenuController.DontOpenAnyMenu = false;
        }

        public static string GetVehDisplayNameFromModel(string name) => GetLabelText(GetDisplayNameFromVehicleModel((uint)GetHashKey(name)));
        public static bool DoesModelExist(string modelName) => DoesModelExist((uint)GetHashKey(modelName));
        public static bool DoesModelExist(uint modelHash) => IsModelInCdimage(modelHash);
    }
}
