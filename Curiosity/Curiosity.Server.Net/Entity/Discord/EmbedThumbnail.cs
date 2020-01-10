using Newtonsoft.Json;

namespace Curiosity.Server.net.Entity.Discord
{
    public class EmbedThumbnail
    {
        [JsonProperty(PropertyName = "url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Url;
    }
}