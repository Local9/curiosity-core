using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace Curiosity.Police.Client.net.Classes
{
    class CreatePed
    {
        static Random random = new Random();

        public static async Task<Ped> Create(Model suspectModel, Vector3 suspectPosition, float suspectHeading, RelationshipGroup relationshipGroup)
        {
            await Client.Delay(0);

            int collisionId = API.RequestCollisionAtCoord(suspectPosition.X, suspectPosition.Y, suspectPosition.Z);

            if (Client.IsHalloween)
            {
                suspectModel.MarkAsNoLongerNeeded();
                suspectModel = PedHash.Zombie01;
            }


            Ped createdPed = await World.CreatePed(suspectModel, suspectPosition, suspectHeading);
            API.SetNetworkIdCanMigrate(createdPed.NetworkId, true);

            createdPed.NeverLeavesGroup = true;

            API.SetEntityAsMissionEntity(createdPed.Handle, true, true);

            await Client.Delay(0);

            Blip suspectBlip = createdPed.AttachBlip();
            suspectBlip.Sprite = BlipSprite.Enemy;
            suspectBlip.Color = BlipColor.Red;
            suspectBlip.Priority = 10;
            suspectBlip.IsShortRange = true;
            suspectBlip.Alpha = 0;

            await Client.Delay(0);

            if (!Client.IsHalloween)
            {
                createdPed.Armor = random.Next(100);

                if (random.Next(2) == 1)
                {
                    createdPed.Weapons.Give(WeaponHash.Pistol, 30, false, true);
                    createdPed.Weapons.Give(WeaponHash.SawnOffShotgun, 30, false, true);
                }
                else
                {
                    createdPed.Weapons.Give(WeaponHash.Pistol, 30, false, true);
                    createdPed.Weapons.Give(WeaponHash.MiniSMG, 30, false, true);
                }
            }
            else
            {
                createdPed.Health = 750;
                createdPed.MaxHealth = 750;
                createdPed.Armor = 100;
            }

            await Client.Delay(0);

            createdPed.Accuracy = random.Next(30, 100);
            createdPed.DropsWeaponsOnDeath = false;

            if (!Client.IsHalloween)
            {
                createdPed.AlwaysDiesOnLowHealth = random.Next(9) == 0;
            }

            API.SetPedHearingRange(createdPed.Handle, 35.0f);
            API.SetPedSeeingRange(createdPed.Handle, 100.0f);

            await Client.Delay(0);

            API.SetPedVisualFieldCenterAngle(createdPed.Handle, 180f);
            API.SetPedVisualFieldMaxAngle(createdPed.Handle, 275f);
            API.SetPedVisualFieldMinAngle(createdPed.Handle, 25f);
            API.SetPedVisualFieldMaxElevationAngle(createdPed.Handle, 90f);
            API.SetPedVisualFieldMinElevationAngle(createdPed.Handle, 0f);
            API.SetPedVisualFieldPeripheralRange(createdPed.Handle, 180f);

            await Client.Delay(0);

            createdPed.IsOnlyDamagedByPlayer = true;

            API.SetEntityAsMissionEntity(createdPed.Handle, true, true);

            if (!Client.IsHalloween)
            {
                API.SetPedCombatAttributes(createdPed.Handle, 1, true); // can use vehicles
                API.SetPedCombatMovement(createdPed.Handle, 2);
                API.SetPedCombatAttributes(createdPed.Handle, 26, true);
                API.SetPedCombatAttributes(createdPed.Handle, 3, false);
                API.SetPedCombatAttributes(createdPed.Handle, 2, true);
                API.SetPedCombatAttributes(createdPed.Handle, 16, true);
            }

            API.SetPedCombatAttributes(createdPed.Handle, 5, true);
            API.SetPedCombatAttributes(createdPed.Handle, 46, true);

            await Client.Delay(0);

            if (createdPed.IsInVehicle())
            {
                API.SetPedSteersAroundObjects(createdPed.Handle, true);
                API.SetPedSteersAroundPeds(createdPed.Handle, true);
                API.SetPedSteersAroundVehicles(createdPed.Handle, true);
                API.SetDriverAbility(createdPed.Handle, 1.0f);
                API.SetDriverAggressiveness(createdPed.Handle, 1.0f);
                await Client.Delay(0);
            }

            API.TaskSetBlockingOfNonTemporaryEvents(createdPed.Handle, true);
            API.SetPedFleeAttributes(createdPed.Handle, 0, false);

            await Client.Delay(0);

            if (Client.IsHalloween)
            {
                if (!API.HasAnimSetLoaded("move_m@drunk@verydrunk"))
                {
                    API.RequestAnimSet("move_m@drunk@verydrunk");
                }
                API.SetPedMovementClipset(createdPed.Handle, "move_m@drunk@verydrunk", 0x3E800000);
            }
            else
            {
                if (random.Next(0, 9) == 0)
                {
                    createdPed.Accuracy = random.Next(30);
                    API.SetPedIsDrunk(createdPed.Handle, true);
                    if (!API.HasAnimSetLoaded("move_m@drunk@verydrunk"))
                    {
                        API.RequestAnimSet("move_m@drunk@verydrunk");
                    }
                    API.SetPedMovementClipset(createdPed.Handle, "move_m@drunk@verydrunk", 0x3E800000);
                }
            }

            await Client.Delay(0);

            createdPed.RelationshipGroup = relationshipGroup;
            API.SetEntityOnlyDamagedByPlayer(createdPed.Handle, true);
            API.SetBlockingOfNonTemporaryEvents(createdPed.Handle, false);
            API.SetPedSphereDefensiveArea(createdPed.Handle, createdPed.Position.X, createdPed.Position.Y, createdPed.Position.Z, 100.0f, true, false);
            API.TaskCombatHatedTargetsAroundPedTimed(createdPed.Handle, 150.0f, -1, 0);
            API.N_0x2016c603d6b8987c(createdPed.Handle, false);
            await Client.Delay(0);

            createdPed.Task.WanderAround(suspectPosition, 5f);

            return createdPed;
        }
    }
}
