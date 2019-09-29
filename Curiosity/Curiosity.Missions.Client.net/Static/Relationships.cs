using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.Static
{
    static class Relationships
    {
        public static RelationshipGroup PlayerRelationship; 
        public static RelationshipGroup HostileRelationship;
        public static RelationshipGroup InfectedRelationship;

        static public void Init()
        {
            // Player
            PlayerRelationship = World.AddRelationshipGroup("PLAYER_RELATIONSHIP");
            Game.PlayerPed.RelationshipGroup = PlayerRelationship;
            // Other Peds
            HostileRelationship = World.AddRelationshipGroup("HOSTILE_RELATIONSHIP");
            HostileRelationship.SetRelationshipBetweenGroups(PlayerRelationship, Relationship.Hate, true);
            // Zombies
            InfectedRelationship.SetRelationshipBetweenGroups(PlayerRelationship, Relationship.Hate, true);
            InfectedRelationship.SetRelationshipBetweenGroups(HostileRelationship, Relationship.Hate, true);
        }
    }
}
