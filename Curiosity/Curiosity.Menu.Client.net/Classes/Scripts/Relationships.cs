using CitizenFX.Core;
using System;

namespace Curiosity.Menus.Client.net.Static
{
    static class Relationships
    {
        static Client client = Client.GetInstance();

        public static RelationshipGroup PlayerRelationship;
        public static RelationshipGroup HostileRelationship;
        public static RelationshipGroup DislikeRelationship;
        public static RelationshipGroup NeutralRelationship;

        public static RelationshipGroup ArrestedRelationship;
        public static RelationshipGroup InfectedRelationship;

        static public RelationshipGroup Fighter1Relationship;
        static public RelationshipGroup Fighter2Relationship;

        static public RelationshipGroup BallasRelationship;
        static public RelationshipGroup FamiliesRelationship;

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

        public static void SetupRelationShips()
        {
            // Player
            PlayerRelationship = World.AddRelationshipGroup("PLAYER");
            Game.PlayerPed.RelationshipGroup = PlayerRelationship;
            // Other Peds
            // Other Peds
            HostileRelationship = World.AddRelationshipGroup("HOSTILE_RELATIONSHIP");
            SetRelationshipBothWays(Relationship.Hate, HostileRelationship, PlayerRelationship);
            DislikeRelationship = World.AddRelationshipGroup("DISLIKE_RELATIONSHIP");
            SetRelationshipBothWays(Relationship.Dislike, DislikeRelationship, PlayerRelationship);
            NeutralRelationship = World.AddRelationshipGroup("NEUTRAL_RELATIONSHIP");
            SetRelationshipBothWays(Relationship.Neutral, NeutralRelationship, PlayerRelationship);
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

            // GANGS
            BallasRelationship = World.AddRelationshipGroup("AMBIENT_GANG_BALLAS");
            FamiliesRelationship = World.AddRelationshipGroup("AMBIENT_GANG_FAMILY");
            SetRelationshipBothWays(Relationship.Hate, BallasRelationship, FamiliesRelationship);
            SetRelationshipBothWays(Relationship.Dislike, PlayerRelationship, BallasRelationship);
            SetRelationshipBothWays(Relationship.Dislike, PlayerRelationship, FamiliesRelationship);

        }
    }
}
