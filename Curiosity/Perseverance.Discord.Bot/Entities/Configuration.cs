using Newtonsoft.Json;

namespace Perseverance.Discord.Bot.Entities
{    public class Configuration
    {
        [JsonProperty("token")]
        public string Token { get; private set; }
        
        [JsonProperty("guild")]
        public long Guild { get; private set; }
        
        [JsonProperty("database")]
        public DatabaseInstance DatabaseInstance { get; set; }
    }

    public class DatabaseInstance
    {
        [JsonProperty("server")]
        public string Server { get; private set; }
        
        [JsonProperty("port")]
        public string Port { get; private set; }

        [JsonProperty("db")]
        public string DataBase { get; private set; }

        [JsonProperty("user")]
        public string User { get; private set; }
        
        [JsonProperty("password")]
        public string Password { get; private set; }
    }
}
