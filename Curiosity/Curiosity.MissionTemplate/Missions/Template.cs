using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Curiosity.Systems.Library.Enums;

namespace Curiosity.MissionTemplate.Missions
{
    [MissionInfo("Template", "Template", 0f, 0f, 0f, MissionType.Mission, true, "None")]
    public class Template : Mission
    {
        public async override void Start()
        {

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {

        }
    }
}
