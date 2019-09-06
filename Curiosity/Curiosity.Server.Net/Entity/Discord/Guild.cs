using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Globalization;

namespace Curiosity.Server.net.Entity.Discord
{
    public partial class Guild
    {
        [JsonProperty("mfa_level", NullValueHandling = NullValueHandling.Ignore)]
        public long? MfaLevel { get; set; }

        [JsonProperty("application_id")]
        public object ApplicationId { get; set; }

        [JsonProperty("features", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Features { get; set; }

        [JsonProperty("afk_timeout", NullValueHandling = NullValueHandling.Ignore)]
        public long? AfkTimeout { get; set; }

        [JsonProperty("system_channel_id", NullValueHandling = NullValueHandling.Ignore)]
        public string SystemChannelId { get; set; }

        [JsonProperty("default_message_notifications", NullValueHandling = NullValueHandling.Ignore)]
        public long? DefaultMessageNotifications { get; set; }

        [JsonProperty("widget_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WidgetEnabled { get; set; }

        [JsonProperty("afk_channel_id")]
        public object AfkChannelId { get; set; }

        [JsonProperty("premium_subscription_count")]
        public object PremiumSubscriptionCount { get; set; }

        [JsonProperty("explicit_content_filter", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExplicitContentFilter { get; set; }

        [JsonProperty("max_presences")]
        public object MaxPresences { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public string Icon { get; set; }

        [JsonProperty("preferred_locale", NullValueHandling = NullValueHandling.Ignore)]
        public string PreferredLocale { get; set; }

        [JsonProperty("verification_level", NullValueHandling = NullValueHandling.Ignore)]
        public long? VerificationLevel { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public Role[] Roles { get; set; }

        [JsonProperty("widget_channel_id")]
        public object WidgetChannelId { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("embed_channel_id")]
        public object EmbedChannelId { get; set; }

        [JsonProperty("system_channel_flags", NullValueHandling = NullValueHandling.Ignore)]
        public long? SystemChannelFlags { get; set; }

        [JsonProperty("banner")]
        public object Banner { get; set; }

        [JsonProperty("premium_tier", NullValueHandling = NullValueHandling.Ignore)]
        public long? PremiumTier { get; set; }

        [JsonProperty("splash")]
        public object Splash { get; set; }

        [JsonProperty("max_members", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxMembers { get; set; }

        [JsonProperty("emojis", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Emojis { get; set; }

        [JsonProperty("embed_enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? EmbedEnabled { get; set; }

        [JsonProperty("region", NullValueHandling = NullValueHandling.Ignore)]
        public string Region { get; set; }

        [JsonProperty("vanity_url_code")]
        public object VanityUrlCode { get; set; }

        [JsonProperty("owner_id", NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerId { get; set; }
    }

    public partial class Role
    {
        [JsonProperty("hoist", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Hoist { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("mentionable", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Mentionable { get; set; }

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public long? Color { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public long? Position { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("managed", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Managed { get; set; }

        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public long? Permissions { get; set; }
    }
}
