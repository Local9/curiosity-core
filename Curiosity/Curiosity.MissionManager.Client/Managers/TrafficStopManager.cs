using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.MissionManager.Client.Managers
{
    public class TrafficStopManager : Manager<TrafficStopManager>
    {
        public static TrafficStopManager Manager;

        public Vehicle tsVehicle { get; private set; }

        public override void Begin()
        {
            Manager = this;
        }

        public void SetVehicle(Vehicle vehicle)
        {
            tsVehicle = vehicle;
        }

        public void ClearVehicle()
        {
            tsVehicle = null;
        }
    }
}
