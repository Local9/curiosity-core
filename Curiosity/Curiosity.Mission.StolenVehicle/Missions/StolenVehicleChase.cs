using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Curiosity.StolenVehicle.Missions
{
    [MissionInfo("Stolen Vehicle", "misSvChase", 485.979f, -1311.222f, 29.249f, MissionType.StolenVehicle, true, "None")]
    public class StolenVehicleChase : Mission
    {
        private MissionState missionState;
        private Ped criminal;
        private Vehicle stolenVehicle;

        List<VehicleHash> vehicleHashes = new List<VehicleHash>()
        {
            VehicleHash.Oracle2,
            VehicleHash.Panto,
            VehicleHash.Sandking,
            VehicleHash.SlamVan,
            VehicleHash.Adder,
            VehicleHash.Faggio,
            VehicleHash.Issi2,
            VehicleHash.Kuruma,
            VehicleHash.F620,
            VehicleHash.Dukes,
            VehicleHash.Baller,
            VehicleHash.Boxville,
            VehicleHash.Rumpo
        };

        List<PedHash> pedHashes = new List<PedHash>()
        {
            PedHash.Tourist01AFM,
            PedHash.Tourist01AFY,
            PedHash.Lost01GFY,
            PedHash.Lost01GMY,
        };

        List<WeaponHash> weaponHashes = new List<WeaponHash>()
        {
            WeaponHash.Pistol,
            WeaponHash.SMG,
            WeaponHash.MicroSMG
        };

        public async override void Start()
        {
            missionState = MissionState.Started;

            Screen.ShowNotification("Mission Started...");
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);

            Screen.ShowNotification("Mission Ended...");
        }

        async Task OnMissionTick()
        {
            if (criminal.Position.Distance(Game.PlayerPed.Position) > 600f)
            {
                missionState = MissionState.Escaped;
            }

            switch (missionState)
            {
                case MissionState.Started:
                    break;
                case MissionState.Escaped:
                    Fail("Vehicle got away....");
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
        ChaseActive,
        Escaped,
        End
    }
}
