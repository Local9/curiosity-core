﻿using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Threading.Tasks;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;

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

        float _currentAreaLimit = 0;

        public override void Begin() => GameEventManager.OnEnteredVehicle += GameEventManager_OnEnteredVehicle;

        private void GameEventManager_OnEnteredVehicle(Player player, Vehicle vehicle)
        {
            if (player.Character != vehicle.Driver) return;

            string vehicleDisplayName = GetDisplayNameFromVehicleModel((uint)vehicle.Model.Hash);

            Screen.ShowNotification(vehicleDisplayName);

            if (_configurationManager.IgnoredVehicles.Contains(vehicleDisplayName)) return;

            PluginManager.Instance.AttachTickHandler(OnSpeedTest);
        }

        public void Dispose()
        {
            Instance.DetachTickHandler(OnSpeedTest);
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

            if (_configurationManager.SpeedCameras.ContainsKey(street))
            {
                //int speedLimit = _configurationManager.SpeedCameras[street];
                //float currentSpeed = Game.PlayerPed.CurrentVehicle.Speed;
                //float speedInMph = currentSpeed * CONVERT_SPEED_MPH;

                _currentAreaLimit = _configurationManager.SpeedCameras[street];
            }
            else
            {
                Screen.ShowNotification($"{street} is unknown, please inform the dev team.");
            }

        DELAY_RETURN:
            return BaseScript.Delay(5000);
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
    }
}
