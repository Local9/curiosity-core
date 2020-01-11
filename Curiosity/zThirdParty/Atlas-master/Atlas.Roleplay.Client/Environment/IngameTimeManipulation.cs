using System;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Managers;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment
{
    public class IngameTimeManipulation : Manager<IngameTimeManipulation>
    {
        [TickHandler]
        private async Task OnTick()
        {
            var date = DateTime.Now;

            API.NetworkOverrideClockTime(date.Hour, date.Minute, date.Second);

            await BaseScript.Delay(1000);
        }
    }
}