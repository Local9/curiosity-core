using CitizenFX.Core;
using System.Collections.Generic;

namespace Curiosity.Missions.Client.net.DataClasses.Mission
{
    class Store
    {
        public string Name;
        public Vector3 Location;
        public List<MissionPedData> missionPeds = new List<MissionPedData>();
        public List<MissionPedData> hostages = new List<MissionPedData>();
    }
}
