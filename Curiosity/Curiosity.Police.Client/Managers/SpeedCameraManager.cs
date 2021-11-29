using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Threading.Tasks;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using Curiosity.Police.Client.Environment.Entities.Models;
using System.Linq;
using Curiosity.Police.Client.Environment.Data;

namespace Curiosity.Police.Client.Managers
{
    public class SpeedCameraManager : Manager<SpeedCameraManager>
    {
        // base premise from Big-Yoda
        // https://github.com/Big-Yoda/Posted-Speedlimit
        
        ConfigurationManager _configurationManager => ConfigurationManager.GetModule();
        const float CONVERT_SPEED_MPH = 2.236936f;
        const float CONVERT_SPEED_KPH = 3.6f;
        
        float _speedCameraDistance;
        float _currentStreetLimit = 0;
        string _currentStreet;
        public override void Begin() => GameEventManager.OnEnteredVehicle += GameEventManager_OnEnteredVehicle;

        private void GameEventManager_OnEnteredVehicle(Player player, Vehicle vehicle)
        {
            if (player.Character != vehicle.Driver) return;

            string vehicleDisplayName = GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash);
            if (_configurationManager.IgnoredVehicles.Contains(vehicleDisplayName)) return;
            if (IsInvalidVehicle(vehicle)) return;

            PluginManager.Instance.AttachTickHandler(OnSpeedTest);
            PluginManager.Instance.AttachTickHandler(OnSpeedCameraCheck);

            _speedCameraDistance = _configurationManager.SpeedCameraDistance;
        }

        public void Dispose()
        {
            Instance.DetachTickHandler(OnSpeedTest);
            Instance.DetachTickHandler(OnSpeedCameraCheck);
        }

        private bool IsInvalidVehicle(Vehicle vehicle)
        {
            VehicleClass vehicleClass = vehicle.ClassType;
            return
                vehicleClass == VehicleClass.Planes
                || vehicleClass == VehicleClass.Boats
                || vehicleClass == VehicleClass.Helicopters
                || vehicleClass == VehicleClass.Cycles
                || vehicleClass == VehicleClass.Trains
                || vehicleClass == VehicleClass.Emergency;
        }

        private async Task OnSpeedTest() // limiter to show, but not report
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                Dispose();
                return;
            }

            Vector3 pos = Game.PlayerPed.CurrentVehicle.Position;
            uint streetHash = 0;
            uint crossingRoad = 0;
            GetStreetNameAtCoord(pos.X, pos.Y, pos.Z, ref streetHash, ref crossingRoad);

            if (streetHash == 0) return;

            string street = GetStreetNameFromHashKey(streetHash);

            if (_configurationManager.SpeedLimits.ContainsKey(street))
            {
                _currentStreet = street;
                _currentStreetLimit = _configurationManager.SpeedLimits[street];
                //Screen.ShowSubtitle($"Speed Limit: {_currentStreetLimit}");
            }
            else
            {
                Screen.ShowNotification($"{street} is unknown, please inform the dev team.");
            }
        }

        public string GetVehicleHeadingDirection()
        {
            if (!Game.PlayerPed.IsInVehicle()) return "U";

            foreach(KeyValuePair<int, string> kvp in CompassDirections.Direction)
            {
                float vehDirection = Game.PlayerPed.CurrentVehicle.Heading;
                if (Math.Abs(vehDirection - kvp.Key) < 22.5)
                {
                    return kvp.Value;
                }
            }

            return "U";
        }

        public List<PoliceCamera> GetClosestCamera(Vector3 position, float distance)
        {
            return _configurationManager.SpeedCameras
                    .Where(x => Vector3.Distance(position, x.Position) < distance)
                    .OrderBy(x => Vector3.Distance(position, x.Position)).ToList();
        }

        private async Task OnSpeedCameraCheck()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                Dispose();
                return;
            }

            Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
            string direction = GetVehicleHeadingDirection();
            List<PoliceCamera> closestCameras = GetClosestCamera(vehicle.Position, _speedCameraDistance);
            if (closestCameras.Count == 0) return;
            foreach(PoliceCamera camera in closestCameras)
            {
                camera.Active = false;

                if (camera.Direction != direction) continue;
                float currentSpeed = Game.PlayerPed.CurrentVehicle.Speed;
                float speedInMph = currentSpeed * CONVERT_SPEED_MPH;

                if (_currentStreetLimit == 0) continue;
                Vector3 p = camera.Position;
                float low = p.Z - 0.5f;
                float high = p.Z + 1f;

                if (!Between(vehicle.Position.Z, low, high)) continue;

                camera.Active = true;

                bool informPolice = false; // legacy
                bool caughtSpeeding = false;
                float limitToReport = 0;

                if (camera.Limit is not null)
                {
                    if (speedInMph > camera.Limit)
                    {
                        limitToReport = camera.Limit ?? 0f;
                        caughtSpeeding = true;
                    }
                }
                else
                {
                    if (speedInMph > _currentStreetLimit)
                    {
                        limitToReport = _currentStreetLimit;
                        caughtSpeeding = true;
                    }
                }

                if (caughtSpeeding)
                {
                    EventSystem.Send("police:ticket:speeding", (int)speedInMph, (int)limitToReport, informPolice, vehicle.NetworkId, _currentStreet, direction);
                    await BaseScript.Delay(5000);
                    camera.Active = false;
                }

                
            }
        }

        private void ShowNotification(string msg)
        {
            Screen.ShowNotification($"{msg}~n~{DateTime.UtcNow.Millisecond}");
        }

        public bool Between(float number, float min, float max)
        {
            return number >= min && number <= max;
        }
    }
}
