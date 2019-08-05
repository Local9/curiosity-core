using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MenuAPI;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Curiosity.Vehicle.Client.net.Classes.Menus
{
    class VehicleSpawn
    {
        static Menu menu;
        static Client client = Client.GetInstance();
        static VehicleSpawnTypes VehicleSpawnType;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Server:Vehicle:VehicleList", new Action<string>(OnUpdateMenu));
        }

        public static void OpenMenu(VehicleSpawnTypes vehicleSpawnType)
        {
            VehicleSpawnType = vehicleSpawnType;

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
            menu.CloseMenu();
        }

        private static void Menu_OnMenuClose(Menu menu)
        {
            menu.ClearMenuItems();
            MenuBaseFunctions.MenuClose();
        }

        private static void Menu_OnMenuOpen(Menu menu)
        {
            MenuBaseFunctions.MenuOpen();
            menu.AddMenuItem(new MenuItem("Loading..."));
            Client.TriggerServerEvent("curiosity:Server:Vehicle:GetVehicleList", VehicleSpawnType);
            OnUpdateMenu(string.Empty);
        }

        private static void OnUpdateMenu(string json)
        {
            menu.ClearMenuItems();
            // add new items
            // VehicleHash, VehicleName, Enabled, Description
            menu.AddMenuItem(new MenuItem("car1") { ItemData = new VehicleItem() { Name = "Ruiner", VehicleHashString = "Ruiner3", SpawnPosition = new Vector3(-1069.468f, -878.0467f, 5.85375f), SpawnHeading = 206.0515f } });
        }

        private static async void Menu_OnItemSelect(Menu menu, MenuItem menuItem, int itemIndex)
        {
            Model model = null;
            VehicleItem vehicleItem = menuItem.ItemData;
            string car = vehicleItem.VehicleHashString;
            var enumName = Enum.GetNames(typeof(VehicleHash)).FirstOrDefault(s => s.ToLower().StartsWith(car.ToLower())) ?? "";

            var modelName = "";

            if (int.TryParse(car, out var hash))
            {
                model = new Model(hash);
                modelName = $"{hash}";
            }
            else if (!string.IsNullOrEmpty(enumName))
            {
                var found = false;
                foreach (VehicleHash p in Enum.GetValues(typeof(VehicleHash)))
                {
                    if (!(Enum.GetName(typeof(VehicleHash), p)?.Equals(enumName, StringComparison.InvariantCultureIgnoreCase) ??
                          false))
                    {
                        continue;
                    }

                    model = new Model(p);
                    modelName = enumName;
                    found = true;
                    break;
                }

                if (!found)
                {
                    return;
                }
            }
            else
            {
                model = new Model(car);
                modelName = car;
            }

            if (!await SpawnVehicle(model, vehicleItem.SpawnPosition, vehicleItem.SpawnHeading))
            {

            }
        }

        private static async Task<bool> SpawnVehicle(Model model, Vector3 spawnPosition, float heading)
        {
            if (Game.PlayerPed.CurrentVehicle != null)
            {
                API.NetworkFadeOutEntity(Game.PlayerPed.CurrentVehicle.Handle, true, false);
                await Client.Delay(1000);
                Game.PlayerPed.CurrentVehicle.Delete();
            }

            if (Client.CurrentVehicle != null)
            {
                if (Client.CurrentVehicle.Exists())
                {
                    API.NetworkFadeOutEntity(Client.CurrentVehicle.Handle, true, false);
                    await Client.Delay(1000);
                    Client.CurrentVehicle.Delete();
                }
            }

            await model.Request(10000);

            var veh = await World.CreateVehicle(model, spawnPosition, heading);
            if (veh == null)
            {
                return false;
            }

            model.MarkAsNoLongerNeeded();

            API.NetworkFadeInEntity(veh.Handle, false);

            Game.PlayerPed.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);
            veh.LockStatus = VehicleLockStatus.Unlocked;
            veh.NeedsToBeHotwired = false;
            veh.IsEngineRunning = true;

            Blip blip = veh.AttachBlip();
            blip.IsShortRange = false;
            blip.Sprite = BlipSprite.PersonalVehicleCar;
            blip.Priority = 100;
            blip.Name = "Personal Vehicle";

            Client.CurrentVehicle = Game.PlayerPed.CurrentVehicle;

            int networkId = API.VehToNet(veh.Handle);
            API.SetNetworkIdExistsOnAllMachines(networkId, true);
            API.SetNetworkIdCanMigrate(networkId, true);

            Client.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", networkId);
            return true;
        }
    }
}
