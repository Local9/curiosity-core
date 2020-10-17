using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.Extensions;
using Curiosity.Missions.Client.MissionPeds;
using Curiosity.Missions.Client.MissionPedTypes;
using System;

namespace Curiosity.Missions.Client.Scripts.PedCreators
{
    static class ZombieCreator
    {
        private static bool _runners = false;

        public static bool Runners
        {
            get
            {
                return _runners;
            }
            set
            {
                _runners = Convert.ToBoolean(value);
            }
        }

        public static ZombiePed InfectPed(Ped ped, int health, bool overrideAsFastZombie = false)
        {
            ZombiePed walker;

            ped.CanPlayGestures = false;
            ped.SetCanPlayAmbientAnims(false);
            ped.SetCanEvasiveDive(false);
            ped.SetPathCanUseLadders(false);
            ped.SetPathCanClimb(false);
            ped.DisablePainAudio(true);
            ped.ApplyDamagePack(0f, 1f, DamagePack.BigHitByVehicle);
            ped.ApplyDamagePack(0f, 1f, DamagePack.ExplosionMed);
            ped.AlwaysDiesOnLowHealth = false;
            ped.SetAlertness(Alertness.Nuetral);
            ped.SetCombatAttributes(CombatAttributes.AlwaysFight, true);
            Function.Call((Hash)8116279360099375049L, new InputArgument[] { ped.Handle, 0, 0 });
            ped.SetConfigFlag(281, true);
            ped.Task.WanderAround(ped.Position, ZombiePed.WanderRadius);
            ped.AlwaysKeepTask = true;
            ped.BlockPermanentEvents = true;

            Blip currentBlip = ped.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }
            ped.IsPersistent = true;
            ped.RelationshipGroup = Static.Relationships.InfectedRelationship;
            float single = 0.055f;
            if (ZombieCreator.IsNightFall())
            {
                single = 0.5f;
            }
            TimeSpan currentDayTime = World.CurrentDayTime;
            if ((currentDayTime.Hours >= 20 ? true : currentDayTime.Hours <= 3))
            {
                single = 0.4f;
            }

            if ((!(Client.Random.NextDouble() < (double)single | overrideAsFastZombie) ? true : !ZombieCreator.Runners))
            {
                int num = health;
                int num1 = num;
                ped.MaxHealth = num;
                ped.Health = num1;
                walker = new ZombieWalker(ped.Handle);
            }
            else
            {
                walker = new ZombieRunner(ped.Handle);
            }

            Decorators.Set(walker.Handle, Client.DECOR_PED_MISSION, true);

            return walker;
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
