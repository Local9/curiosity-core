using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Interface;
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

            EventSystem.Attach("repair:vehicle", new EventCallback(metadata =>
            {
                Logger.Debug("repair vehicle");

                Vehicle vehicle = Cache.PersonalVehicle;

                if (Game.PlayerPed.IsInVehicle())
                    vehicle = Game.PlayerPed.CurrentVehicle;

                if (vehicle != null)
                {
                    vehicle.Wash();
                    vehicle.Repair();
                    vehicle.EngineHealth = 1000f;
                    vehicle.BodyHealth = 1000f;
                    vehicle.PetrolTankHealth = 1000f;
                    vehicle.Health = vehicle.MaxHealth;
                    vehicle.ClearLastWeaponDamage();

                    Notify.Success($"Vehicle Repaired");
                }

                return null;
            }));

            // edit
        }
    }
}
