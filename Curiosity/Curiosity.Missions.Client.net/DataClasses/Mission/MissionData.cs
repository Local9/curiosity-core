using CitizenFX.Core;
using System.Collections.Generic;

namespace Curiosity.Missions.Client.net.DataClasses.Mission
{
    class MissionData
    {
        public string Name;
        public Vector3 Location;
        public bool HostageRescueRequired;
        public string AudioStart;
        public string AudioEnd;
        public List<MissionPedData> MissionGangOne = new List<MissionPedData>();
        public List<MissionPedData> MissionGangTwo = new List<MissionPedData>();
        public List<MissionPedData> Hostages = new List<MissionPedData>();

        public float ResurectionRange { get; internal set; }
        public float SpawnRange { get; internal set; }
        public Blip Blip { get; internal set; }
    }
}
