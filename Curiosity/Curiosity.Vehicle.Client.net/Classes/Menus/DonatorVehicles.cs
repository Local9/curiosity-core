using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Vehicle.Client.net.Classes.Menus
{
    class DonatorVehicles
    {
        static Menu donatorVehicleMenu;
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicle:OpenDonatorVehicles", new Action(OpenMenu));
            client.RegisterEventHandler("curiosity:Client:Vehicle:DonatorVehicleList", new Action<string>(OnUpdateMenu));
            
            MenuController.DontOpenAnyMenu = true;
        }

        public static void OpenMenu()
        {
            MenuController.DontOpenAnyMenu = false;

            Client.TriggerServerEvent("curiosity:Server:Vehicle:GetDonatorVehicleList");

            if (donatorVehicleMenu == null)
            {
                donatorVehicleMenu = new Menu("Donator Vehicles", "Select a vehicle");
                donatorVehicleMenu.OnMenuOpen += Menu_OnMenuOpen;
                donatorVehicleMenu.OnMenuClose += Menu_OnMenuClose;
                donatorVehicleMenu.OnItemSelect += Menu_OnItemSelect;

                MenuController.EnableMenuToggleKeyOnController = false;
                MenuController.EnableManualGCs = false;

                MenuController.AddMenu(donatorVehicleMenu);
            }

            donatorVehicleMenu.ClearMenuItems();
            donatorVehicleMenu.OpenMenu();
        }



        public static void CloseMenu()
        {
            if (donatorVehicleMenu != null)
                donatorVehicleMenu.CloseMenu();
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;

            menu.ClearMenuItems();
            MenuBaseFunctions.MenuClose();
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBaseFunctions.MenuOpen();
            menu.AddMenuItem(new MenuItem("Loading...") { Enabled = false });
        }

        private static void OnUpdateMenu(string encodedJson)
        {

            string skillDesc = string.Empty;
            try
            {
                donatorVehicleMenu.ClearMenuItems();

                string json = Encode.BytesToStringConverted(System.Convert.FromBase64String(encodedJson));
                List<VehicleItem> vehicleItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<VehicleItem>>(json);

                if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                {
                    VehicleItem refVeh = vehicleItems[0];
                    VehicleItem dev = new VehicleItem();
                    dev.InstallSirens = false;
                    dev.VehicleHashString = "tezeract";

                    donatorVehicleMenu.AddMenuItem(new MenuItem("Developer Car") { ItemData = dev });
                }

                foreach (VehicleItem vehicle in vehicleItems)
                {
                    MenuItem item = new MenuItem(vehicle.Name) { ItemData = vehicle };
                    donatorVehicleMenu.AddMenuItem(item);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting list, possible that you have no experience in the required skill.");
            }
        }

        private static async void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            try
            {
                Model model = null;
                VehicleItem vehicleItem = menuItem.ItemData;
                string car = vehicleItem.VehicleHashString;
                var enumName = Enum.GetNames(typeof(VehicleHash)).FirstOrDefault(s => s.ToLower().StartsWith(car.ToLower())) ?? "";

                if (Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info(vehicleItem.ToString());
                }

                var modelName = "";

                if (int.TryParse(car, out var hash))
                {
                    model = new Model(hash);
                    modelName = $"{hash}";

                    if (Player.PlayerInformation.IsDeveloper())
                    {
                        Log.Info("INT TryParse Valid");
                    }
                }
                else if (!string.IsNullOrEmpty(enumName))
                {
                    var found = false;
                    foreach (VehicleHash p in Enum.GetValues(typeof(VehicleHash)))
                    {
                        if (!(Enum.GetName(typeof(VehicleHash), p)?.Equals(enumName, StringComparison.InvariantCultureIgnoreCase) ?? false))
                        {
                            continue;
                        }

                        model = new Model(p);
                        modelName = enumName;
                        found = true;

                        if (Classes.Player.PlayerInformation.IsDeveloper())
                        {
                            Screen.ShowNotification($"~r~Info~s~:~n~Model Valid: {model.IsValid}~n~Model: {modelName}");
                        }

                        break;
                    }

                    if (!model.IsValid)
                    {
                        Screen.ShowNotification($"~r~ERROR~s~: Could not model {car}");
                        return;
                    }

                    if (!found)
                    {
                        Screen.ShowNotification($"~r~ERROR~s~: Could not load model {car}");
                        return;
                    }
                }
                else
                {
                    model = new Model(car);
                    modelName = car;
                }



                Vector3 positionToSpawn = new Vector3(vehicleItem.SpawnPositionX, vehicleItem.SpawnPositionY, vehicleItem.SpawnPositionZ);

                bool spawnSuccess = await Vehicle.Spawn.SpawnVehicle(model, positionToSpawn, vehicleItem.SpawnHeading, vehicleItem.InstallSirens);

                if (!spawnSuccess)
                {
                    Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Unable to spawn vehicle", "Sorry...", "It took too long to load the vehicle or a cooldown is active, please try again later.", 2);
                    return;
                }


            }
            catch (Exception ex)
            {
                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Error($"Menu_OnItemSelect -> {ex.Message}");
                }
            }
        }
    }
}
