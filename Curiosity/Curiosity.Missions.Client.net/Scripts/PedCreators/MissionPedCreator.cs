
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Utils;
using Curiosity.Missions.Client.Extensions;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.MissionPedTypes;
using Curiosity.Missions.Client.Static;

namespace Curiosity.Missions.Client.Scripts.PedCreators
{
    static class MissionPedCreator
    {
        public static MissionPed Ped(Ped ped, Alertness alertness = Alertness.Nuetral, Difficulty difficulty = Difficulty.BringItOn, float visionDistance = 50f)
        {
            return Ped(ped, Relationships.HostileRelationship, alertness, difficulty, visionDistance);
        }

        public static MissionPed Ped(Ped ped, RelationshipGroup relationshipGroup, Alertness alertness = Alertness.Nuetral, Difficulty difficulty = Difficulty.BringItOn, float visionDistance = 50f)
        {
            MissionPed missionPed;

            ped.CanPlayGestures = true;
            ped.SetCanPlayAmbientAnims(true);
            ped.SetCanEvasiveDive(true);
            ped.SetPathCanUseLadders(true);
            ped.SetPathCanClimb(true);
            ped.AlwaysDiesOnLowHealth = Utility.RANDOM.Next(2) == 1;
            ped.SetAlertness(alertness);
            ped.SetCombatAttributes(CombatAttributes.AlwaysFight, true);
            Function.Call((Hash)8116279360099375049L, new InputArgument[] { ped.Handle, 0, 0 });
            ped.SetConfigFlag(281, true);

            API.SetEntityAsMissionEntity(ped.Handle, false, false);

            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;

            Blip currentBlip = ped.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }
            ped.IsPersistent = true;
            ped.RelationshipGroup = relationshipGroup;

            switch (difficulty)
            {
                case Difficulty.HurtMePlenty:
                    missionPed = new MissionPedNormal(ped.Handle, visionDistance);
                    if (missionPed.Health == 100)
                        missionPed.Health = 300;
                    break;
                case Difficulty.BringItOn:
                    missionPed = new MissionPedNormal(ped.Handle, visionDistance);
                    break;
                default:
                    missionPed = new MissionPedNormal(ped.Handle, visionDistance);
                    break;
            }

            return missionPed;
        }
    }
}
