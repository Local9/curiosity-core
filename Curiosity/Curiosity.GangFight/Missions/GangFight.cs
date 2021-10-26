using CitizenFX.Core;
using FxPed = CitizenFX.Core.Ped;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.Mugging.Missions
{
    [MissionInfo("Gang Fight", "misGf", 0f, 0f, 0f, MissionType.Developer, true, "None", PatrolZone.Anywhere)]
    public class GangFight : Mission
    {
        MissionState missionState;
        List<FxPed> pedsToTrack = new List<FxPed>();
        Vector3 gangArea = new Vector3(-180.0207f, -1545.602f, 35.08198f); // Families: Chamberlin Hills

        RelationshipGroup gangRelationship1;
        RelationshipGroup gangRelationship2;
        RelationshipGroup playerRelationship;

        List<PedHash> models = new List<PedHash>()
        {
            PedHash.Lost01GFY,
            PedHash.Lost01GMY,
            PedHash.Lost02GMY,
            PedHash.Lost03GMY,
            PedHash.Families01GFY,
            // BALLA
            PedHash.BallaEast01GMY,
            PedHash.BallaOrig01GMY,
            PedHash.Ballas01GFY,
            PedHash.Ballasog,
            PedHash.BallasogCutscene,
            PedHash.BallaSout01GMY,
        };

        public override void Start()
        {
            gangRelationship1 = World.AddRelationshipGroup("GANG_ONE");
            gangRelationship2 = World.AddRelationshipGroup("GANG_TWO");
            playerRelationship = World.AddRelationshipGroup("PLAYER");

            gangRelationship1.SetRelationshipBetweenGroups(gangRelationship2, Relationship.Hate, true);

            playerRelationship.SetRelationshipBetweenGroups(gangRelationship1, Relationship.Hate, true);
            playerRelationship.SetRelationshipBetweenGroups(gangRelationship2, Relationship.Hate, true);

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            switch(missionState)
            {
                case MissionState.Start:
                    // wait till player is in range
                    while (Game.PlayerPed.Position.Distance(gangArea) > 300f)
                    {
                        await BaseScript.Delay(100);
                    }
                    FxPed[] peds = World.GetAllPeds();

                    int odd = 0;

                    foreach(FxPed ped in peds)
                    {
                        if (ped.IsPlayer) continue;
                        if (!ped.Exists()) continue;
                        if (ped.IsDead) continue;

                        if (ped.Position.Distance(gangArea) < 200f)
                        {
                            if (odd % 2 == 0)
                            {
                                ped.RelationshipGroup = gangRelationship1;
                            }
                            else
                            {
                                ped.RelationshipGroup = gangRelationship2;
                            }

                            ped.Task.FightAgainstHatedTargets(200f);

                            if (ped.AttachedBlip is null)
                            {
                                Blip b = ped.AttachBlip();
                                b.Sprite = BlipSprite.Enemy;
                                b.Color = BlipColor.Red;
                                b.Scale = 0.5f;

                                RegisterBlip(b);
                            }

                            pedsToTrack.Add(ped);
                        }
                        odd++;
                    }
                    missionState = MissionState.StartScene;
                    break;
                case MissionState.StartScene:

                    if (Game.PlayerPed.Position.Distance(gangArea) > 250f) missionState = MissionState.LeftArea;

                    List<FxPed> copyPeds = new List<FxPed>(pedsToTrack);
                    foreach (FxPed ped in copyPeds)
                    {
                        if (!ped.Exists())
                        {
                            pedsToTrack.Remove(ped);
                            continue;
                        }

                        if (ped.IsDead)
                        {
                            pedsToTrack.Remove(ped);
                        }
                    }

                    if (pedsToTrack.Count == 0)
                        missionState = MissionState.End;
                    break;
                case MissionState.LeftArea:
                    missionState = MissionState.CleanUp;
                    Fail("You left the area");
                    break;
                case MissionState.End:
                    missionState = MissionState.CleanUp;
                    Pass();
                    break;
            }
        }
    }

    enum MissionState
    {
        Start,
        SpawnPeds,
        SpawnedPeds,
        End,
        StartScene,
        SceneStarted,
        CleanUp,
        LeftArea
    }
}
