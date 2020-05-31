using CitizenFX.Core;
using Curiosity.Callouts.Client.Utils;
using Curiosity.Callouts.Shared.Utils;
using System;
using System.Threading.Tasks;

using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Callouts.Client.Classes.Vehicle;

namespace Curiosity.Callouts.Client.Managers
{
    class SpawnManager
    {
        public static SpawnManager Instance { get; private set; }

        public SpawnManager()
        {
            Instance = this;
        }

        internal static async Task<Tuple<Ped, Vehicle>> CreateNewCop(Vector3 position)
        {
            Ped ped = await Ped.Spawn(PedHash.Cop01SMY, position);

            // Edit list based on player location.

            Vehicle vehicle = await Vehicle.Spawn(Collections.PoliceCars.ALL.Random(), position);
            return new Tuple<Ped, Vehicle>(ped, vehicle);
        }
    }
}
