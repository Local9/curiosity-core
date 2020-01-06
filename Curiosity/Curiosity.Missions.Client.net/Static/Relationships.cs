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
        static Client client = Client.GetInstance();

        public static RelationshipGroup PlayerRelationship; 
        public static RelationshipGroup HostileRelationship;
        public static RelationshipGroup ArrestedRelationship;
        public static RelationshipGroup InfectedRelationship;

        static public RelationshipGroup Fighter1Relationship;
        static public RelationshipGroup Fighter2Relationship;

        public static void SetRelationshipBothWays(Relationship rel, RelationshipGroup group1, RelationshipGroup group2)
        {
            group1.SetRelationshipBetweenGroups(group2, rel);
            group2.SetRelationshipBetweenGroups(group1, rel);
        }

        static public void Init()
        {
            SetupRelationShips();

            client.RegisterEventHandler("playerSpawned", new Action(SetupRelationShips));
        }

        static public void SetupRelationShips()
        {
            // Player
            PlayerRelationship = World.AddRelationshipGroup("PLAYER_RELATIONSHIP");
            Game.PlayerPed.RelationshipGroup = PlayerRelationship;
            // Other Peds
            HostileRelationship = World.AddRelationshipGroup("HOSTILE_RELATIONSHIP");
            SetRelationshipBothWays(Relationship.Hate, HostileRelationship, PlayerRelationship);
            // Zombies
            InfectedRelationship = World.AddRelationshipGroup("INFECTED_RELATIONSHIP");
            SetRelationshipBothWays(Relationship.Hate, InfectedRelationship, HostileRelationship);
            SetRelationshipBothWays(Relationship.Hate, InfectedRelationship, PlayerRelationship);
            // Arrested
            ArrestedRelationship = World.AddRelationshipGroup("ARRESTED_RELATIONSHIP");
            SetRelationshipBothWays(Relationship.Like, ArrestedRelationship, PlayerRelationship);

            // Fighters
            Fighter1Relationship = World.AddRelationshipGroup("FIGHTER_1");
            Fighter2Relationship = World.AddRelationshipGroup("FIGHTER_2");
            SetRelationshipBothWays(Relationship.Hate, Fighter1Relationship, Fighter2Relationship);
            SetRelationshipBothWays(Relationship.Hate, PlayerRelationship, Fighter1Relationship);
            SetRelationshipBothWays(Relationship.Hate, PlayerRelationship, Fighter2Relationship);

        }
    }
}
