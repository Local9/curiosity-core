using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Attributes
{
    /// <summary>
    /// All of the necessary info to set up a mission
    /// </summary>
    public class MissionInfo : Attribute
    {
        public string displayName, id, preRequisites;
        public bool canBeReplayed;
        public float xPos, yPos, zPos;
        public Vector3 startPoint;
        public MissionType type;

        /// <summary>
        /// The basic information about a mission
        /// </summary>
        /// <param name="displayName">The name seen in-game</param>
        /// <param name="id">The unique ID of the mission. Typically this will be formatted something like this: [Mission Pack Name][Mission Name]. For example: exampleMissionsCarRobbery</param>
        /// <param name="xPos">The X position of the starting point</param>
        /// <param name="yPos">The Y position of the starting point</param>
        /// <param name="zPos">The Z position of the starting point</param>
        /// <param name="type">The type of mission, this determines things like the blip and end screen</param>
        /// <param name="canBeReplayed">Whether or not the mission is replayable</param>
        /// <param name="preRequisites">The mission(s) that must be completed before this mission can be triggered. This MUST be formatted as follows: "[Mission1ID] [Mission2ID]" and so on.</param>
        public MissionInfo(string displayName, string id, float xPos, float yPos, float zPos, MissionType type, bool canBeReplayed = false, string preRequisites = "None")
        {
            // Might be worth working on roles availability?

            this.displayName = displayName;
            this.id = id;
            this.preRequisites = preRequisites;
            this.canBeReplayed = canBeReplayed;
            this.startPoint = new Vector3();
            this.xPos = xPos;
            this.yPos = yPos;
            this.zPos = zPos;
            this.type = type;
        }
    }
}
