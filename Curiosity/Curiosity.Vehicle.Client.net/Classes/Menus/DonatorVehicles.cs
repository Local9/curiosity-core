using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using Curiosity.Vehicle.Client.net.Classes.CuriosityVehicle;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Vehicle.Client.net.Classes.Menus
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
                menu.CloseMenu();

                if (Client.CurrentVehicle != null)
                {
                    if (Client.CurrentVehicle.Exists() && Client.CurrentVehicle.Position.Distance(Game.PlayerPed.Position) < 300f)
                    {
                        Screen.ShowNotification("~r~Sorry, you currently have a vehicle out or close by.");
                        return;
                    }
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

                

                Vector3 outPos = new Vector3();
                if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
                {
                    Vector3 spawningPosition = new Vector3();
                    float heading = 0f;
                    int u = 0;

                    if (GetNthClosestVehicleNodeWithHeading(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, random.Next(100, 150), ref spawningPosition, ref heading, ref u, 9, 3.0f, 2.5f))
                    {
                        Vector3 safespaceOut = new Vector3();
                        if (GetSafeCoordForPed(spawningPosition.X, spawningPosition.Y, spawningPosition.Z, true, ref safespaceOut, 16))
                        {
                            spawningPosition = safespaceOut;
                        }

                        CitizenFX.Core.Vehicle spawnedVechicle = await Spawn.SpawnVehicleEmpty(model, spawningPosition, vehicleItem.SpawnHeading, vehicleItem.InstallSirens);

                        Model mechanic = PedHash.Xmech01SMY;
                        await mechanic.Request(10000);
                        Ped ped = await World.CreatePed(mechanic, spawningPosition + new Vector3(0f, 0f, 2f));
                        ped.IsPositionFrozen = true;
                        mechanic.MarkAsNoLongerNeeded();
                        ped.Task.WarpIntoVehicle(spawnedVechicle, VehicleSeat.Driver);
                        ped.IsPositionFrozen = false;

                        TaskSetBlockingOfNonTemporaryEvents(ped.Handle, true);

                        ped.RelationshipGroup = Client.MechanicRelationshipGroup;

                        ped.RelationshipGroup.SetRelationshipBetweenGroups(Client.PlayerRelationshipGroup, Relationship.Like, true);

                        ped.Task.ClearAll();

                        ped.DrivingStyle = DrivingStyle.IgnoreLights;

                        if (outPos.Distance(Game.PlayerPed.Position) >= 20f)
                        {
                            outPos = Game.PlayerPed.Position;
                        }
                            
                        TaskVehiclePark(ped.Handle, spawnedVechicle.Handle, outPos.X, outPos.Y, outPos.Z, 0f, 3, 20f, true);

                        while (spawnedVechicle.Position.Distance(outPos) >= 5f)
                        {
                            await BaseScript.Delay(0);

                            if (spawnedVechicle.Position.Distance(Game.PlayerPed.Position) < 50f && spawnedVechicle.IsStopped && spawnedVechicle.Position.Distance(outPos) < 50f)
                                break;
                        }

                        SetVehicleHalt(spawnedVechicle.Handle, 3f, 0, false);

                        ped.SetConfigFlag(122, true);
                        ped.SetConfigFlag(314, true);
                        SetEnableHandcuffs(ped.Handle, true);

                        spawnedVechicle.SoundHorn(250);
                        await BaseScript.Delay(250);
                        spawnedVechicle.SoundHorn(250);
                        await BaseScript.Delay(250);

                        spawnedVechicle.IsPositionFrozen = true;
                        TaskLeaveVehicle(ped.Handle, spawnedVechicle.Handle, 1);

                        await BaseScript.Delay(200);
                        spawnedVechicle.IsPositionFrozen = false;
                        spawnedVechicle.IsStolen = false;
                        spawnedVechicle.IsWanted = false;

                        ped.Task.WanderAround();

                        while (!ped.IsOccluded)
                        {
                            await BaseScript.Delay(0);
                            ped.MarkAsNoLongerNeeded();
                            ped.Delete();
                        }
                    }
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
