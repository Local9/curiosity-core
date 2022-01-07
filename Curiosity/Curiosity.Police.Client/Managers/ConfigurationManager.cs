using Curiosity.Police.Client.Diagnostics;
using Curiosity.Police.Client.Environment.Entities.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Police.Client.Managers
{
    public class ConfigurationManager : Manager<ConfigurationManager>
    {
        PoliceConfigFile _policeConfig;

        public override void Begin()
        {
            
        }

        private PoliceConfigFile GetPoliceConfig()
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

        public Dictionary<string, int> SpeedLimits => GetPoliceConfig().SpeedLimits;
        public List<string> IgnoredVehicles => GetPoliceConfig().IgnoredVehicles;
        public List<PoliceCamera> SpeedCameras => GetPoliceConfig().SpeedCameras;
        public float SpeedCameraDistance => GetPoliceConfig().SpeedCameraDistance;
    }
}
