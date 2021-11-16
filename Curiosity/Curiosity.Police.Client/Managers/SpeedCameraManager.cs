using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Threading.Tasks;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using Curiosity.Police.Client.Environment.Entities.Models;
using System.Linq;

namespace Curiosity.Police.Client.Managers
{
    public class SpeedCameraManager : Manager<SpeedCameraManager>
    {
        // base premise from Big-Yoda
        // https://github.com/Big-Yoda/Posted-Speedlimit
        
        ConfigurationManager _configurationManager => ConfigurationManager.GetModule();
        const float CONVERT_SPEED_MPH = 2.236936f;
        const float CONVERT_SPEED_KPH = 3.6f;

        Dictionary<int, string> _cameraDirection = new()
        {
            { 0, "N" },
            { 45, "NW" },
            { 90, "W" },
            { 135, "SW" },
            { 180, "S" },
            { 225, "SE" },
            { 270, "E" },
            { 315, "NE" },
            { 360, "N" }
        };

        float _speedCameraDistance;
        float _currentStreetLimit = 0;
        string _currentStreet;

        public override void Begin() => GameEventManager.OnEnteredVehicle += GameEventManager_OnEnteredVehicle;

        private void GameEventManager_OnEnteredVehicle(Player player, Vehicle vehicle)
        {
            if (player.Character != vehicle.Driver) return;

            string vehicleDisplayName = GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash);

            Screen.ShowNotification(vehicleDisplayName);

            if (_configurationManager.IgnoredVehicles.Contains(vehicleDisplayName)) return;

            PluginManager.Instance.AttachTickHandler(OnSpeedTest);
            PluginManager.Instance.AttachTickHandler(OnSpeedCameraCheck);

            _speedCameraDistance = _configurationManager.SpeedCameraDistance;
        }

        public void Dispose()
        {
            Instance.DetachTickHandler(OnSpeedTest);
            Instance.DetachTickHandler(OnSpeedCameraCheck);
        }

        private Task OnSpeedTest() // limiter to show, but not report
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                Dispose();
                return BaseScript.Delay(0);
            }

            Vector3 playerPos = Game.PlayerPed.Position;
            uint streetHash = 0;
            uint crossingRoad = 0;
            GetStreetNameAtCoord(playerPos.X, playerPos.Y, playerPos.Z, ref streetHash, ref crossingRoad);

            if (streetHash == 0)
            {
                goto DELAY_RETURN;
            }

            string street = GetStreetNameFromHashKey(streetHash);

            if (_configurationManager.SpeedLimits.ContainsKey(street))
            {
                //int speedLimit = _configurationManager.SpeedCameras[street];
                //float currentSpeed = Game.PlayerPed.CurrentVehicle.Speed;
                //float speedInMph = currentSpeed * CONVERT_SPEED_MPH;
                _currentStreet = street;
                _currentStreetLimit = _configurationManager.SpeedLimits[street];
            }
            else
            {
                Screen.ShowNotification($"{street} is unknown, please inform the dev team.");
            }

        DELAY_RETURN:
            return BaseScript.Delay(2000);
        }

        public string GetVehicleHeadingDirection()
        {
            if (!Game.PlayerPed.IsInVehicle()) return "U";

            foreach(KeyValuePair<int, string> kvp in _cameraDirection)
            {
                float vehDirection = Game.PlayerPed.CurrentVehicle.Heading;
                if (Math.Abs(vehDirection - kvp.Key) < 22.5)
                {
                    return kvp.Value;
                }
            }

            return "U";
        }

        public List<SpeedCamera> GetClosestCamera(Vector3 position, float distance)
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
            List<SpeedCamera> closestCameras = GetClosestCamera(vehicle.Position, _speedCameraDistance);
            if (closestCameras.Count == 0) return;
            foreach(SpeedCamera camera in closestCameras)
            {
                if (camera.Direction != direction) continue;
                float currentSpeed = Game.PlayerPed.CurrentVehicle.Speed;
                float speedInMph = currentSpeed * CONVERT_SPEED_MPH;

                if (speedInMph > _currentStreetLimit)
                {
                    Screen.ShowNotification($"Speeding!~n~{_currentStreet}~n~{_currentStreetLimit}mph");
                }
            }
        }
    }
}
