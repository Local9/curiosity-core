﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
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
        Ped driver;

        public override void Start()
        {
            veh = TrafficStopManager.Manager.tsVehicle;
            driver = TrafficStopManager.Manager.tsDriver;


            // https://runtime.fivem.net/doc/natives/?_0x0FA6E4B75F302400
            // Pull over this will try to avoid
            API.TaskVehicleEscort(driver.Handle, veh.Handle, Game.PlayerPed.CurrentVehicle.Handle, 0, 15f, (int)Collections.CombinedVehicleDrivingFlags.Normal, 8f, 0, 0f);

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
