using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using System;

namespace Curiosity.Core.Client.Managers
{
    public class ExportManager : Manager<ExportManager>
    {
        WeatherType weatherType;
        WeatherSeason weatherSeason;
        string temp;

        public override void Begin()
        {
            Instance.ExportDictionary.Add("SetJobActivity", new Func<bool, bool, string, bool>(
                (active, onDuty, job) =>
                {
                    string msg = new JsonBuilder()
                        .Add("operation", "JOB_ACTIVITY")
                        .Add("jobActive", active)
                        .Add("jobOnDuty", onDuty)
                        .Add("jobTitle", job.ToTitleCase())
                        .Build();

                    API.SendNuiMessage(msg);

                    return true;
                }));

            Instance.ExportDictionary.Add("SetWeather", new Func<int, int, bool>(
                (weather, season) =>
                {
                    weatherType = (WeatherType)weather;
                    weatherSeason = (WeatherSeason)season;

                    string msg = new JsonBuilder()
                                .Add("operation", "WEATHER")
                                .Add("type", weatherType.GetStringValue())
                                .Add("temp", temp)
                                .Add("season", weatherSeason.GetStringValue())
                                .Build();

                    API.SendNuiMessage(msg);

                    return true;
                }));

            Instance.ExportDictionary.Add("AddToChat", new Func<string, bool>(
                (json) =>
                {
                    API.SendNuiMessage(json);
                    return true;
                }));
        }
    }
}
