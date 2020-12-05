using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.MissionManager.Client.Managers
{
    public class TrafficStopManager : Manager<TrafficStopManager>
    {
        public static TrafficStopManager Manager;

        public Vehicle tsVehicle { get; private set; }
        public Ped tsDriver { get; private set; }

        public override void Begin()
        {
            Manager = this;
        }

        public void SetVehicle(Vehicle vehicle)
        {
            tsVehicle = vehicle;
            tsDriver = new Ped(vehicle.Fx.Driver);
        }

        public void ClearVehicle()
        {
            tsVehicle = null;
            tsDriver = null;
        }
    }
}
