using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class ConfigurationManager : Manager<ConfigurationManager>
    {
        ClientConfig _configCache;

        public override void Begin()
        {
            
        }

        private ClientConfig GetConfig()
        {
            ClientConfig config = new();

            string jsonFile = LoadResourceFile(GetCurrentResourceName(), "config/config.json"); // Fuck you VS2019 UTF8 BOM

            try
            {
                if (string.IsNullOrEmpty(jsonFile))
                {
                    Logger.Error($"config.json file is empty or does not exist, please fix this");
                }
                else
                {
                    if (_configCache is not null)
                        return _configCache;

                    _configCache = JsonConvert.DeserializeObject<ClientConfig>(jsonFile);
                    return _configCache;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Config JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        public List<string> VehiclesToSuppress()
        {
            return GetConfig().VehiclesToSuppress;
        }
    }
}
