using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Managers;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Curiosity.MissionManager.Client
{
    public class Functions
    {
        /// <summary>
        /// Registers a mission so it can be seen and used in-game
        /// </summary>
        /// <param name="mission">The mission to register</param>
        public static void RegisterMission(Type mission)
        {
            if (mission.GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(MissionInfo)) == null)
                throw new Exception("Mission must have the MissionInfo attribute!");

            Log.Info($"[MissionManager] Registered: {mission.Name}");

            MissionInfo missionInfo = GetMissionInfo(mission);
            if (missionInfo.startPoint.X > 0f && missionInfo.startPoint.Y > 0f)
            {
                // world blips for set mission locations
            }

            Mission.missions.Add(mission);
        }

        /// <summary>
        /// Starts a mission
        /// </summary>
        /// <param name="mission">The mission to start</param>
        public static async void StartMission(Type mission)
        {
            // Remove any blips if they are left around
            foreach (Blip blip in PluginManager.Blips)
            {
                blip.Delete();
            }

            var constructor = mission.GetConstructor(Type.EmptyTypes);
            var typeClass = constructor.Invoke(new object[] { });
            var mis = (Mission)typeClass;

            MissionInfo missionInfo = GetMissionInfo(mission);

            Logger.Debug($"StartMission : {missionInfo.displayName}");

            Mission.currentMission = mis;
            Mission.missionType = mission;
            Mission.AddPlayer(Game.Player);
            Mission.ResetCountOfArrested();
            
            Mission.PatrolZone = JobManager.PatrolZone; // Always get the current zone from the player at this point
            // Mission randomiser will also use the PatrolZone of the player to select a mission, but the mission needs to know the players state IF the mission doesn't have a PatrolZone assigned

            Mission.RegisteredPeds = new List<Classes.Ped>();
            Mission.RegisteredVehicles = new List<Classes.Vehicle>();
            Mission.RegisteredParticles = new List<ParticleEffect>();

            API.RequestAnimDict("mp_arresting");
            API.RequestAnimDict("random@arrests@busted");
            API.RequestAnimDict("random@arrests");

            Mission.isOnMission = true;
            Mission.currentMission.Start();
        }

        /// <summary>
        /// Returns a MissionInfo object based on the inputted mission's attributes
        /// </summary>
        /// <param name="mission">The mission to get info on</param>
        internal static MissionInfo GetMissionInfo(Type mission)
        {
            if (mission == null) return null;

            MissionInfo info = (MissionInfo)mission.GetCustomAttribute(typeof(MissionInfo));
            return info;
        }
    }
}
