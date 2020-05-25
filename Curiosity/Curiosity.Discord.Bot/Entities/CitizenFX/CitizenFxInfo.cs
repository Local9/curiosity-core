using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.LifeV.Bot.Entities.CitizenFX
{
    struct CitizenFxInfo
    {
        [JsonProperty("enhancedHostSupport")]
        public bool EnhancedHostSupport { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("resources")]
        public List<string> Resources { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("vars")]
        public Dictionary<string, string> Variables { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }
    }
}
