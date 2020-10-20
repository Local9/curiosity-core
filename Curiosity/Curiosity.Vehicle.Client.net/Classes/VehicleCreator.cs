using CitizenFX.Core;
using Curiosity.Shared.Client.net;
using Curiosity.Vehicles.Client.net;
using Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle;
using System;
using System.Linq;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Vehicle.Client.net.Classes
{
    class VehicleCreator
    {
        static Plugin client = Plugin.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Vehicle:Create", new Action<string>(SpawnVehicle));
        }

        public static async void SpawnVehicle(string vehicleHash)
        {
            Model model = null;
            var enumName = Enum.GetNames(typeof(VehicleHash)).FirstOrDefault(s => s.ToLower().StartsWith(vehicleHash.ToLower())) ?? "";

            var modelName = "";

            if (int.TryParse(vehicleHash, out var hash))
            {
                model = new Model(hash);
                modelName = $"{hash}";
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

                    break;
                }

                if (!model.IsValid)
                {
                    Log.Info($"~r~ERROR~s~: Model Invalid {vehicleHash}");
                    return;
                }

                if (!found)
                {
                    Log.Info($"~r~ERROR~s~: Could not load model {vehicleHash}");
                    return;
                }
            }
            else
            {
                model = new Model(vehicleHash);
                modelName = vehicleHash;
            }

            Vector3 outPos = new Vector3();
            if (GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, 3, ref outPos, 0, 0, 0))
            {
                if (Game.PlayerPed.IsInVehicle())
                {
                    if (Plugin.CurrentVehicle == Game.PlayerPed.CurrentVehicle)
                        Game.PlayerPed.Task.WarpOutOfVehicle(Plugin.CurrentVehicle);
                }

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

                    Plugin.CurrentVehicle = await Spawn.SpawnVehicleEmpty(model, spawningPosition, heading);

                    Game.PlayerPed.Task.WarpIntoVehicle(Plugin.CurrentVehicle, VehicleSeat.Driver);
                }
                else
                {
                    Game.PlayerPed.IsInvincible = true;
                    Plugin.CurrentVehicle = await Spawn.SpawnVehicleEmpty(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
                }
            }
            else
            {
                Game.PlayerPed.IsInvincible = true;
                Plugin.CurrentVehicle = await Spawn.SpawnVehicleEmpty(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
            }

            if (Plugin.CurrentVehicle != null)
            {
                Game.PlayerPed.Task.WarpIntoVehicle(Plugin.CurrentVehicle, VehicleSeat.Driver);
                Game.PlayerPed.IsInvincible = false;

                BaseScript.TriggerEvent("curiosity:Player:Menu:VehicleId", Plugin.CurrentVehicle.Handle);
            }
        }
    }
}
