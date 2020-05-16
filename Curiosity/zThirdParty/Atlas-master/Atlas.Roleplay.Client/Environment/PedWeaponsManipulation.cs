using Atlas.Roleplay.Client.Managers;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Environment
{
    public class PedWeaponsManipulation : Manager<PedWeaponsManipulation>
    {
        [TickHandler(SessionWait = true)]
        private async Task OnTick()
        {
            World.GetAllPeds().ToList().ForEach(self => API.SetPedDropsWeaponsWhenDead(self.Handle, false));

            await BaseScript.Delay(500);
        }
    }
}