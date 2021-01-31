using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Events;

namespace Curiosity.Core.Client.Managers
{
    public class VehicleManager : Manager<VehicleManager>
    {
        public override void Begin()
        {
            // spawn
            // delete

            EventSystem.Attach("delete:vehicle", new EventCallback(metadata =>
            {
                Logger.Debug("delete vehicle");

                Vehicle vehicle = Cache.PersonalVehicle;

                if (Game.PlayerPed.IsInVehicle())
                    vehicle = Game.PlayerPed.CurrentVehicle;

                if (vehicle != null)
                    EventSystem.Send("delete:entity", vehicle.NetworkId);
                    
                return null;
            }));

            // edit
        }
    }
}
