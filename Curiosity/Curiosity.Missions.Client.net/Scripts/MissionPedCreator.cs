using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.MissionPedTypes;

namespace Curiosity.Missions.Client.net.Scripts
{
    static class MissionPedCreator
    {

        public static MissionPed Ped(Ped ped, Alertness alertness = Alertness.Nuetral, Difficulty difficulty = Difficulty.BringItOn, float visionDistance = 50f)
        {
            MissionPed missionPed;

            ped.CanPlayGestures = true;
            ped.SetCanPlayAmbientAnims(true);
            ped.SetCanEvasiveDive(true);
            ped.SetPathCanUseLadders(true);
            ped.SetPathCanClimb(true);
            ped.AlwaysDiesOnLowHealth = Client.Random.Next(2) == 1;
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
            ped.RelationshipGroup = Static.Relationships.HostileRelationship;

            switch (difficulty)
            {
                case Difficulty.BringItOn:
                    missionPed = new MissionPedNormal(ped.Handle, visionDistance);
                    break;
                default:
                    missionPed = new MissionPedNormal(ped.Handle, visionDistance);
                    break;
            }

            return missionPed;
        }

        public static bool IsNightFall()
        {
            bool flag;
            if (ZombieCreator.Runners)
            {
                TimeSpan currentDayTime = World.CurrentDayTime;
                flag = (currentDayTime.Hours >= 20 ? true : currentDayTime.Hours <= 3);
            }
            else
            {
                flag = false;
            }
            return flag;
        }
    }
}
