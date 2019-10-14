using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.net.Scripts
{
    class PedCreator
    {
        public static List<Ped> PedList = new List<Ped>();

        static public void Init()
        {
            // only here to init the class
        }

        static public async Task<Ped> CreatePedAtLocation(Model model, Vector3 location, float heading, bool dropsWeaponsOnDeath = false)
        {
            await model.Request(10000);
            API.ClearAreaOfEverything(location.X, location.Y, location.Z, 2f, true, true, true, true);
            Ped spawnedPed = await World.CreatePed(model, location, heading);
            API.NetworkFadeInEntity(spawnedPed.Handle, false);
            model.MarkAsNoLongerNeeded();

            API.SetPedFleeAttributes(spawnedPed.Handle, 0, false);
            spawnedPed.DropsWeaponsOnDeath = dropsWeaponsOnDeath;

            PedList.Add(spawnedPed);

            EntityEventWrapper entityEventWrapper = new EntityEventWrapper(spawnedPed);
            entityEventWrapper.Died += new EntityEventWrapper.OnDeathEvent(EventWrapperOnDied);
            entityEventWrapper.Disposed += new EntityEventWrapper.OnWrapperDisposedEvent(EventWrapperOnDisposed);

            return spawnedPed;
        }

        static private void EventWrapperOnDisposed(EntityEventWrapper sender, Entity entity)
        {
            if (PedList.Contains(entity as Ped))
            {
                PedList.Remove(entity as Ped);
            }
        }

        static private void EventWrapperOnDied(EntityEventWrapper sender, Entity entity)
        {
            Ped ped = entity as Ped;
            PedList.Remove(ped);
            Ped killerEnt = ped.GetKiller() as Ped;

            if (killerEnt.IsPlayer) {
                if (ped.GetRelationshipWithPed(killerEnt) != Relationship.Hate)
                {
                    // KILLED A CLIVILIAN
                    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Civilian Killed", $"", string.Empty, 2);
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
