using CitizenFX.Core.Native;
using System;

namespace Curiosity.Systems.Client.Discord
{
    public class DiscordRichPresence
    {
        public int MaximumPlayers { get; set; }

        public string AppId { get; set; }
        public string Asset { get; set; }
        public string AssetText { get; set; }

        public string SmallAsset { get; set; }
        public string SmallAssetText { get; set; }
        public string Status { get; set; }

        public DiscordRichPresence(int maximumPlayers, string appId, string asset, string assetText)
        {
            MaximumPlayers = maximumPlayers;
            AppId = appId;
            Asset = asset;
            AssetText = assetText;
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

            if (Status != null) API.SetRichPresence($"({API.GetNumberOfPlayers()}/{MaximumPlayers}): {Status}");
        }
    }
}