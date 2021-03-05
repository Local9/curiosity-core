using Newtonsoft.Json;

namespace Curiosity.Core.Server.Web.Discord.Entity
{
    public class EmbedThumbnail
    {
        [JsonProperty(PropertyName = "url", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Url;
    }
}
