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
        private static List<Ped> _peds = new List<Ped>();

        static public void Init()
        {
            // only here to init the class
        }

        static public async Task<Ped> CreatePedAtLocation(Model model, Vector3 location, float heading, bool blockTempEvents = true)
        {
            await model.Request(10000);
            Ped spawnedPed = await World.CreatePed(model, location, heading);
            model.MarkAsNoLongerNeeded();
            // API.TaskSetBlockingOfNonTemporaryEvents(spawnedPed.Handle, blockTempEvents);
            API.SetPedFleeAttributes(spawnedPed.Handle, 0, false);

            _peds.Add(spawnedPed);

            EntityEventWrapper entityEventWrapper = new EntityEventWrapper(spawnedPed);
            entityEventWrapper.Died += new EntityEventWrapper.OnDeathEvent(EventWrapperOnDied);
            entityEventWrapper.Disposed += new EntityEventWrapper.OnWrapperDisposedEvent(EventWrapperOnDisposed);

            return spawnedPed;
        }

        static private void EventWrapperOnDisposed(EntityEventWrapper sender, Entity entity)
        {
            if (_peds.Contains(entity as Ped))
            {
                _peds.Remove(entity as Ped);
            }
        }

        static private void EventWrapperOnDied(EntityEventWrapper sender, Entity entity)
        {
            _peds.Remove(entity as Ped);
            Entity killerEnt = new Ped(entity.Handle).GetKiller();
            Ped killerPed = new Ped(killerEnt.Handle);

            if (killerPed.IsPlayer) {
                CitizenFX.Core.UI.Screen.ShowNotification($"Ped {entity.Handle}");
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
