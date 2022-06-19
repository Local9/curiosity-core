using Curiosity.Core.Client.Extensions;
using System.Linq;

namespace Curiosity.Core.Client.Managers
{
    public class VehicleSpawnSafetyManager : Manager<VehicleSpawnSafetyManager>
    {
        static Dictionary<int, SafeZoneVehicle> safeZoneVehicles = new Dictionary<int, SafeZoneVehicle>();

        public override void Begin()
        {

        }

        public async Task EnableSafeSpawnCheck()
        {
            Instance.AttachTickHandler(SafeSpawnCheck);

            DateTime maxWaitTime = DateTime.UtcNow.AddSeconds(5);
            while (true)
            {
                await BaseScript.Delay(500);

                if (maxWaitTime < DateTime.UtcNow) break;
            }

            DisableSafeSpawnCheck();
        }

        public async Task DisableSafeSpawnCheck()
        {
            List<Vehicle> vehicles = World.GetAllVehicles().ToList().Select(m => m).Where(m => m.Position.Distance(Cache.PlayerPed.Position) < 3f && m.Handle != Cache.PlayerPed.CurrentVehicle.Handle).ToList();

            if (vehicles.Count == 0)
            {
                Instance.DetachTickHandler(SafeSpawnCheck);
                return;
            }

            await BaseScript.Delay(1000);
            DisableSafeSpawnCheck();
        }

        public async Task SafeSpawnCheck()
        {
            try
            {
                List<Vehicle> vehicles = World.GetAllVehicles().ToList().Select(m => m).Where(m => m.Position.Distance(Game.PlayerPed.Position) < 30f && m.Handle != Game.PlayerPed.CurrentVehicle.Handle).ToList();

                foreach (Vehicle vehicle in vehicles)
                {
                    if (vehicle is null) continue;
                    if (!vehicle.Exists()) continue;

                    if (safeZoneVehicles.ContainsKey(vehicle.Handle)) continue;
                    safeZoneVehicles.Add(vehicle.Handle, new SafeZoneVehicle(vehicle));
                }
            }
            catch (Exception ex)
            {

            }
        }
    }

    class SafeZoneVehicle
    {
        Vehicle _vehicle;
        PluginManager PluginManager = PluginManager.Instance;

        public SafeZoneVehicle(Vehicle vehicle)
        {
            _vehicle = vehicle;
            PluginManager.AttachTickHandler(DisableCollision);
        }

        async Task DisableCollision()
        {
            if (!_vehicle.IsInRangeOf(Cache.PlayerPed.Position, 10f))
            {
                EnableCollision();
            }
            else
            {
                _vehicle.Opacity = 200;
                _vehicle.SetNoCollision(Cache.PlayerPed, false);

                if (Cache.PlayerPed.IsInVehicle())
                {
                    _vehicle.SetNoCollision(Cache.PlayerPed.CurrentVehicle, false);
                    Cache.PlayerPed.CurrentVehicle.SetNoCollision(_vehicle, false);
                }
            }
        }

        public void EnableCollision()
        {
            PluginManager.DetachTickHandler(DisableCollision);

            _vehicle.ResetOpacity();

            _vehicle.SetNoCollision(Game.PlayerPed, true);
            Game.Player.Character.SetNoCollision(_vehicle, true);

            if (Game.PlayerPed.IsInVehicle())
            {

                _vehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, true);
                Game.PlayerPed.CurrentVehicle.SetNoCollision(_vehicle, true);
            }
        }
    }
}
