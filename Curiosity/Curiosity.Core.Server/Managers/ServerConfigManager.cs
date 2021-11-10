using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Environment;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class ServerConfigManager : Manager<ServerConfigManager>
    {
        ServerConfig _configCache;
        dynamic _loadingConfig;

        public override void Begin()
        {
            
        }

        private ServerConfig Get()
        {
            ServerConfig config = new();

            try
            {
                if (_configCache is not null)
                    return _configCache;

                _configCache = JsonConvert.DeserializeObject<ServerConfig>(Properties.Resources.server_config);
                return _configCache;
            }
            catch (Exception ex)
            {
                Logger.Error($"Config JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        public List<string> LoadingImages()
        {
            return Get().LoadingImages;
        }

        public List<Tip> LoadingTips()
        {
            return Get().Tips;
        }

        public dynamic GetLoadingConfig()
        {
            if (_loadingConfig is not null)
                return _loadingConfig;

            var config = new {
                images = new List<string>(),
                tips = new List<Tip>()
            };

            foreach(string img in LoadingImages())
            {
                config.images.Add(img);
            }

            foreach(Tip tip in LoadingTips())
            {
                config.tips.Add(tip);
            }

            return _loadingConfig = config;
        }
    }
}
