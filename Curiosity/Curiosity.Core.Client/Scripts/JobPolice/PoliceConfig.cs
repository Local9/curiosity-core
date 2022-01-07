using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models.Config;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Scripts.JobPolice
{
    static class PoliceConfig
    {
        static PoliceConfigFile _policeConfig;

        private static PoliceConfigFile GetPoliceConfig()
        {
            if (_policeConfig is not null)
                return _policeConfig;

            PoliceConfigFile config = new();

            try
            {
                _policeConfig = JsonConvert.DeserializeObject<PoliceConfigFile>(Properties.Resources.police);
                return _policeConfig;
            }
            catch (Exception ex)
            {
                Logger.Error($"Police Config JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        public static Dictionary<string, int> SpeedLimits => GetPoliceConfig().SpeedLimits;
        public static List<string> IgnoredVehicles => GetPoliceConfig().IgnoredVehicles;
        public static List<PoliceCamera> SpeedCameras => GetPoliceConfig().SpeedCameras;
        public static float SpeedCameraDistance => GetPoliceConfig().SpeedCameraDistance;
    }
}
