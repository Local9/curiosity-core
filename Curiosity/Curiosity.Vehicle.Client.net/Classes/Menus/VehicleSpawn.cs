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
        static Random random = new Random();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicle:VehicleList", new Action<string>(OnUpdateMenu));
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
            if (menu != null)
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
            menu.AddMenuItem(new MenuItem("Ruiner") { ItemData = new VehicleItem() { Name = "Ruiner", VehicleHashString = "Ruiner3", SpawnPosition = new Vector3(-1069.468f, -878.0467f, 5.85375f), SpawnHeading = 206.0515f, LocationOfSpawn = new Vector3(-1108.226f, -847.1646f, 19.31689f) } });
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



            if (!await Vehicle.Spawn.SpawnVehicle(model, vehicleItem.SpawnPosition, vehicleItem.SpawnHeading, vehicleItem.LocationOfSpawn))
            {
                Client.TriggerEvent("curiosity:Client:Notification:LifeV", 1, "Unable to spawn vehicle", "Please try again shortly.", string.Empty, 2);
            }
        }
    }
}
