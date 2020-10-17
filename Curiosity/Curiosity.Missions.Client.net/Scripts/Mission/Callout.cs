using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// using Ped = Curiosity.Callouts.Client.Classes.Ped;
using Vehicle = Curiosity.Missions.Client.Classes.Vehicle;

namespace Curiosity.Missions.Client.Scripts.Mission
{
    [Serializable]
    internal abstract class Callout
    {
        static Client clientInstance = Client.GetInstance();
        public event Action<bool> Ended;
        public bool IsSetup = false;
        public bool IsRunning = false;

        internal virtual string Name { get; set; }

        protected internal List<Player> Players { get; }
        public List<Vehicle> RegisteredVehicles { get; }

        protected int progress = 1;
    }
}
