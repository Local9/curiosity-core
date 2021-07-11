using CitizenFX.Core;
using Curiosity.MissionManager.Client.Managers;

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
        }
    }
}
