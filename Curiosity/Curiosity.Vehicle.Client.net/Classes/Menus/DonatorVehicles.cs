using CitizenFX.Core;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Client.net;
using Curiosity.Vehicle.Client.net.Classes;
using Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle;
using Curiosity.Vehicles.Client.net.Classes.CurPlayer;
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
        static Plugin client = Plugin.GetInstance();

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

            bool canActivate = (PlayerInformation.IsStaff() || PlayerInformation.IsDonator());
            if (!canActivate)
            {
                Log.Info("Donators Only!");
                return;
            }

            Plugin.TriggerServerEvent("curiosity:Server:Vehicle:GetDonatorVehicleList");

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

                if (PlayerInformation.privilege == Privilege.DEVELOPER)
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

                if (PlayerInformation.IsDeveloper())
                {
                    Log.Info($"~r~Info~s~:~n~Trying to spawn {menuItem.ItemData}");
                }

                if (!PlayerInformation.IsDonator() || !PlayerInformation.IsStaff())
                {
                    Log.Info("Donator Only!");
                }
                VehicleItem vehicleItem = menuItem.ItemData;
                string car = vehicleItem.VehicleHashString;

                VehicleCreator.SpawnVehicle(car);
            }
            catch (Exception ex)
            {
                if (PlayerInformation.IsDeveloper())
                {
                    Log.Error($"Menu_OnItemSelect -> {ex.Message}");
                }
            }
        }
    }
}
