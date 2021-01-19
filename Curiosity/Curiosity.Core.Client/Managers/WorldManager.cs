using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class WorldManager : Manager<WorldManager>
    {
        WeatherType lastWeather = WeatherType.UNKNOWN;
        CuriosityWeather CuriosityWeather = new CuriosityWeather();
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

            Instance.ExportDictionary.Add("GetWeather", new Func<int>(
                () =>
                {
                    Instance.ExportDictionary["curiosity-ui"].SetWeather((int)CuriosityWeather.WeatherType, (int)CuriosityWeather.Season);
                    return (int)CuriosityWeather.WeatherType;
                }));

            UpdateWeather();
            LoadIpls();
        }

        private void LoadIpls()
        {
            API.LoadMpDlcMaps();
            API.EnableMpDlcMaps(true);

            // Ferris Wheel
            API.RequestIpl("ferris_finale_anim");
            API.RequestIpl("ferris_finale_anim_lod");

            // Tunnel Roof
            API.RequestIpl("dt1_03_gr_closed");

            // Missing Elevators
            API.RequestIpl("dt1_21_prop_lift");
            // API.RequestIpl("dt1_21_prop_lift_on");

            // Fountain Fix
            API.RequestIpl("DT1_05_HC_REMOVE");

            API.RequestIpl("cs5_4_trains"); // 2773.61 2835.327 35.1903
            API.RequestIpl("airfield"); // 1743.682 3286.251 40.0875
            API.RequestIpl("trailerparkA_grp1"); // Lost trailer
            API.RequestIpl("dockcrane1"); // 889.3 -2910.9 40
            API.RequestIpl("chophillskennel"); // 19.0568 536.4818 169.6277

            // FIB WINDOW: 136.1795f, -750.701f, 262.0516f
            API.RequestIpl("atriumglstatic");

            // Hospital: 330.4596 -584.8196 42.3174
            API.RemoveIpl("RC12B_HospitalInterior"); // Broken interior
            API.RemoveIpl("RC12B_Destroyed"); // broken windows
            API.RequestIpl("RC12B_Default"); // default look
            API.RemoveIpl("RC12B_Fixed"); // boarded up

            // Trevor: 1985.48132, 3828.76757, 32.5
            // Trash or Tidy.Only choose one.
            API.RequestIpl("TrevorsTrailer");
            API.RequestIpl("TrevorsTrailerTidy");

            // rails: 2626.374 2949.869 39.1409
            API.RequestIpl("ld_rail_01_track");
            API.RequestIpl("ld_rail_02_track");
            API.RequestIpl("FBI_repair");

            // golf flags
            API.RequestIpl("golfflags");
        }

        [TickHandler]
        private async static Task OnRemoveCargoShip()
        {
            API.RemoveIpl("cargoship");
            API.RemoveIpl("sunkcargoship");
        }

        [TickHandler]
        private async Task OnWeatherSyncTick()
        {
            if (DateTime.Now.Subtract(lastRun).TotalSeconds >= 60)
            {
                UpdateWeather();
            }
        }

        async void UpdateWeather()
        {
            lastRun = DateTime.Now;

            Vector3 pos = Game.PlayerPed.Position;

            string zoneStr = API.GetNameOfZone(pos.X, pos.Y, pos.Z);
            Enum.TryParse(zoneStr, out SubRegion subRegion);

            CuriosityWeather = await EventSystem.Request<CuriosityWeather>("weather:sync", (int)subRegion);
            WeatherType weatherType = CuriosityWeather.WeatherType;

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

                Instance.ExportDictionary["curiosity-ui"].SetWeather((int)CuriosityWeather.WeatherType, (int)CuriosityWeather.Season);
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
