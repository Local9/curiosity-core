using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Threading.Tasks;
using CitizenFX.Core.UI;
using System;

namespace Curiosity.Police.Client.Managers
{
    public class SpeedCameraManager : Manager<SpeedCameraManager>
    {
        // base premise from Big-Yoda
        // https://github.com/Big-Yoda/Posted-Speedlimit
        
        ConfigurationManager _configurationManager => ConfigurationManager.GetModule();
        const float CONVERT_SPEED_MPH = 2.236936f;
        const float CONVERT_SPEED_KPH = 3.6f;

        public override void Begin()
        {
            GameEventManager.OnEnteredVehicle += GameEventManager_OnEnteredVehicle;
        }

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

        private Task OnSpeedTest()
        {
            if (!Game.PlayerPed.IsInVehicle())
            {
                Dispose();
                goto DELAY_RETURN;
            }

            Vector3 playerPos = Game.PlayerPed.Position;
            uint streetHash = 0;
            uint crossingRoad = 0;
            GetStreetNameAtCoord(playerPos.X, playerPos.Y, playerPos.Z, ref streetHash, ref crossingRoad);

            if (streetHash == 0) goto DELAY_RETURN;

            string street = GetStreetNameFromHashKey(streetHash);

            if (_configurationManager.SpeedCameras.ContainsKey(street))
            {
                int speedLimit = _configurationManager.SpeedCameras[street];
                float currentSpeed = Game.PlayerPed.CurrentVehicle.Speed;
                float speedInMph = currentSpeed * CONVERT_SPEED_MPH;

                if (speedLimit <= 0) goto DELAY_RETURN; // no limit

                if (speedInMph > speedLimit)
                {
                    Screen.ShowNotification($"Speeding: {street} {speedLimit}mph");
                }
            }
            else
            {
                Screen.ShowNotification($"{street} is Unknown");
            }

        DELAY_RETURN:
            return BaseScript.Delay(2000);
        }
    }
}
