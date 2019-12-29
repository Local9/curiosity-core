using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using Curiosity.Vehicle.Client.net.Classes.CuriosityVehicle;

namespace Curiosity.Vehicle.Client.net.Classes.Menus
{
    class VehicleSpawn
    {
        static Menu menu;
        static Client client = Client.GetInstance();
        static Random random = new Random();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicle:VehicleList", new Action<string>(OnUpdateMenu));
        }

        public static void OpenMenu(int spawnId)
        {
            MenuController.DontOpenAnyMenu = false;
            Client.TriggerServerEvent("curiosity:Server:Vehicle:GetVehicleList", spawnId);

            if (menu == null)
            {
                menu = new Menu("Vehicle Spawn", "Select a vehicle");
                menu.OnMenuOpen += Menu_OnMenuOpen;
                menu.OnMenuClose += Menu_OnMenuClose;
                menu.OnItemSelect += Menu_OnItemSelect;

                MenuController.AddMenu(menu);
                MenuController.EnableMenuToggleKeyOnController = false;
                MenuController.EnableManualGCs = false;
            }

            menu.ClearMenuItems();
            menu.OpenMenu();
        }

        public static void CloseMenu()
        {
            if (menu != null)
                menu.CloseMenu();
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
                menu.ClearMenuItems();

                string json = Encode.BytesToStringConverted(System.Convert.FromBase64String(encodedJson));
                List<VehicleItem> vehicleItems = Newtonsoft.Json.JsonConvert.DeserializeObject<List<VehicleItem>>(json);

                if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                {
                    VehicleItem refVeh = vehicleItems[0];
                    VehicleItem dev = new VehicleItem();
                    dev.InstallSirens = false;
                    dev.SpawnHeading = refVeh.SpawnHeading;
                    dev.SpawnPositionX = refVeh.SpawnPositionX;
                    dev.SpawnPositionY = refVeh.SpawnPositionY;
                    dev.SpawnPositionZ = refVeh.SpawnPositionZ;
                    dev.VehicleHashString = "tezeract";

                    menu.AddMenuItem(new MenuItem("Developer Car") { ItemData = dev });
                }

                foreach (VehicleItem vehicle in vehicleItems)
                {
                    MenuItem item = new MenuItem(vehicle.Name) { ItemData = vehicle };

                    if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                    {
                        item.Enabled = true;
                    }
                    else if (vehicle.UnlockRequirementValue == 0)
                    {
                        item.Enabled = true;
                    }
                    else
                    {
                        if (!Player.PlayerInformation.playerInfo.Skills.ContainsKey(vehicle.UnlockRequiredSkill))
                        {
                            item.Enabled = false;
                        }
                        else
                        {
                            item.Enabled = (Player.PlayerInformation.playerInfo.Skills[vehicle.UnlockRequiredSkill].Value >= vehicle.UnlockRequirementValue);
                        }
                    }

                    item.Description = $"Requires: {vehicle.UnlockRequiredSkillDescription} >= {vehicle.UnlockRequirementValue}";
                    skillDesc = vehicle.UnlockRequiredSkillDescription;
                    menu.AddMenuItem(item);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting list, possible that you have no experience in the required skill. Skill Required: {skillDesc}");
            }
        }

        private static async void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            try
            {
                if (Spawn.IsSpawning)
                {
                    Screen.ShowNotification("~b~Vehicle Spawn:\n~r~Cooldown currently active.");
                    return;
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

                bool spawnSuccess = await Spawn.SpawnVehicle(model, positionToSpawn, vehicleItem.SpawnHeading, vehicleItem.InstallSirens);

                if (!spawnSuccess)
                {
                    Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Unable to spawn vehicle", "Sorry...", "It took too long to load the vehicle or a cooldown is active, please try again later.", 2);
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
