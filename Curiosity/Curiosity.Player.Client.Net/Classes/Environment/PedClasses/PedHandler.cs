﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.PedClasses
{
    class PedHandler
    {
        static Client client = Client.GetInstance();
        static List<Ped> peds = new List<Ped>();
        static Random random = new Random(Guid.NewGuid().GetHashCode());

        static bool IsFightingPlayer = false;
        static bool IsDrivingToPlayer = false;

        static Array weaponValues = Enum.GetValues(typeof(WeaponHash));

        static List<int> groups = new List<int>();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Command:SpawnChaser", new Action(CreateChaser));
            client.RegisterEventHandler("onClientResourceStop", new Action<string>(OnClientResourceStop));
        }

        static async void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            foreach(int groupId in groups)
            {
                if (API.DoesGroupExist(groupId))
                {
                    API.RemoveGroup(groupId);
                    await Client.Delay(0);
                }
            }
        }

        public static async void CreateChaser()
        {
            try
            {
                Model model = PedHash.Trevor;
                await model.Request(10000);

                Model vehModel = VehicleHash.Bodhi2;
                await vehModel.Request(10000);

                string group = "TREVOR";
                RelationshipGroup suspectGroup = World.AddRelationshipGroup(group);
                suspectGroup.SetRelationshipBetweenGroups(Client.PlayerRelationshipGroup, Relationship.Hate, true);

                while (!model.IsLoaded)
                    await BaseScript.Delay(0);

                Vector3 spawnPosition = new Vector3();
                API.GetNthClosestVehicleNode(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, random.Next(500, 1000), ref spawnPosition, 0, 0, 0);
                
                await BaseScript.Delay(1000);

                Vector3 streetSpawnPosition = World.GetNextPositionOnStreet(spawnPosition, true);
                Vehicle vehicle = await World.CreateVehicle(vehModel, streetSpawnPosition, 0.0f);

                Blip vehBlip = vehicle.AttachBlip();
                vehBlip.Alpha = 0;

                vehModel.MarkAsNoLongerNeeded();

                Ped ped = await World.CreatePed(model, new Vector3(spawnPosition.X, spawnPosition.Y, spawnPosition.Z), 180.0f);
                ped.IsPositionFrozen = true;
                API.SetPedHearingRange(ped.Handle, 1000.0f);
                API.SetPedSeeingRange(ped.Handle, 1000.0f);

                peds.Add(ped);

                await BaseScript.Delay(0);
                model.MarkAsNoLongerNeeded();

                await BaseScript.Delay(1000);

                while (!ped.IsInVehicle())
                {
                    ped.IsPositionFrozen = false;
                    API.TaskWarpPedIntoVehicle(ped.Handle, vehicle.Handle, (int)VehicleSeat.Driver);
                    await BaseScript.Delay(0);
                }

                ped.Weapons.Give((WeaponHash)weaponValues.GetValue(random.Next(weaponValues.Length)), 1000, false, true);
                ped.Weapons.Give(WeaponHash.APPistol, 1000, false, true);
                ped.Weapons.Give(WeaponHash.HeavyPistol, 1000, false, true);
                ped.Weapons.Give(WeaponHash.HeavyShotgun, 1000, false, true);
                ped.Weapons.Give((WeaponHash)weaponValues.GetValue(random.Next(weaponValues.Length)), 1000, false, true);
                ped.Weapons.Give((WeaponHash)weaponValues.GetValue(random.Next(weaponValues.Length)), 1000, false, true);

                await BaseScript.Delay(0);

                API.SetPedVisualFieldCenterAngle(ped.Handle, 180f);
                API.SetPedVisualFieldMaxAngle(ped.Handle, 180f);
                API.SetPedVisualFieldMinAngle(ped.Handle, 75f);
                API.SetPedVisualFieldMaxElevationAngle(ped.Handle, 90f);
                API.SetPedVisualFieldMinElevationAngle(ped.Handle, 0f);
                API.SetPedVisualFieldPeripheralRange(ped.Handle, 45f);

                await BaseScript.Delay(0);
                ped.RelationshipGroup = suspectGroup;
                API.SetPedCombatMovement(ped.Handle, 2);

                API.SetPedCombatAttributes(ped.Handle, 0, true);
                API.SetPedCombatAttributes(ped.Handle, 1, true);
                API.SetPedCombatAttributes(ped.Handle, 2, true);
                API.SetPedCombatAttributes(ped.Handle, 3, true);
                API.SetPedCombatAttributes(ped.Handle, 5, true);
                API.SetPedCombatAttributes(ped.Handle, 46, true);
                API.SetPedCombatAttributes(ped.Handle, 52, true);
                API.SetPedCombatAbility(ped.Handle, 100);

                await BaseScript.Delay(0);

                API.SetPedSteersAroundObjects(ped.Handle, true);
                API.SetPedSteersAroundPeds(ped.Handle, true);
                API.SetPedSteersAroundVehicles(ped.Handle, true);
                API.SetDriverAbility(ped.Handle, 1.0f);
                API.SetDriverAggressiveness(ped.Handle, 1.0f);
                API.SetPedFleeAttributes(ped.Handle, 0, false);
                API.TaskSetBlockingOfNonTemporaryEvents(ped.Handle, true);

                ped.Armor = random.Next(100);

                await BaseScript.Delay(0);

                ped.IsOnlyDamagedByPlayer = true;
                ped.Task.FightAgainstHatedTargets(100.0f);

                Blip blip = ped.AttachBlip();
                blip.Alpha = 0;

                await BaseScript.Delay(0);
                client.RegisterTickHandler(PedTick);

                Classes.Environment.UI.Notifications.Advanced("CHAR_TREVOR", 2, "AARRRGGGHHHH!!!", "You idiot!", "I was busy and now you send me a random message, you'll pay for this!", 31);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CreateChaser] -> {ex}");
            }
        }

        static async Task PedTick()
        {
            try
            {
                if (peds.Count == 0)
                {
                    await BaseScript.Delay(10);
                    return;
                }

                List<Ped> pedsToRun = new List<Ped>(peds);
                foreach(Ped ped in pedsToRun)
                {
                    await BaseScript.Delay(50);

                    if (ped.Exists())
                    {
                        if (ped.IsAlive)
                        {
                            if (ped.Position.DistanceToSquared(Game.PlayerPed.Position) > 100f && ped.IsInVehicle())
                            {
                                await Client.Delay(10000);
                                ped.Task.EnterAnyVehicle(VehicleSeat.Driver, -1, 2f);
                                ped.Task.DriveTo(ped.CurrentVehicle, Game.PlayerPed.Position, 10.0f, 30.0f, (int)DrivingStyle.ShortestPath);
                            }
                            
                            if (ped.Position.DistanceToSquared(Game.PlayerPed.Position) < 99.9f)
                            {
                                ped.Task.LeaveVehicle();
                                await BaseScript.Delay(1000);
                                ped.Task.FightAgainstHatedTargets(100.0f);
                                await Client.Delay(10000);
                            }
                            
                        }

                        if (ped.IsDead)
                        {
                            if (ped.CurrentVehicle.Exists())
                                ped.CurrentVehicle.Delete();

                            ped.Weapons.RemoveAll();
                            ped.AttachedBlip.Delete();
                            ped.Delete();
                            peds.Remove(ped);
                            client.DeregisterTickHandler(PedTick);

                            IsDrivingToPlayer = false;
                            IsFightingPlayer = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PedTick] -> {ex.Message}");
            }
            await Client.Delay(50);
            await Task.FromResult(0);
        }
    }
}
