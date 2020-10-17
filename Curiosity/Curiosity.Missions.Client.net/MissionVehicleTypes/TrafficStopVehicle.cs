using CitizenFX.Core;
using Curiosity.Missions.Client.MissionVehicles;

namespace Curiosity.Missions.Client.MissionVehicleTypes
{
    internal class TrafficStopVehicle : InteractiveVehicle
    {
        private readonly Vehicle _vehicle;

        public TrafficStopVehicle(int handle) : base(handle)
        {
            this._vehicle = this;
        }
    }
}
