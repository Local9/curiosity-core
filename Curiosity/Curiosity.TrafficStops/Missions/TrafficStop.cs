using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Interface;
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

        MissionState missionState;

        public override void Start()
        {
            veh = TrafficStopManager.Manager.tsVehicle;
            veh.AddToMission();

            Logger.Debug($"Traffic Stop -> Vehicle {veh.Handle}");

            driver = TrafficStopManager.Manager.tsDriver;
            driver.AddToMission();

            Logger.Debug($"Traffic Stop -> Driver {driver.Handle}");

            RegisteredVehicles.Add(veh);
            RegisteredPeds.Add(driver);

            TrafficStopManager.Manager.tsPassengers.ForEach(p =>
            {
                Logger.Debug($"Traffic Stop -> Passenger {p.Handle}");

                RegisteredPeds.Add(p);
                p.AddToMission();
            });

            // https://runtime.fivem.net/doc/natives/?_0x0FA6E4B75F302400
            // Pull over this will try to avoid
            API.TaskVehicleEscort(driver.Handle, veh.Handle, Game.PlayerPed.CurrentVehicle.Handle, 0, 8f, (int)Collections.CombinedVehicleDrivingFlags.Normal, 5f, 0, 0f);

            Notify.Info($"Vehicle is attempting to pull over.");

            missionState = MissionState.AwaitingVehicleToStop;

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            int totalNumberOfPlayers = Players.Count;
            int playersDead = 0;
            Players.ForEach(p =>
            {
                if (p.IsDead)
                    playersDead++;
            });

            if (playersDead == totalNumberOfPlayers)
                Fail("All Players have died.", EndState.FailPlayersDead);
        }

        enum MissionState
        {
            AwaitingVehicleToStop,
        }
    }
}
