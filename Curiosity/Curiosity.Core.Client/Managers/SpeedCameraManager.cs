using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models.Config;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Scripts.JobPolice;
using Curiosity.Core.Client.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class SpeedCameraManager : Manager<SpeedCameraManager>
    {
        // base premise from Big-Yoda
        // https://github.com/Big-Yoda/Posted-Speedlimit

        const float CONVERT_SPEED_MPH = 2.236936f;
        const float CONVERT_SPEED_KPH = 3.6f;
        
        float _speedCameraDistance;
        float _currentStreetLimit = 0;
        string _currentStreet;

        public bool isDebugging = false;
        Vehicle currentVehicle;

        public override void Begin() => GameEventManager.OnEnteredVehicle += GameEventManager_OnEnteredVehicle;

        private void GameEventManager_OnEnteredVehicle(Player player, Vehicle vehicle)
        {
            if (player.Character != vehicle.Driver) return;

            if (currentVehicle != vehicle)
                currentVehicle = vehicle;

            string vehicleDisplayName = GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash);
            if (PoliceConfig.IgnoredVehicles.Contains(vehicleDisplayName)) return;
            if (IsInvalidVehicle(vehicle)) return;

            Instance.AttachTickHandler(OnSpeedTest);
            Instance.AttachTickHandler(OnSpeedCameraCheck);

            _speedCameraDistance = PoliceConfig.SpeedCameraDistance;

            Logger.Info($"Speed Cameras Active");
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

            Vector3 pos = currentVehicle.Position;
            uint streetHash = 0;
            uint crossingRoad = 0;
            GetStreetNameAtCoord(pos.X, pos.Y, pos.Z, ref streetHash, ref crossingRoad);

            if (streetHash == 0) return;

            string street = GetStreetNameFromHashKey(streetHash);

            if (PoliceConfig.SpeedLimits.ContainsKey(street))
            {
                _currentStreet = street;
                _currentStreetLimit = PoliceConfig.SpeedLimits[street];
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

            foreach(KeyValuePair<int, string> kvp in Common.WorldCompassDirection)
            {
                float vehDirection = currentVehicle.Heading;
                if (Math.Abs(vehDirection - kvp.Key) < 22.5)
                {
                    return kvp.Value;
                }
            }

            return "U";
        }

        public List<PoliceCamera> GetClosestCamera(Vector3 position, float distance)
        {
            return PoliceConfig.SpeedCameras
                    .Where(x => position.Distance(x.Center) < distance)
                    .OrderBy(x => position.Distance(x.Center)).ToList();
        }

        private async Task OnSpeedCameraCheck()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                Dispose();
                return;
            }

            if (currentVehicle.ClassType == VehicleClass.Emergency)
            {
                if (currentVehicle.IsSirenActive) return;
            }

            string direction = GetVehicleHeadingDirection();
            List<PoliceCamera> closestCameras = GetClosestCamera(currentVehicle.Position, _speedCameraDistance);

            if (closestCameras.Count == 0) return;
            foreach(PoliceCamera camera in closestCameras)
            {
                camera.Active = false;

                if (isDebugging)
                    Screen.ShowSubtitle($"{camera.Direction} / {direction}");

                if (camera.Direction != direction) continue;
                float currentSpeed = currentVehicle.Speed;
                float speedInMph = currentSpeed * CONVERT_SPEED_MPH;

                if (_currentStreetLimit == 0) continue;
                Vector3 start = camera.Start.Vector3;
                Vector3 end = camera.End.Vector3;

                if (!Common.IsEntityInAngledArea(currentVehicle, start, end, camera.Width ?? PoliceConfig.SpeedCameraWidth, debug: isDebugging)) continue;

                camera.Active = true;

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
                    EventSystem.Send("police:ticket:speeding", (int)speedInMph, (int)limitToReport, false, currentVehicle.NetworkId, _currentStreet, direction);
                    await BaseScript.Delay(5000);
                    camera.Active = false;
                }

                
            }
        }

        public bool Between(float number, float min, float max)
        {
            return number >= min && number <= max;
        }
    }
}
