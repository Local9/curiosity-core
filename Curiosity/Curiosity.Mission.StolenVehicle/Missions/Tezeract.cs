using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.Shared.Client.net;
using System.Threading.Tasks;

namespace Curiosity.StolenVehicle.Missions
{
    [MissionInfo("Stolen Vehicle", "misSvTezeract", 485.979f, -1311.222f, 29.249f, MissionType.StolenVehicle, true, "None")]
    public class Tezeract : Mission
    {
        MissionState missionState;

        public override void Start()
        {
            Screen.ShowNotification("Mission Start...");
            missionState = MissionState.Started;

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);

            Screen.ShowNotification("Mission Ended...");
        }

        async Task OnMissionTick()
        {
            switch(missionState)
            {
                case MissionState.Started:

                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to end mission.");

                    if (Game.IsControlPressed(0, Control.Context))
                        missionState = MissionState.End;
                    break;
                case MissionState.End:
                    Pass();
                    break;
            }
        }
    }

    enum MissionState
    {
        Started,
        End
    }
}
