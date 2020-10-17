using CitizenFX.Core;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Entity;
using Curiosity.Shared.Client.net;
using Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicles.Client.net.Classes.Menus
{
    class DonatorVehicles
    {
        static Menu donatorVehicleMenu;
        static Client client = Client.GetInstance();

        static Random random = new Random();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicle:OpenDonatorVehicles", new Action(OpenMenu));
            client.RegisterEventHandler("curiosity:Client:Vehicle:DonatorVehicleList", new Action<string>(OnUpdateMenu));

            MenuController.DontOpenAnyMenu = true;
        }

        public static void OpenMenu()
        {
            Log.Info("Donator menu opening");

            MenuController.DontOpenAnyMenu = false;

            bool canActivate = (Player.PlayerInformation.IsStaff() || Player.PlayerInformation.IsDonator());
            if (!canActivate)
            {
                Log.Info("Donators Only!");
                return;
            }

            Client.TriggerServerEvent("curiosity:Server:Vehicle:GetDonatorVehicleList");

            if (donatorVehicleMenu == null)
            {

                donatorVehicleMenu = new Menu("Donator Vehicles", "Select a vehicle");
                donatorVehicleMenu.OnMenuOpen += Menu_OnMenuOpen;
                donatorVehicleMenu.OnMenuClose += Menu_OnMenuClose;
                donatorVehicleMenu.OnItemSelect += Menu_OnItemSelect;

                MenuController.AddMenu(donatorVehicleMenu);
                MenuController.EnableMenuToggleKeyOnController = false;
                MenuController.EnableManualGCs = false;
            }

            donatorVehicleMenu.ClearMenuItems();
            donatorVehicleMenu.OpenMenu();
        }

        public static void CloseMenu()
        {
            if (donatorVehicleMenu != null && donatorVehicleMenu.Visible)
                donatorVehicleMenu.CloseMenu();
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            MenuController.DontOpenAnyMenu = true;
            menu.ClearMenuItems();
        }

        private async static void Menu_OnMenuOpen(Menu menu)
        {
            await BaseScript.Delay(10);
            MenuBaseFunctions.MenuOpen();
            menu.AddMenuItem(new MenuItem("Loading...") { Enabled = false });
        }

        private static void OnUpdateMenu(string encodedJson)
        {

            try
            {
                donatorVehicleMenu.ClearMenuItems();
                Log.Info("Data returned");

                string json = Encode.BytesToStringConverted(System.Convert.FromBase64String(encodedJson));
                List<VehicleItem> vehicleItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<VehicleItem>>(json);

                if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                {
                    VehicleItem refVeh = vehicleItems[0];
                    VehicleItem dev = new VehicleItem();
                    dev.InstallSirens = false;
                    dev.VehicleHashString = "tezeract";

                    donatorVehicleMenu.AddMenuItem(new MenuItem("Developer Car") { ItemData = dev });

                    Log.Info($"Data Added: {dev}");
                }

                foreach (VehicleItem vehicle in vehicleItems)
                {
                    MenuItem item = new MenuItem(vehicle.Name) { ItemData = vehicle };
                    donatorVehicleMenu.AddMenuItem(item);

                    Log.Info($"Data Added: {vehicle}");
                }

                if (!donatorVehicleMenu.Visible)
                {
                    MenuController.DontOpenAnyMenu = false;
                    donatorVehicleMenu.OpenMenu();
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
                menu.CloseMenu();

                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info($"~r~Info~s~:~n~Trying to spawn {menuItem.ItemData}");
                }

                if (!Player.PlayerInformation.IsDonator() || !Player.PlayerInformation.IsStaff())
                {
                    Log.Info("Donator Only!");
                }

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
                            Log.Info($"~r~Info~s~:~n~Model Valid: {model.IsValid}~n~Model: {modelName}");
                        }

                        break;
                    }

                    if (!model.IsValid)
                    {
                        Log.Info($"~r~ERROR~s~: Could not model {car}");
                        return;
                    }

                    if (!found)
                    {
                        Log.Info($"~r~ERROR~s~: Could not load model {car}");
                        return;
                    }
                }
                else
                {
                    model = new Model(car);
                    modelName = car;
                }

                if (Classes.Player.PlayerInformation.IsDeveloper())
                {
                    Log.Info($"~r~Info~s~:~n~Model found");
                }

                Vector3 outPos = new Vector3();
                if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
                {
                    Vector3 spawningPosition = new Vector3();
                    float heading = 0f;
                    int u = 0;

                    if (GetNthClosestVehicleNodeWithHeading(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref spawningPosition, ref heading, ref u, 9, 3.0f, 2.5f))
                    {
                        Vector3 safespaceOut = new Vector3();
                        if (GetSafeCoordForPed(spawningPosition.X, spawningPosition.Y, spawningPosition.Z, true, ref safespaceOut, 16))
                        {
                            spawningPosition = safespaceOut;
                        }

                        if (Classes.Player.PlayerInformation.IsDeveloper())
                        {
                            Log.Info($"~r~Info~s~:~n~Found a safe location");
                        }

                        CitizenFX.Core.Vehicle spawnedVechicle = await Spawn.SpawnVehicleEmpty(model, spawningPosition, heading, vehicleItem.InstallSirens);

                        Game.PlayerPed.Task.WarpIntoVehicle(spawnedVechicle, VehicleSeat.Driver);
                    }
                    else
                    {
                        Game.PlayerPed.IsInvincible = true;
                        CitizenFX.Core.Vehicle spawnedVechicle = await Spawn.SpawnVehicleEmpty(model, Game.PlayerPed.Position, Game.PlayerPed.Heading, vehicleItem.InstallSirens);
                        Game.PlayerPed.Task.WarpIntoVehicle(spawnedVechicle, VehicleSeat.Driver);
                        Game.PlayerPed.IsInvincible = false;
                    }
                }
                else
                {
                    Game.PlayerPed.IsInvincible = true;
                    CitizenFX.Core.Vehicle spawnedVechicle = await Spawn.SpawnVehicleEmpty(model, Game.PlayerPed.Position, Game.PlayerPed.Heading, vehicleItem.InstallSirens);
                    Game.PlayerPed.Task.WarpIntoVehicle(spawnedVechicle, VehicleSeat.Driver);
                    Game.PlayerPed.IsInvincible = false;
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
