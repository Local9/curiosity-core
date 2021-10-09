using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.Systems.Library.Enums;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.Quest.Missions
{
    [MissionInfo("Template", "Template", 0f, 0f, 0f, MissionType.Mission, true, "None")]
    public class Template : Mission
    {
        public override void Start()
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
