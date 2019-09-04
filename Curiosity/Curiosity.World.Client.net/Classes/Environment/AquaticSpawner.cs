using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.World.Client.net.Classes.Environment
{
    static class AquaticSpawner
    {
        static Client client = Client.GetInstance();

        // TODO: Weighted spawn probabilities
        static List<PedHash> AquaticHashes = new List<PedHash>()
        {
            PedHash.TigerShark,
            PedHash.HammerShark,
            PedHash.KillerWhale
        };
        static Vector3 Center = new Vector3(-2900, -1970, 0);
        static float Radius = 100f;
        static int MinimumCount = 50;
        static float SpawnDepth = -3f;
        static float AggroDistanceOnSpawn = 200f;

        static string Relationship = "SHARKS";
        static RelationshipGroup RelationshipGroupSharks;

        static public void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
        }

        static void OnPlayerSpawned(dynamic spawnData)
        {
            PeriodicCheck();
            RelationshipGroupSharks = CitizenFX.Core.World.AddRelationshipGroup(Relationship);
        }

        static private async void PeriodicCheck()
        {
            while (true)
            {
                if (Game.PlayerPed.Position.DistanceToSquared(Center) < Math.Pow(Radius, 2))
                {
                    var AquaticLifeList = CitizenFX.Core.World.GetAllPeds().Select(p => p).Where(p => p.Exists() && AquaticHashes.Contains((PedHash)p.Model.Hash) && p.Position.DistanceToSquared(Center) < Math.Pow(Radius, 2));
                    if (AquaticLifeList.Count() < MinimumCount)
                    {
                        Vector2 SpawnLocation = GetRandomPointAroundPlayer();
                        Ped Ped = await CitizenFX.Core.World.CreatePed(AquaticHashes[new Random().Next(0, AquaticHashes.Count)], new Vector3(SpawnLocation.X, SpawnLocation.Y, SpawnDepth));

                        Ped.RelationshipGroup = RelationshipGroupSharks;
                        Ped.RelationshipGroup.SetRelationshipBetweenGroups(Client.PlayerRelationshipGroup, CitizenFX.Core.Relationship.Hate, true);

                        if (Ped.Position.DistanceToSquared(Game.PlayerPed.Position) < AggroDistanceOnSpawn)
                        {
                            Ped.Task.FightAgainstHatedTargets(AggroDistanceOnSpawn);
                        }
                    }
                }
                await BaseScript.Delay(1000);
            }
        }

        static public Vector2 GetRandomPointAroundPlayer()
        {
            Random Random = new Random();
            float distance = (float)Random.NextDouble() * 10f;
            double angleInRadians = Random.Next(360) / (2 * Math.PI);

            float x = (float)(distance * Math.Cos(angleInRadians));
            float y = (float)(distance * Math.Sin(angleInRadians));
            return new Vector2(Game.PlayerPed.Position.X + x, Game.PlayerPed.Position.Y + y);
        }
    }
}
