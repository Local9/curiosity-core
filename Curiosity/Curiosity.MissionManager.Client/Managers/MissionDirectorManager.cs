using Curiosity.MissionManager.Client.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Managers
{
    public class MissionDirectorManager : Manager<MissionDirectorManager>
    {
        public static bool MissionDirectorState = false;

        public static void ToggleMissionDirector()
        {
            MissionDirectorState = !MissionDirectorState;
            string state = MissionDirectorState ? "~g~Enabled" : "~o~Disabled";
            Notify.Info($"~b~Dispatch A.I. {state}");
        }

        public override void Begin()
        {
            
        }
    }
}
