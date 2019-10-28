using CitizenFX.Core;
using Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Global.Shared.net.Entity
{
    public class Mission
    {
        // CORE
        public string title;
        public string author;
        public string description;
        public Vector3 location;
        // Mission Settings
        public MissionType missionType;
        public MissionDifficulty missionDifficulty;
        public ObjectiveType objectiveType;
        // Objectives...

        // Mission Actors
        
        // Mission Vehicles
        
        // Mission Props
        
        // Mission Pickups
        
        // Mission Messages
        
        // Mission Teleports

    }

    public class MissionMessage
    {
        int _missionCompleted = 0;
        
        public string MissionTitle;
        
        public int MissionCompleted
        {
            get
            {
                return _missionCompleted > 0 ? 1 : 0;
            }
            set
            {
                _missionCompleted = value > 0 ? 1 : 0;
            }
        }

        public int HostagesRescued = 0;

        public int MoneyEarnt;
        public int MoneyLost;

        public MissionMessage(string title)
        {
            this.MissionTitle = title;
        }
    }

    public class MissionCreate
    {
        public int MissionId;
        public int PatrolZone;

        public override string ToString()
        {
            return $"MissionId: {MissionId}, PatrolZone: {PatrolZone}";
        }
    }
}
