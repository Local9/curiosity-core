using Newtonsoft.Json;

namespace Curiosity.Server.net.Entity.Discord
{
    public class EmbedAuthor
    {
        [JsonProperty(PropertyName = "name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name;

        [JsonProperty(PropertyName = "icon_url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string IconUrl;
    }
}