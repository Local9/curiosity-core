using Atlas.Roleplay.Client.Managers;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Atlas.Roleplay.Client.Environment
{
    public class WeatherManipulation : Manager<WeatherManipulation>
    {
        [TickHandler]
        private async Task OnTick()
        {
            API.SetOverrideWeather("EXTRASUNNY");

            await BaseScript.Delay(1000);
        }
    }
}