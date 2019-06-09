using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Curiosity.Client.net.Classes.Actions
{
    class Spawners
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Spawn:Weapon", new Action<string>(SpawnWeapon));
            client.RegisterEventHandler("curiosity:Client:Spawn:Car", new Action<string>(SpawnCar));

            API.RegisterCommand("mod", new Action<int, List<object>, string>(ModVehicle), false);
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
