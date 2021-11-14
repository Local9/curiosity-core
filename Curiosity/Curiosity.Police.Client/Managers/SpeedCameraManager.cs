using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            
        }

        public void Dispose()
        {

        }
    }
}
