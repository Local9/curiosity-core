﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Handler;
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
        public static int GameTimeOfLastMission;
        public static int GameTimeTillNextMission = 120000;

        static List<Type> currentMissionSelection = new List<Type>();

        public static void ToggleMissionDirector()
        {
            MissionDirectorState = !MissionDirectorState;
            string state = MissionDirectorState ? "~g~Enabled" : "~o~Disabled";
            Notify.Info($"~b~Dispatch A.I. {state}");

            BaseScript.TriggerEvent(LegacyEvents.Client.PoliceDutyEvent, true, MissionDirectorState, "Police");

            if (MissionDirectorState)
            {
                GameTimeOfLastMission = API.GetGameTimer() + 120000;

                Instance.RegisterTickHandler(OnMissionDirectorTick);
            }
            else
            {
                Instance.DeregisterTickHandler(OnMissionDirectorTick);
            }
        }

        public override void Begin()
        {
            
        }

        private static async Task OnMissionDirectorTick()
        {
            if (Mission.isOnMission)
            {
                await BaseScript.Delay(5000);
            }
            else
            {
                while ((API.GetGameTimer() - GameTimeOfLastMission) < GameTimeTillNextMission)
                {
                    await BaseScript.Delay(1500);
                }

                if (!LatestPatrolZone.Equals(PlayerHandler.PatrolZone))
                {
                    LatestPatrolZone = PlayerHandler.PatrolZone;

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

            int timerStarted = API.GetGameTimer();

            while((API.GetGameTimer() - timerStarted) < 10000 && !Mission.isOnMission)
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
                GameTimeOfLastMission = API.GetGameTimer();
                GameTimeTillNextMission = (1000 * 60) * Utility.RANDOM.Next(2, 6);
                // by not accepting a mission, a user will wait longer next time
                Notify.DispatchAI("Wasting Dispatch Time", "Not accepting my calls will mean you'll have to wait longer.");
            }
        }
    }
}
