using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models.Config;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Scripts.JobPolice;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        const string REPLACE_MAKE_NAME = "MAKE_NAME";
        const float LIMIT_BUFFER = 5f;

        float _speedCameraDistance;
        float _currentStreetLimit = 0;
        string _currentStreet;
        float _lastSpeedLimit = 0;

        public bool isDebugging = false;
        Vehicle currentVehicle;

        public override void Begin()
        {
            EventSystem.Attach("police:notify", new EventCallback(metadata =>
            {
                int notificationType = metadata.Find<int>(0);
                string notificationMessage = metadata.Find<string>(1);
                int notificationDuration = metadata.Find<int>(2);
                int notificationVehicle = metadata.Find<int>(3);

                string vehicleName = "Unknown";

                if (notificationVehicle > 0)
                {
                    int entityHandle = NetworkGetEntityFromNetworkId(notificationVehicle);
                    if (DoesEntityExist(entityHandle))
                    {
                        Vehicle vehicle = new Vehicle(entityHandle);
                        string name = vehicle.DisplayName;
                        vehicleName = Game.GetGXTEntry(name);
                        notificationMessage = notificationMessage.Replace(REPLACE_MAKE_NAME, vehicleName);
                    }
                }

                    NotificationManager.GetModule().SendNui((eNotification)notificationType, notificationMessage, "bottom-right", "snackbar", notificationDuration, true, false);

                return null;
            }));

            GameEventManager.OnEnteredVehicle += GameEventManager_OnEnteredVehicle;
        }

        private void GameEventManager_OnEnteredVehicle(Player player, Vehicle vehicle)
        {
            if (player.Character != vehicle.Driver) return;

            if (currentVehicle != vehicle)
                currentVehicle = vehicle;

            string vehicleDisplayName = GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash);
            if (IsInvalidVehicle(vehicle)) return;

            Instance.AttachTickHandler(OnSpeedTest);
            Instance.AttachTickHandler(OnSpeedCameraCheck);
            Instance.AttachTickHandler(OnShowSpeedLimit);

            _speedCameraDistance = PoliceConfig.SpeedCameraDistance;

            Logger.Info($"Speed Cameras Active");
        }

        public void Dispose()
        {
            Instance.DetachTickHandler(OnSpeedTest);
            Instance.DetachTickHandler(OnSpeedCameraCheck);
            Instance.DetachTickHandler(OnShowSpeedLimit);
        }

        private async Task OnShowSpeedLimit()
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

            if (PoliceConfig.SpeedLimits.ContainsKey($"{streetHash}"))
            {
                string street = GetStreetNameFromHashKey(streetHash);
                _currentStreet = street;
                _currentStreetLimit = PoliceConfig.SpeedLimits[$"{streetHash}"];

                if (_lastSpeedLimit != _currentStreetLimit)
                {
                    _lastSpeedLimit = _currentStreetLimit;
                    Notify.Info($"<center><b>📸 Speed Limit : {_currentStreetLimit} MPH</b></center>", "bottom-middle");
                }
            }
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

            if (PoliceConfig.SpeedLimits.ContainsKey($"{streetHash}"))
            {
                _currentStreet = street;
                _currentStreetLimit = PoliceConfig.SpeedLimits[$"{streetHash}"];
            }
            else
            {
                Logger.Debug($"{street}:{streetHash} is unknown, please inform the dev team.");
            }
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

            string direction = Common.GetVehicleHeadingDirection();
            List<PoliceCamera> closestCameras = GetClosestCamera(currentVehicle.Position, _speedCameraDistance);

            if (closestCameras.Count == 0) return;
            foreach(PoliceCamera camera in closestCameras)
            {
                camera.Active = false;

                if (camera.Direction != direction) continue;
                float currentSpeed = currentVehicle.Speed;
                int speedInMph = (int)(currentSpeed * CONVERT_SPEED_MPH);

                if (isDebugging)
                    Screen.ShowSubtitle($"{camera.Direction} / {direction} : {speedInMph}");

                if (_currentStreetLimit == 0) continue;
                Vector3 start = camera.Start.Vector3;
                Vector3 end = camera.End.Vector3;

                if (!Common.IsEntityInAngledArea(currentVehicle, start, end, camera.Width ?? PoliceConfig.SpeedCameraWidth, debug: isDebugging)) continue;

                camera.Active = true;

                bool caughtSpeeding = false;
                float limitToReport = 0;

                if (camera.Limit is not null)
                {
                    if (speedInMph > (int)(camera.Limit + LIMIT_BUFFER))
                    {
                        limitToReport = camera.Limit ?? 0f;
                        caughtSpeeding = true;
                    }
                }
                else
                {
                    if (speedInMph > (int)(_currentStreetLimit + LIMIT_BUFFER))
                    {
                        limitToReport = _currentStreetLimit;
                        caughtSpeeding = true;
                    }
                }

                if (caughtSpeeding)
                {
                    Game.PlaySound("Camera_Shoot", "Phone_Soundset_Franklin");
                    EventSystem.Send("police:ticket:speeding", (int)speedInMph, (int)limitToReport, currentVehicle.NetworkId, _currentStreet, direction);
                    Logger.Debug($"Player speeding: {speedInMph} / {limitToReport} / {_currentStreet} / {direction}");
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
