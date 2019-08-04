using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Enums;

namespace Curiosity.Police.Client.net.Classes
{
    class CreatePed
    {
        static Random random = new Random();

        public static async Task<Ped> Create(Model suspectModel, Vector3 suspectPosition, float suspectHeading, RelationshipGroup relationshipGroup)
        {
            await Client.Delay(0);

            Ped createdPed = await World.CreatePed(suspectModel, suspectPosition, suspectHeading);
            API.SetNetworkIdCanMigrate(createdPed.NetworkId, true);

            await Client.Delay(0);

            Blip suspectBlip = createdPed.AttachBlip();
            suspectBlip.Sprite = BlipSprite.Enemy;
            suspectBlip.Color = BlipColor.Red;
            suspectBlip.Priority = 10;
            suspectBlip.IsShortRange = true;
            suspectBlip.Alpha = 0;

            await Client.Delay(0);

            createdPed.Armor = random.Next(100);

            if (random.Next(2) == 1)
            {
                createdPed.Weapons.Give(WeaponHash.Pistol, 30, true, true);
            }
            else
            {
                createdPed.Weapons.Give(WeaponHash.SawnOffShotgun, 30, true, true);
            }

            createdPed.Accuracy = random.Next(30, 100);
            createdPed.DropsWeaponsOnDeath = false;
            createdPed.AlwaysDiesOnLowHealth = random.Next(9) == 0;

            await Client.Delay(0);

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

            await Client.Delay(0);

            createdPed.RelationshipGroup = relationshipGroup;
            API.SetEntityOnlyDamagedByPlayer(createdPed.Handle, true);
            API.SetBlockingOfNonTemporaryEvents(createdPed.Handle, false);
            API.SetPedSphereDefensiveArea(createdPed.Handle, createdPed.Position.X, createdPed.Position.Y, createdPed.Position.Z, 80.0f, true, false);
            API.TaskCombatHatedTargetsAroundPedTimed(createdPed.Handle, 130.0f, -1, 0);
            API.N_0x2016c603d6b8987c(createdPed.Handle, false);

            await Client.Delay(0);

            return createdPed;
        }
    }
}
