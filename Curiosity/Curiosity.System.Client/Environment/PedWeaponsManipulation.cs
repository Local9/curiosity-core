using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.System.Client.Managers;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.System.Client.Environment
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