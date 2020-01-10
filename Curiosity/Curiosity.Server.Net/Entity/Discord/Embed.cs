using Newtonsoft.Json;

namespace Curiosity.Server.net.Entity.Discord
{
    public class Embed
    {
        [JsonProperty(PropertyName = "author", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public EmbedAuthor Author;

        [JsonProperty(PropertyName = "title", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Title;

        [JsonProperty(PropertyName = "description", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Description;

        [JsonProperty(PropertyName = "color", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Color;

        [JsonProperty(PropertyName = "thumbnail", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public EmbedThumbnail Thumbnail;
    }
}