using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Missions.Client.net.Classes.Environment.Rage.Classes
{
    class PedCreator
    {
        static public async Task<Ped> CreatePedAtLocation(Model model, Vector3 location, float heading, bool blockTempEvents = true)
        {
            await model.Request(10000);
            Ped spawnedPed = await World.CreatePed(model, location, heading);
            model.MarkAsNoLongerNeeded();
            API.TaskSetBlockingOfNonTemporaryEvents(spawnedPed.Handle, blockTempEvents);
            return spawnedPed;
        }
    }
}
