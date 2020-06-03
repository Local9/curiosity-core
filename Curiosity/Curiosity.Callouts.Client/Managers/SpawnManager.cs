using CitizenFX.Core;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Classes;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers
{
    class SpawnManager
    {
        public static SpawnManager Instance { get; private set; }

        static List<PedHash> CityCops = new List<PedHash>() { PedHash.Cop01SFY, PedHash.Cop01SMY };
        static List<PedHash> RuralCops = new List<PedHash>() { PedHash.Sheriff01SFY, PedHash.Sheriff01SFY };

        public SpawnManager()
        {
            Instance = this;
        }

        internal static async Task<Tuple<Ped, Vehicle>> CreateNewCop(Vector3 position)
        {
            // Edit list based on player location.
            PedHash pedToSpawn;
            VehicleHash vehicleHash;

            switch(PlayerManager.PatrolZone)
            {
                case PatrolZone.Highway:
                    pedToSpawn = RuralCops.Random();
                    vehicleHash = Collections.PoliceCars.HIGHWAY.Random();
                    break;
                case PatrolZone.Country:
                case PatrolZone.Rural:
                    pedToSpawn = RuralCops.Random();
                    vehicleHash = Collections.PoliceCars.RURAL.Random();
                    break;
                default:
                    pedToSpawn = CityCops.Random();
                    vehicleHash = Collections.PoliceCars.URBAN.Random();
                    break;
            }

            Ped ped = await Ped.Spawn(pedToSpawn, position);
            ped.IsMission = true;
            ped.Fx.AlwaysKeepTask = true;

            if (ped == null)
            {
                Logger.Log($"Failed to create cop");
            }

            Vehicle vehicle = await Vehicle.Spawn(vehicleHash, position);
            ped.PutInVehicle(vehicle);

            vehicle.Fx.IsSirenActive = true;

            Blip blip = ped.AttachBlip();
            blip.Color = BlipColor.Blue;
            blip.IsFriendly = true;

            if (vehicle == null)
            {
                Logger.Log($"Failed to create vehicle");
            }

            return new Tuple<Ped, Vehicle>(ped, vehicle);
        }
    }
}
