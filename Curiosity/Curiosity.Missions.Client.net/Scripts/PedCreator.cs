﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Classes.Environment;

namespace Curiosity.Missions.Client.net.Scripts
{
    class PedCreator
    {
        static public async Task<Ped> CreatePedAtLocation(Model model, Vector3 location, float heading, bool dropsWeaponsOnDeath = false)
        {
            await model.Request(10000);
            API.ClearAreaOfEverything(location.X, location.Y, location.Z - 1f, 2f, true, true, true, true);
            Ped spawnedPed = await World.CreatePed(model, location, heading);
            API.NetworkFadeInEntity(spawnedPed.Handle, false);
            model.MarkAsNoLongerNeeded();

            API.SetPedFleeAttributes(spawnedPed.Handle, 0, false);
            spawnedPed.DropsWeaponsOnDeath = dropsWeaponsOnDeath;

            EntityEventWrapper entityEventWrapper = new EntityEventWrapper(spawnedPed);
            entityEventWrapper.Died += new EntityEventWrapper.OnDeathEvent(EventWrapperOnDied);
            entityEventWrapper.Disposed += new EntityEventWrapper.OnWrapperDisposedEvent(EventWrapperOnDisposed);

            return spawnedPed;
        }

        static private void EventWrapperOnDisposed(EntityEventWrapper sender, Entity entity)
        {

        }

        static private void EventWrapperOnDied(EntityEventWrapper sender, Entity entity)
        {
            Ped ped = entity as Ped;
            Ped killerPed = ped.GetKiller() as Ped;

            if (killerPed.IsPlayer) {
                if (ped.GetRelationshipWithPed(killerPed) != Relationship.Hate)
                {
                    Player p = new Player(API.NetworkGetPlayerIndexFromPed(killerPed.Handle));
                    Client.TriggerServerEvent("curiosity:Server:Mission:killedCivilian", p.ServerId);
                }
            }

            Blip currentBlip = entity.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }
            entity.MarkAsNoLongerNeeded();
            sender.Dispose();
        }
    }
}
