using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using System.Threading.Tasks;

namespace Curiosity.StolenVehicle.Missions
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
