using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Missions.Client.net.MissionVehicles;

namespace Curiosity.Missions.Client.net.MissionVehicleTypes
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
