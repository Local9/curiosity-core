using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Actions
{
    class ChatCommands
    {
        static Client client = Client.GetInstance();
        static uint playerGroupHash = 0;

        public static void Init()
        {
            API.AddRelationshipGroup("PLAYER", ref playerGroupHash);

            client.RegisterEventHandler("curiosity:Client:Command:SpawnWeapon", new Action<string>(SpawnWeapon));
            client.RegisterEventHandler("curiosity:Client:Command:SpawnCar", new Action<string>(SpawnCar));

            API.RegisterCommand("mod", new Action<int, List<object>, string>(ModVehicle), false);
            API.RegisterCommand("tp", new Action<int, List<object>, string>(Teleport), false);
            API.RegisterCommand("pos", new Action<int, List<object>, string>(SaveCoords), false);
            API.RegisterCommand("dv", new Action<int, List<object>, string>(DeleteVehicle), false);
            API.RegisterCommand("dvn", new Action<int, List<object>, string>(DeleteVehicleNuke), false);
            // test commands
            API.RegisterCommand("pulse", new Action<int, List<object>, string>(Pulse), false);
            API.RegisterCommand("fire", new Action<int, List<object>, string>(Fire), false);

            API.RegisterCommand("knifeCallout", new Action<int, List<object>, string>(KnifeCallout), false);
        }

        static void KnifeCallout(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            int numberToSpawn = 1;
            if (arguments.Count > 0)
                int.TryParse($"{arguments[0]}", out numberToSpawn);

            bool attackPlayer = false;
            string nameOfPlayer = string.Empty;
            if (arguments.Count > 1)
            {
                bool.TryParse($"{arguments[1]}", out attackPlayer);
                nameOfPlayer = $"{arguments[2]}";
            }

            if (numberToSpawn > 8)
                numberToSpawn = 8;

            string location = "~o~24/7, Sandy Shores~w~";
            string callout = "~r~Armed Man~w~";
            string response = "~r~Code 3~w~";

            uint suspectGroupHash = 0;
            API.AddRelationshipGroup("suspect", ref suspectGroupHash);
            API.SetRelationshipBetweenGroups(5, suspectGroupHash, playerGroupHash);
            API.SetRelationshipBetweenGroups(5, playerGroupHash, suspectGroupHash);

            Model marine = PedHash.Marine01SMY;
            Vector3 postion = new Vector3(1966.8389892578f, 3737.8703613281f, 32.188823699951f);

            for(int i = 0; i < numberToSpawn; i++)
                Environment.PedClasses.PedHandler.Create(marine, postion, 180.0f, suspectGroupHash, true, attackPlayer, nameOfPlayer);

            Environment.UI.Notifications.NineOneOne(2, $"All Units", $"{response} {location}", $"{callout}", 2);
        }

        

        static void Fire(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            Random random = new Random();

            Vector3 pos = Game.PlayerPed.Position;
            Vector3 offset = API.GetOffsetFromEntityInWorldCoords(Client.PedHandle, 0.0f, 5f, 0.0f);

            float posZ = offset.Z;
            API.GetGroundZFor_3dCoord(offset.X, offset.Y, offset.Z, ref posZ, false);
            API.StartScriptFire(offset.X, offset.Y, posZ, 25, true);
        }

        static async void Pulse(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;
            Screen.Fading.FadeOut(10000);
            while (Screen.Fading.IsFadingOut)
            {
                await BaseScript.Delay(1);
            }
            Screen.Fading.FadeIn(10000);
            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(1);
            }
        }

        static async void DeleteVehicleNuke(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsStaff()) return;

            int totalFound = 0;
            int totalNotDeleted = 0;

            foreach(int vehicleHandle in new Helpers.VehicleList())
            {
                if (!IsPedAPlayer(GetPedInVehicleSeat(vehicleHandle, -1)))
                {
                    totalFound++;

                    int currentHandle = vehicleHandle;
                    int vehToDelete = vehicleHandle;
                    SetVehicleHasBeenOwnedByPlayer(vehicleHandle, false);
                    await Client.Delay(0);
                    SetEntityAsMissionEntity(vehicleHandle, false, false);
                    API.SetEntityAsNoLongerNeeded(ref currentHandle);
                    await Client.Delay(0);
                    API.DeleteEntity(ref vehToDelete);

                    if (DoesEntityExist(vehicleHandle))
                        API.DeleteEntity(ref vehToDelete);

                    await Client.Delay(0);

                    if (DoesEntityExist(vehicleHandle))
                        totalNotDeleted++;
                }
            }
            Environment.UI.Notifications.LifeV(7, "Server Information", "Vehicle Nuke Info", $"Total Removed: {totalFound - totalNotDeleted:00} / {totalFound:00}", 2);
        }

        static async void DeleteVehicle(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsStaff()) return;

            CitizenFX.Core.Vehicle veh = null;

            if (Game.PlayerPed.IsInVehicle())
            {
                veh = Game.PlayerPed.CurrentVehicle;
                Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);
            }
            else
            {
                 veh = Helpers.WorldProbe.GetVehicleInFrontOfPlayer();
            }

            if (veh == null) return;

            int handle = veh.Handle;

            API.NetworkFadeOutEntity(veh.Handle, true, false);
            veh.IsEngineRunning = false;
            veh.MarkAsNoLongerNeeded();
            await Client.Delay(1000);
            API.DeleteEntity(ref handle);


            await BaseScript.Delay(0);
        }

        static async void SaveCoords(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;
            if (arguments.Count < 1) return;

            Vector3 pos = Game.PlayerPed.Position;

            Client.TriggerServerEvent("curiosity:Server:Command:SavePosition", $"{arguments[0]}", pos.X, pos.Y, pos.Z, Game.PlayerPed.Heading);

            await BaseScript.Delay(0);
        }

        static async void SpawnCar(string car)
        {
            try
            {
                if (!Player.PlayerInformation.IsDeveloper()) return;
                if (string.IsNullOrEmpty(car)) return;

                Model model = null;
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
                        Screen.ShowNotification($"~r~ERROR~s~: Could not load model {car}");
                        return;
                    }
                }
                else
                {
                    model = new Model(car);
                    modelName = car;
                }

                if (!await SpawnVehicle(model))
                {
                    Environment.UI.Notifications.Curiosity(1, "Curiosity", "Vehicle Error", $"Could not load model {modelName}", 2);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        
        private static async Task<bool> SpawnVehicle(Model model)
        {
            if (Game.PlayerPed.CurrentVehicle != null)
            {
                Game.PlayerPed.CurrentVehicle.Delete();
            }

            var veh = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
            if (veh == null)
            {
                return false;
            }

            API.NetworkFadeInEntity(veh.Handle, false);

            Game.PlayerPed.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);
            veh.LockStatus = VehicleLockStatus.Unlocked;
            veh.NeedsToBeHotwired = false;
            veh.IsEngineRunning = true;
            veh.Mods.LicensePlate = "DEVTOOLS";

            veh.Mods.InstallModKit();
            veh.Mods[VehicleModType.Engine].Index = veh.Mods[VehicleModType.Engine].ModCount - 1;
            veh.Mods[VehicleModType.Brakes].Index = veh.Mods[VehicleModType.Brakes].ModCount - 1;
            veh.Mods[VehicleModType.Transmission].Index = veh.Mods[VehicleModType.Transmission].ModCount - 1;

            int networkId = API.VehToNet(veh.Handle);
            API.SetNetworkIdExistsOnAllMachines(networkId, true);
            API.SetNetworkIdCanMigrate(networkId, true);

            Client.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", networkId);
            Environment.UI.Notifications.Curiosity(1, "Curiosity", "Vehicle Spawned", $"Available Mods can be found in the Debug Console", 2);
            Debug.WriteLine($"Vehicle Mods: {string.Join(", ", veh.Mods.GetAllMods().Select(m => Enum.GetName(typeof(VehicleModType), m.ModType)))}");

            return true;
        }

        static void SpawnWeapon(string weapon)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;
            
        }

        static void Teleport(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;
            if (arguments.Count < 3) return;

            float posX = float.Parse(arguments[0].ToString());
            float posY = float.Parse(arguments[1].ToString());
            float posZ = float.Parse(arguments[2].ToString());

            API.FreezeEntityPosition(Game.PlayerPed.Handle, true);
            Game.PlayerPed.Position = new Vector3(posX, posY, posZ);
            API.FreezeEntityPosition(Game.PlayerPed.Handle, false);
        }

        static void ModVehicle(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!Player.PlayerInformation.IsDeveloper()) return;

                CitizenFX.Core.Vehicle veh = Game.PlayerPed.CurrentVehicle;
                if (veh == null || arguments.Count < 2) return;

                string vehicleModTypeName = arguments[0].ToString();
                int vehicleModIndex = int.Parse(arguments[1].ToString());

                if (vehicleModTypeName == "list")
                {
                    Debug.WriteLine($"Vehicle Mods: {string.Join(", ", veh.Mods.GetAllMods().Select(m => Enum.GetName(typeof(VehicleModType), m.ModType)))}");
                    return;
                }

                foreach (VehicleModType vehicleMod in Enum.GetValues(typeof(VehicleModType)))
                {
                    if (Enum.GetName(typeof(VehicleModType), vehicleMod).Equals(vehicleModTypeName))
                    {
                        veh.Mods[vehicleMod].Index = MathUtil.Clamp(vehicleModIndex, 0, veh.Mods[vehicleMod].ModCount - 1);
                        Environment.UI.Notifications.Curiosity(1, "Curiosity", "Vehicle Mod", $"Set {vehicleModTypeName} to {vehicleModIndex}.", 2);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }
        }
    }
}
