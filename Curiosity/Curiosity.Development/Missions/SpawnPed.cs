using CitizenFX.Core;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.Development.Missions
{
    [MissionInfo("Development Ped", "devPed", 0f, 0f, 0f, MissionType.Developer, true, "None")]
    public class SpawnPed : Mission
    {
        private Ped criminal;

        List<PedHash> pedHashes = new List<PedHash>()
        {
            PedHash.Tourist01AFM,
            PedHash.Tourist01AFY,
            PedHash.Lost01GFY,
            PedHash.Lost01GMY,
        };

        bool missionPassed = false;

        public async override void Start()
        {
            criminal = await PedSpawn(pedHashes.Random(), Players[0].Character.Position.Around(3f, 4f), sidewalk: true);

            if (criminal == null)
            {
                Stop(EndState.Error);
                return;
            }

            criminal.IsImportant = true;
            criminal.IsMission = true;
            criminal.IsSuspect = true;
            criminal.AttachSuspectBlip();

            Mission.RegisterPed(criminal);

            Notify.Info($"Ped Created");

            Game.PlayerPed.Task.TurnTo(criminal.Fx);

            await BaseScript.Delay(2000);

            Game.PlayerPed.Task.ClearAllImmediately();
            Game.PlayerPed.Task.StartScenario("forcestop", Game.PlayerPed.Position);

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            if (Mission.NumberPedsArrested > 0 && !missionPassed)
            {
                missionPassed = true;
                Pass();
            }

            if (criminal.IsDead)
            {
                missionPassed = false;
                Fail("Suspect is dead", EndState.Fail);
            }
        }
    }
}
