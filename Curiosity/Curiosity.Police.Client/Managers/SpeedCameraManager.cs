using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.Managers
{
    public class SpeedCameraManager : Manager<SpeedCameraManager>
    {
        // base premise from Big-Yoda
        // https://github.com/Big-Yoda/Posted-Speedlimit
        
        ConfigurationManager _configurationManager => ConfigurationManager.GetModule();

        public override void Begin()
        {
            GameEventManager.OnEnteredVehicle += GameEventManager_OnEnteredVehicle;
        }

        private void GameEventManager_OnEnteredVehicle(Player player, Vehicle vehicle)
        {
            if (player.Character != vehicle.Driver) return;
        }

        public void Dispose()
        {

        }

        private Task OnSpeedTest()
        {
            Vector3 playerPos = Game.PlayerPed.Position;
            uint streetHash = 0;
            uint crossingRoad = 0;
            GetStreetNameAtCoord(playerPos.X, playerPos.Y, playerPos.Z, ref streetHash, ref crossingRoad);

            if (streetHash == 0) return Task.Delay(2000);

            string street = GetStreetNameFromHashKey(streetHash);



            return Task.Delay(2000);
        }
    }
}
