using CitizenFX.Core;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.TrafficStops.Missions
{
    [MissionInfo("Traffic Stop", "misTrafficStop", 0f, 0f, 0f, MissionType.TrafficStop, true, "None")]
    public class TrafficStop : Mission
    {
        Vehicle veh;

        public override void Start()
        {
            veh = TrafficStopManager.Manager.tsVehicle;

            if (veh == null)
                Stop(EndState.Error);

            if (!veh.Exists())
                Stop(EndState.Error);

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
