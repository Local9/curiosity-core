using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.net.Scripts.Mission
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
