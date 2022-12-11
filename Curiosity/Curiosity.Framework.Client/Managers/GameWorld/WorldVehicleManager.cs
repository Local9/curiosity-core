using Curiosity.Framework.Client.Managers.GameWorld.Entity;

namespace Curiosity.Framework.Client.Managers.GameWorld
{
    public class WorldVehicleManager : Manager<WorldVehicleManager>
    {
        int HASH_PARTYBUS => GetHashKey("pbus2");

        Dictionary<int, WorldVehicle> _worldVehicles = new();

        public override void Begin()
        {
            
        }

        [TickHandler]
        private async Task OnWorldVehicleAsync()
        {
            Vehicle[] vehicles = World.GetAllVehicles();
            foreach (Vehicle vehicle in vehicles)
            {
                if (vehicle.Model.Hash == HASH_PARTYBUS)
                {
                    if (!_worldVehicles.ContainsKey(vehicle.Handle))
                        _worldVehicles.Add(vehicle.Handle, new WorldVehiclePartyBus(vehicle));
                }
            }
        }
    }
}