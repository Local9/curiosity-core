namespace Curiosity.Framework.Client.Discord
{
    public class DiscordRichPresence
    {
        public string AppId { get; set; }
        public string Asset { get; set; }
        public string AssetText { get; set; }

        public string SmallAsset { get; set; }
        public string SmallAssetText { get; set; }
        public string Status { get; set; }

        public DiscordRichPresence(string asset, string assetText)
        {
            AppId = GetResourceMetadata(GetCurrentResourceName(), "discord_app", 0); ;
            Asset = asset;
            AssetText = assetText;

            string serverAddress = API.GetCurrentServerEndpoint();
            API.SetDiscordRichPresenceAction(0, "Join Server", $"fivem://connect/{serverAddress}");
            string websiteText = GetResourceMetadata(GetCurrentResourceName(), "discord_button_text", 0);
            string websiteUrl = GetResourceMetadata(GetCurrentResourceName(), "discord_button_url", 0);
            API.SetDiscordRichPresenceAction(1, websiteText, websiteUrl);
        }

        public void Commit()
        {
            if (string.IsNullOrEmpty(AppId))
            {
                throw new ArgumentNullException("AppId must be supplied");
            }

            API.SetDiscordAppId(AppId);
            API.SetDiscordRichPresenceAssetSmall(Asset);
            API.SetDiscordRichPresenceAssetText(AssetText);

            if (SmallAsset != null && SmallAssetText != null)
            {
                API.SetDiscordRichPresenceAssetSmall(SmallAsset);
                API.SetDiscordRichPresenceAssetSmallText(SmallAssetText);
            }

            if (Status != null) API.SetRichPresence($"{Status}");
        }
    }
}
