using CitizenFX.Core;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Curiosity.MissionManager.Client
{
    public class Func
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

            Mission.currentMission = mis;
            Mission.missionType = mission;
            Mission.AddPlayer(Game.Player);

            Mission.RegisteredPeds = new List<Classes.Ped>();
            Mission.RegisteredVehicles = new List<Classes.Vehicle>();

            Mission.isOnMission = true;
            Mission.currentMission.Start();
        }

        /// <summary>
        /// Returns a MissionInfo object based on the inputted mission's attributes
        /// </summary>
        /// <param name="mission">The mission to get info on</param>
        internal static MissionInfo GetMissionInfo(Type mission)
        {
            MissionInfo info = (MissionInfo)mission.GetCustomAttribute(typeof(MissionInfo));
            return info;
        }
    }
}
