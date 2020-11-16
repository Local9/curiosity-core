using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.ParkingMeters.Models;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;
using Vehicle = Curiosity.MissionManager.Client.Classes.Vehicle;

namespace Curiosity.ParkingMeters.Missions
{
    [MissionInfo("Parking Meters", "misGenParkingMeter", 0f, 0f, 0f, MissionType.Mission, true, "None")]
    public class ParkingMeterTickets : Mission
    {
        List<ParkingMeter> parkingMeters = new List<ParkingMeter>();

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
