using CitizenFX.Core;
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
        static int pedGroup = 1;
        static uint playerGroupHash = 0;

        static Array weaponValues = Enum.GetValues(typeof(WeaponHash));

        static List<int> groups = new List<int>();

        public static void Init()
        {
            API.AddRelationshipGroup("PLAYER", ref playerGroupHash);
            client.RegisterTickHandler(PedTick);

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

                uint suspectGroupHash = 0;
                API.AddRelationshipGroup("suspect", ref suspectGroupHash);
                API.SetRelationshipBetweenGroups(5, suspectGroupHash, playerGroupHash);
                API.SetRelationshipBetweenGroups(5, playerGroupHash, suspectGroupHash);

                while (!model.IsLoaded)
                    await BaseScript.Delay(0);

                if (!API.DoesGroupExist(pedGroup))
                {
                    API.CreateGroup(pedGroup);
                    API.SetGroupFormation(pedGroup, 2);
                }

                
                Vector3 spawnPosition = World.GetNextPositionOnSidewalk(new Vector3(0f, 0f, 0f));
                Ped ped = await World.CreatePed(model, spawnPosition, 180.0f);

                ped.Weapons.Give((WeaponHash)weaponValues.GetValue(random.Next(weaponValues.Length)), 1000, true, true);
                ped.Weapons.Give((WeaponHash)weaponValues.GetValue(random.Next(weaponValues.Length)), 1000, true, true);
                ped.Weapons.Give((WeaponHash)weaponValues.GetValue(random.Next(weaponValues.Length)), 1000, true, true);
                ped.Task.FightAgainstHatedTargets(20.0f);

                API.SetPedRelationshipGroupHash(ped.Handle, suspectGroupHash);
                API.SetPedCombatMovement(ped.Handle, 2);

                API.SetPedCombatAttributes(ped.Handle, 0, true);
                API.SetPedCombatAttributes(ped.Handle, 1, true);
                API.SetPedCombatAttributes(ped.Handle, 2, true);
                API.SetPedCombatAttributes(ped.Handle, 3, true);
                API.SetPedCombatAttributes(ped.Handle, 5, true);
                API.SetPedCombatAttributes(ped.Handle, 46, true);
                API.SetPedCombatAttributes(ped.Handle, 52, true);
                API.SetPedCombatAbility(ped.Handle, 100);

                ped.Armor = random.Next(100);

                await BaseScript.Delay(0);

                model.MarkAsNoLongerNeeded();
                
                Vector3 streetSpawnPosition = World.GetNextPositionOnStreet(ped.Position, true);
                
                Model vehModel = VehicleHash.Bodhi2;
                await vehModel.Request(10000);

                CitizenFX.Core.Vehicle vehicle = await World.CreateVehicle(vehModel, streetSpawnPosition, ped.Heading);

                vehModel.MarkAsNoLongerNeeded();

                Blip blip = ped.AttachBlip();
                blip.Sprite = BlipSprite.Enemy;
                blip.Color = BlipColor.Red;
                blip.IsFriendly = false;
                blip.IsShortRange = true;
                blip.Scale = 0.0f;
                blip.Name = "Looks angry... run?";

                while(!ped.IsInVehicle())
                {
                    ped.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                    await BaseScript.Delay(0);
                }

                peds.Add(ped);

                if (peds.Count == 0)
                {
                    API.SetPedAsGroupLeader(ped.Handle, pedGroup);
                }
                else
                {
                    API.SetPedAsGroupMember(ped.Handle, pedGroup);
                }
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
                List<Ped> pedsToRun = peds;
                for (var i = 0; i < pedsToRun.Count; i++)
                {
                    try
                    {
                        if (!pedsToRun[i].IsNearEntity(Game.PlayerPed, new Vector3(15.0f, 15.0f, 15.0f)))
                        {
                            pedsToRun[i].Task.DriveTo(pedsToRun[i].CurrentVehicle, Game.PlayerPed.Position, 10.0f, 40.0f, 1074528293);
                        }
                        else
                        {
                            pedsToRun[i].AttachedBlip.Scale = 1.0f;
                            pedsToRun[i].Task.FightAgainst(Game.PlayerPed);
                        }

                        if (pedsToRun[i].IsDead)
                        {
                            if (pedsToRun[i].CurrentVehicle.Exists())
                                pedsToRun[i].CurrentVehicle.Delete();

                            pedsToRun[i].Weapons.RemoveAll();
                            pedsToRun[i].AttachedBlip.Delete();
                            pedsToRun[i].Delete();
                            peds.Remove(peds[i]);
                        }
                    }
                    catch (Exception ex)
                    {
                        peds.Remove(peds[i]);
                    }
                }

                //if (pedsToRun.Count == 0 && vehicle != null)
                //{
                //    if (vehicle.Exists())
                //        vehicle.Delete();

                //    if (!vehicle.Exists())
                //        vehicle = null;
                //}

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PedTick] -> {ex.Message}");
            }
            await Task.FromResult(0);
        }
    }
}
