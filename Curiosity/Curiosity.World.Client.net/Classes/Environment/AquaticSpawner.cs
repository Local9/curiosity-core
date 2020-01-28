using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.GameWorld.Client.net.Classes.Environment
{
    static class AquaticSpawner
    {
        static Client client = Client.GetInstance();

        static Random random = new Random();

        // TODO: Weighted spawn probabilities
        static List<PedHash> AquaticHashes = new List<PedHash>()
        {
            PedHash.TigerShark,
            PedHash.HammerShark,
            PedHash.KillerWhale
        };

        static int MinimumCount = 1;
        static float SpawnDepth = -3f;
        static float AggroDistanceOnSpawn = 200f;

        static string Relationship = "SHARKS";
        static RelationshipGroup RelationshipGroupSharks;
        static bool AlreadyRunning = false;

        static public void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action<dynamic>(OnPlayerSpawned));
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
        }

        static void OnClientResourceStart(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            PeriodicCheck();
        }

        static void OnPlayerSpawned(dynamic spawnData)
        {
            PeriodicCheck();
        }

        static private async void PeriodicCheck()
        {

            if (AlreadyRunning) return;
            AlreadyRunning = true;

            RelationshipGroupSharks = CitizenFX.Core.World.AddRelationshipGroup(Relationship);

            while (true)
            {
                try
                {
                    if (Game.PlayerPed.IsInWater && Game.PlayerPed.Position.Z < -20f)
                    {
                        if (random.Next(1000) == 1)
                        {
                            var AquaticLifeList = CitizenFX.Core.World.GetAllPeds().Select(p => p).Where(p => p.Exists() && AquaticHashes.Contains((PedHash)p.Model.Hash));
                            if (AquaticLifeList.Count() < MinimumCount)
                            {
                                Vector2 SpawnLocation = GetRandomPointAroundPlayer();

                                Model shark = AquaticHashes[new Random().Next(AquaticHashes.Count - 1)];
                                await shark.Request(10000);

                                Ped Ped = await CitizenFX.Core.World.CreatePed(shark, new Vector3(SpawnLocation.X, SpawnLocation.Y, SpawnDepth));

                                if (Ped != null)
                                {
                                    Ped.RelationshipGroup = RelationshipGroupSharks;
                                    Ped.RelationshipGroup.SetRelationshipBetweenGroups(Client.PlayerRelationshipGroup, CitizenFX.Core.Relationship.Hate, true);
                                    Ped.Task.WanderAround();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"AquaticSpawner -> Failed to load ped: {ex}");
                }
                await BaseScript.Delay(1000);
            }
        }

        static public Vector2 GetRandomPointAroundPlayer()
        {
            Random Random = new Random();
            float distance = (float)Random.NextDouble() * 50f;
            double angleInRadians = Random.Next(360) / (2 * Math.PI);

            float x = (float)(distance * Math.Cos(angleInRadians));
            float y = (float)(distance * Math.Sin(angleInRadians));
            return new Vector2(Game.PlayerPed.Position.X + x, Game.PlayerPed.Position.Y + y);
        }
    }
}
