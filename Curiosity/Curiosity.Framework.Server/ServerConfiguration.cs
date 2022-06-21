using Curiosity.Framework.Server.Models;

namespace Curiosity.Framework.Server
{
    static class ServerConfiguration
    {
        const string SERVER_CONFIG_LOCATION = $"/data/server-config.json";
        private static ServerConfig _serverConfig = null;

        private static ServerConfig GetConfig()
        {
            try
            {
                if (_serverConfig is not null)
                    return _serverConfig;

                string serverConfigFile = LoadResourceFile(GetCurrentResourceName(), SERVER_CONFIG_LOCATION);
                _serverConfig = JsonConvert.DeserializeObject<ServerConfig>(serverConfigFile);
                return _serverConfig;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Server Configuration was unable to be loaded.");
                return (ServerConfig)default!;
            }
        }

        public static ServerConfig GetServerConfig => GetConfig();
        public static DatabaseConfig GetDatabaseConfig => GetServerConfig.Database;
    }
}
