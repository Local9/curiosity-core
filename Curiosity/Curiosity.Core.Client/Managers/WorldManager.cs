using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Data;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class WorldManager : Manager<WorldManager>
    {
        WeatherType lastWeather;
        DateTime lastRun = DateTime.Now;

        public override void Begin()
        {
            
        }

        [TickHandler]
        private async Task OnWeatherSyncTick()
        {
            if (DateTime.Now.Subtract(lastRun).TotalSeconds >= 60)
            {
                lastRun = DateTime.Now;

                Vector3 pos = Game.PlayerPed.Position;

                string zoneStr = API.GetNameOfZone(pos.X, pos.Y, pos.Z);
                Enum.TryParse(zoneStr, out SubRegion subRegion);
                
                int srvRegionWeather = await EventSystem.Request<int>("weather:sync", (int)subRegion);
                WeatherType weatherType = (WeatherType)srvRegionWeather;

                if (!weatherType.Equals(lastWeather))
                {
                    Logger.Debug($"Weather Sync: {weatherType} : Region: {subRegion}");

                    await BaseScript.Delay(15000);

                    API.ClearOverrideWeather();
                    API.ClearWeatherTypePersist();
                    API.SetWeatherTypeOverTime($"{weatherType}", 15f);
                    API.SetWeatherTypePersist($"{weatherType}");
                    API.SetWeatherTypeNow($"{weatherType}");
                    API.SetWeatherTypeNowPersist($"{weatherType}");

                    lastWeather = weatherType;
                }
            }
        }
    }
}
