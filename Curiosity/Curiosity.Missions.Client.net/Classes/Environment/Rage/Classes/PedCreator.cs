using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

using Curiosity.Missions.Client.net.Wrappers;

namespace Curiosity.Missions.Client.net.Classes.Environment.Rage.Classes
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
            model.MarkAsNoLongerNeeded();
            // API.TaskSetBlockingOfNonTemporaryEvents(spawnedPed.Handle, blockTempEvents);
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
            PedList.Remove(entity as Ped);
            Entity killerEnt = new Ped(entity.Handle).GetKiller();
            Ped killerPed = new Ped(killerEnt.Handle);

            if (killerPed.IsPlayer) {
                CitizenFX.Core.UI.Screen.ShowNotification($"Civil Ped {entity.Handle}");
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
