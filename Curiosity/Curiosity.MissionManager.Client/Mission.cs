﻿using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client
{
    public abstract class Mission : BaseScript
    {
        internal static List<MissionType> missions = new List<MissionType>();
        internal static bool isOnMission = false;
        internal static Mission currentMission = null;
        internal static MissionType missionType;

        /// <summary>
        /// This is called when a mission is started by the user. Typically this would be used to set up the mission by spawning things like peds and vehicles
        /// </summary>
        public abstract void Start();
        /// <summary>
        /// This is called on every tick even from ScriptHookVDotNet
        /// </summary>
        [Tick]
        public abstract void OnMissionTick();
        /// <summary>
        /// This is called as soon as the mission is ended. Typically this would be used for cleanup such as removing peds and vehicles
        /// </summary>
        public abstract void End();

        /// <summary>
        /// Stops a mission
        /// </summary>
        /// <param name="reason">The reason the mission was stopped</param>
        public void Stop(EndState reason)
        {
            isOnMission = false;
            missionType = MissionType.NotSet;
            currentMission.End();
            currentMission = null;

            if (reason == EndState.Pass)
            {
                // trigger success
            }

            foreach(Blip blip in PluginManager.Blips)
            {
                blip.Delete();
            }

            PluginManager.Blips.Clear();
        }

        /// <summary>
        /// Fails the mission for the specified reason
        /// </summary>
        /// <param name="failReason">The reason the mission failed</param>
        public void Fail(string failReason)
        {
            
        }

        public void Pass()
        {

        }

    }
    /// <summary>
    /// The type of mission, this determines what kind of blip and mission passed screen the mission has
    /// </summary>
    public enum MissionType
    {
        NotSet,
        Mission,
        Stranger,
        Heist,
        HeistSetup
    }


    /// <summary>
    /// The reason a mission ended, this is exclusively used for the Stop function
    /// </summary>
    public enum EndState
    {
        Fail,
        Pass
    }
}
