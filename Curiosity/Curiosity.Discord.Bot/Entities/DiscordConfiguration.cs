using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.LifeV.Bot.Entities
{
    public class Logging
    {
        [JsonProperty("LogLevel")]
        public Dictionary<string, string> LogLevel { get; set; }
    }

    struct DiscordConfiguration
    {
        [JsonProperty("Logging")]
        public Logging Logging { get; set; }

        [JsonProperty("BotSettings")]
        public Dictionary<string, string> BotSettings { get; set; }

        [JsonProperty("ConnectionStrings")]
        public Dictionary<string, string> ConnectionStrings { get; set; }
    }
}
