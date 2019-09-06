using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Global.Shared.net.Entity
{
    public class Mission
    {
        public string name;
        public Vector3 location;
        public MissionType missionType;
        public MissionDifficulty missionDifficulty;
    }
}
