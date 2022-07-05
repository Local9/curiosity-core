namespace Curiosity.Framework.Server.Web.Discord.API
{
    public class DiscordAvatar
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id;

        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username;

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string Avatar;

        [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
        public string Discriminator;

        [JsonProperty("public_flags", NullValueHandling = NullValueHandling.Ignore)]
        public int PublicFlags;

        [JsonProperty("banner", NullValueHandling = NullValueHandling.Ignore)]
        public object Banner;

        [JsonProperty("banner_color", NullValueHandling = NullValueHandling.Ignore)]
        public object BannerColor;

        [JsonProperty("accent_color", NullValueHandling = NullValueHandling.Ignore)]
        public object AccentColor;

        [JsonProperty("avatarurl", NullValueHandling = NullValueHandling.Ignore)]
        public string Avatarurl;
    }
}
