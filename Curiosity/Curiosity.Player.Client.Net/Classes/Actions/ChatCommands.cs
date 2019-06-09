using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Curiosity.Client.net.Classes.Actions
{
    class ChatCommands
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Command:SpawnWeapon", new Action<string>(SpawnWeapon));
            client.RegisterEventHandler("curiosity:Client:Command:SpawnCar", new Action<string>(SpawnCar));

            API.RegisterCommand("mod", new Action<int, List<object>, string>(ModVehicle), false);
            API.RegisterCommand("tp", new Action<int, List<object>, string>(Teleport), false);
            API.RegisterCommand("pos", new Action<int, List<object>, string>(SaveCoords), false);
            API.RegisterCommand("dv", new Action<int, List<object>, string>(DeleteVehicle), false);
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

            Client.TriggerServerEvent("curiosity:Server:Command:SavePosition", $"{arguments[0]}", pos.X, pos.Y, pos.Z);

            await BaseScript.Delay(0);
        }

        static async void SpawnCar(string car)
        {
            try
            {
                if (!Player.PlayerInformation.IsDeveloper()) return;

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
            Environment.UI.Notifications.Curiosity(1, "Curiosity", "Vehicle Spawned", $"Available Mods: {string.Join(", ", veh.Mods.GetAllMods().Select(m => Enum.GetName(typeof(VehicleModType), m.ModType)))}", 2);

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
                    Environment.UI.Notifications.Curiosity(1, "Curiosity", "Mod Type Unknown", $"Available Mods: {string.Join(", ", veh.Mods.GetAllMods().Select(m => Enum.GetName(typeof(VehicleModType), m.ModType)))}", 2);
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
