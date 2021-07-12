using CitizenFX.Core;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.Systems.Library.Enums;

namespace Curiosity.MissionManager.Client.Manager
{
    public class PlayerManager : Manager<PlayerManager>
    {
        public Vehicle PersonalVehicle;

        public PlayerManager()
        {

        }

        public void SetVehicle(int vehicleId)
        {
            PersonalVehicle = new Vehicle(vehicleId);

            string playerName = PersonalVehicle.State.Get(StateBagKey.VEH_OWNER) ?? string.Empty;

            if (!string.IsNullOrEmpty(playerName))
                Logger.Debug($"Vehicle for '{playerName}' assigned.");
        }
    }
}
