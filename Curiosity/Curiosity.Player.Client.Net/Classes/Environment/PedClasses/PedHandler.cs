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

                Model vehModel = VehicleHash.Bodhi2;
                await vehModel.Request(10000);

                string group = "TREVOR";
                RelationshipGroup suspectGroup = World.AddRelationshipGroup(group);
                suspectGroup.SetRelationshipBetweenGroups(Client.PlayerRelationshipGroup, Relationship.Hate, true);

                while (!model.IsLoaded)
                    await BaseScript.Delay(0);

                Vector3 spawnPosition = new Vector3();
                API.GetNthClosestVehicleNode(0f, 0f, 0f, random.Next(10, 20), ref spawnPosition, 0, 0, 0);

                Vector3 streetSpawnPosition = World.GetNextPositionOnStreet(spawnPosition, true);
                Vehicle vehicle = await World.CreateVehicle(vehModel, streetSpawnPosition, 0.0f);

                vehModel.MarkAsNoLongerNeeded();

                Ped ped = await World.CreatePed(model, new Vector3(402.668f, -1003.000f, -98.004f), 180.0f);
                ped.IsPositionFrozen = true;
                await BaseScript.Delay(0);
                model.MarkAsNoLongerNeeded();

                while (!ped.IsInVehicle())
                {
                    ped.IsPositionFrozen = false;
                    await BaseScript.Delay(0);
                    ped.Task.WarpIntoVehicle(vehicle, VehicleSeat.Driver);
                    await BaseScript.Delay(0);
                }
                
                ped.Weapons.Give((WeaponHash)weaponValues.GetValue(random.Next(weaponValues.Length)), 1000, true, true);

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

                ped.Armor = random.Next(100);

                await BaseScript.Delay(0);

                Blip blip = ped.AttachBlip();
                blip.Sprite = BlipSprite.Enemy;
                blip.Color = BlipColor.Red;
                blip.IsFriendly = false;
                blip.IsShortRange = false;
                blip.Alpha = 0;
                blip.Name = "Looks angry... run?";
                API.SetBlipDisplay(blip.Handle, 5);

                await BaseScript.Delay(0);
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
                List<Ped> pedsToRun = new List<Ped>(peds);
                foreach(Ped ped in pedsToRun)
                {
                    await BaseScript.Delay(50);
                    if (ped.IsAlive)
                    {
                        if (!ped.IsNearEntity(Game.PlayerPed, new Vector3(15.0f, 15.0f, 15.0f)))
                        {
                            ped.Task.DriveTo(ped.CurrentVehicle, Game.PlayerPed.Position, 10.0f, 40.0f, 1074528293);
                        }
                        else
                        {
                            ped.Task.FightAgainstHatedTargets(40.0f);
                        }
                    }

                    await BaseScript.Delay(10);
                    if (ped.IsDead)
                    {
                        if (ped.CurrentVehicle.Exists())
                            ped.CurrentVehicle.Delete();

                        ped.Weapons.RemoveAll();
                        ped.AttachedBlip.Delete();
                        ped.Delete();
                        peds.Remove(ped);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PedTick] -> {ex.Message}");
            }
            await Task.FromResult(0);
        }
    }
}
