using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.DataClasses.Mission
{
    class Store
    {
        public string Name;
        public Vector3 Location;
        public List<MissionPedData> missionPeds;
    }
}
