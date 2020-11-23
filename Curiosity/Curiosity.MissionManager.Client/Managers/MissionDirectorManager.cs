using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.EventWrapperLegacy;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Managers
{
    public class MissionDirectorManager : Manager<MissionDirectorManager>
    {
        public static bool MissionDirectorState = false;
        static PluginManager Instance => PluginManager.Instance;
        static PatrolZone LatestPatrolZone;
        public static DateTime GameTimeTillNextMission;

        static List<Type> currentMissionSelection = new List<Type>();

        public static void ToggleMissionDirector()
        {
            MissionDirectorState = !MissionDirectorState;
            string state = MissionDirectorState ? "~g~Enabled" : "~o~Disabled";
            Notify.Info($"~b~Dispatch A.I. {state}");

            if (MissionDirectorState)
            {
                GameTimeTillNextMission = DateTime.Now.AddMinutes(Utility.RANDOM.Next(2, 4));

                Instance.RegisterTickHandler(OnMissionDirectorTick);
            }
            else
            {
                Instance.DeregisterTickHandler(OnMissionDirectorTick);
            }
        }

        public override void Begin()
        {
            Logger.Info($"- [MissionDirectorManager] Begin -----------------");
        }

        private static async Task OnMissionDirectorTick()
        {
            if (Mission.isOnMission)
            {
                await BaseScript.Delay(5000);
            }
            else
            {
                Logger.Debug($"OnMissionDirectorTick Init");

                while (DateTime.Now.TimeOfDay.TotalMilliseconds < GameTimeTillNextMission.TimeOfDay.TotalMilliseconds)
                {
                    if (!MissionDirectorState) return;

                    Screen.ShowSubtitle($"TimeSpan: {DateTime.Now.TimeOfDay.TotalMilliseconds}~n~Req: {GameTimeTillNextMission.TimeOfDay.TotalMilliseconds}");
                    await BaseScript.Delay(1500);
                }

                if (!MissionDirectorState) return;

                if (!LatestPatrolZone.Equals(JobManager.PatrolZone))
                {
                    Logger.Debug($"Patrol Zone changed since last check");

                    LatestPatrolZone = JobManager.PatrolZone == PatrolZone.Anywhere ? PatrolZone.City : JobManager.PatrolZone;

                    List<Type> missions = Mission.missions; // make a copy of the list for this instance
                    currentMissionSelection.Clear();

                    // loop over the list and narrow the missions down
                    // no point having county missions while in city
                    missions.ForEach(mis =>
                    {
                        MissionInfo missionInfo = Functions.GetMissionInfo(mis);
                        double chance = Utility.RANDOM.NextDouble();

                        if (missionInfo.type.Equals(MissionType.Developer))
                            return;

                        if (missionInfo.patrolZone.Equals(LatestPatrolZone))
                        {
                            currentMissionSelection.Add(mis);
                        }
                        else if (missionInfo.patrolZone.Equals(PatrolZone.Anywhere))
                        {
                            currentMissionSelection.Add(mis);
                        }

                    });

                    await SelectRandomMission(currentMissionSelection);
                }
                else
                {
                    await SelectRandomMission(currentMissionSelection);
                }

                await BaseScript.Delay(5000);
            }
        }

        private static async Task SelectRandomMission(List<Type> missions)
        {
            if (missions == null)
                return;

            Logger.Debug($"{missions.Count} Random Missions");

            double randomSpawn = Utility.RANDOM.NextDouble();

            List<Type> missionsByChance = new List<Type>();

            missions.ForEach(m =>
            {
                MissionInfo missionInfo = Functions.GetMissionInfo(m);

                if (missionInfo.chanceOfSpawn > randomSpawn)
                    missionsByChance.Add(m);
            });

            // try again if nothing was added, this should be fucking RARE because there should
            // always be missions with a chance at 100%, this is for the rare chance of something
            // different to spawn, like who knows, a fucking tank or something!
            if (missionsByChance.Count == 0)
            {
                SelectRandomMission(missions);
                return;
            }

            Type mission = missionsByChance[Utility.RANDOM.Next(missionsByChance.Count)];

            int notificationId = Notify.CustomControl("~b~~h~Dispatch A.I.~h~~s~: Press to accept call.", true);

            DateTime timerStarted = DateTime.Now;

            while (DateTime.Now.Subtract(timerStarted).TotalSeconds < 10 && !Mission.isOnMission)
            {
                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    Functions.StartMission(mission);
                }
                await BaseScript.Delay(0);
            }

            API.ThefeedRemoveItem(notificationId); // remove the notification
            missionsByChance = null; // clear it, don't need it in memory

            // if they are not on a mission because they didn't accept it, reset for a new mission
            if (!Mission.isOnMission)
            {
                await BaseScript.Delay(100);
                GameTimeTillNextMission = DateTime.Now.AddMinutes(Utility.RANDOM.Next(4, 6));
                // by not accepting a mission, a user will wait longer next time
                Notify.DispatchAI("Wasting Dispatch Time", "Not accepting my calls will mean you'll have to wait longer.");
            }
        }
    }
}
