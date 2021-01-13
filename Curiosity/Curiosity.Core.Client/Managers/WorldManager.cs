using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Events;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class WorldManager : Manager<WorldManager>
    {
        WeatherType lastWeather;
        DateTime lastRun = DateTime.Now;

        // Time
        double clientBaseTime = 0;
        double clientTimeOffset = 0;
        double clientTimer = 0;
        int hour;
        int minute;

        public override void Begin()
        {
            EventSystem.Attach("world:time", new EventCallback(metadata =>
            {
                clientBaseTime = metadata.Find<double>(0);
                clientTimeOffset = metadata.Find<double>(1);
                return null;
            }));
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
                    API.SetWeatherTypeOverTime($"{weatherType}", 30f);
                    API.SetWeatherTypePersist($"{weatherType}");
                    API.SetWeatherTypeNow($"{weatherType}");
                    API.SetWeatherTypeNowPersist($"{weatherType}");

                    lastWeather = weatherType;
                }
            }
        }

        [TickHandler]
        private async Task OnWorldTimeSyncTick()
        {
            await BaseScript.Delay(0);

            double newBaseTime = clientBaseTime;
            if ((API.GetGameTimer() - 500) > clientTimer)
            {
                newBaseTime += 0.25;
                clientTimer = API.GetGameTimer();
            }

            clientBaseTime = newBaseTime;

            hour = (int)Math.Floor(((clientBaseTime + clientTimeOffset) / 60) % 24);
            minute = (int)Math.Floor((clientBaseTime + clientTimeOffset) % 60);

            API.NetworkOverrideClockTime(hour, minute, 0);
            API.SetClockTime(hour, minute, 0);
        }
    }
}
