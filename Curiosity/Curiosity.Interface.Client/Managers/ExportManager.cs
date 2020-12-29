using CitizenFX.Core.Native;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Interface.Client.Managers
{
    public class ExportManager : Manager<ExportManager>
    {
        public override void Begin()
        {
            Instance.ExportRegistry.Add("SetJobActivity", new Func<bool, bool, string, bool>(
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

            Instance.ExportRegistry.Add("GetClientWeather", new Func<string>(
                () =>
                {
                    return $"{SeasonManager.SeasonInstance.CurrentWeather}".ToUpper();
                }));

            Instance.ExportRegistry.Add("GetClientSeason", new Func<string>(
                () =>
                {
                    return $"{SeasonManager.SeasonInstance.CurrentSeason}".ToUpper();
                }));
        }
    }
}
