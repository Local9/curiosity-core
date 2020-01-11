using CitizenFX.Core.Native;
using JetBrains.Annotations;

namespace Atlas.Roleplay.Client.Discord
{
    public class DiscordRichPresence
    {
        public int MaximumPlayers { get; set; }

        [NotNull] public string AppId { get; set; }
        [NotNull] public string Asset { get; set; }
        [NotNull] public string AssetText { get; set; }

        public string SmallAsset { get; set; }
        public string SmallAssetText { get; set; }
        public string Status { get; set; }

        public DiscordRichPresence(int maximumPlayers, [NotNull] string appId, [NotNull] string asset,
            [NotNull] string assetText)
        {
            MaximumPlayers = maximumPlayers;
            AppId = appId;
            Asset = asset;
            AssetText = assetText;
        }

        public void Commit()
        {
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