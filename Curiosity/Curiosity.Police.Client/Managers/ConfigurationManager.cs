using Curiosity.Police.Client.Diagnostics;
using Curiosity.Police.Client.Environment.Entities.Models;
using Newtonsoft.Json;
using System;

namespace Curiosity.Police.Client.Managers
{
    public class ConfigurationManager : Manager<ConfigurationManager>
    {
        PoliceConfig _policeConfig;

        public override void Begin()
        {
            
        }

        private PoliceConfig GetPoliceConfig()
        {
            if (_policeConfig is not null)
                return _policeConfig;

            PoliceConfig config = new();

            try
            {
                _policeConfig = JsonConvert.DeserializeObject<PoliceConfig>(Properties.Resources.police);
                return _policeConfig;
            }
            catch (Exception ex)
            {
                Logger.Error($"Police Config JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }
    }
}
