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


        public async override void Start()
        {
            criminal = await Ped.Spawn(pedHashes.Random(), Players[0].Character.Position.Around(3f, 4f), sidewalk: true);

            if (criminal == null)
            {
                Stop(EndState.Error);
                return;
            }

            criminal.IsImportant = true;
            criminal.IsMission = true;
            criminal.IsSuspect = true;

            Blip b = criminal.AttachBlip();
            b.Color = BlipColor.Red;
            b.Scale = .5f;
            b.Sprite = BlipSprite.Enemy;

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

        }
    }
}
