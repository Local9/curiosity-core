using System;

namespace Curiosity.MissionManager.Client.Managers
{
    public class ExportsManager : Manager<ExportsManager>
    {
        public override async void Begin()
        {
            Instance.ExportRegistry.Add("Discord", new Func<string, string, string, string, string, bool>(
                (asset, assetText, smallAsset, smallAssetTest, status) =>
                {
                    if (!string.IsNullOrEmpty(asset))
                        Instance.DiscordRichPresence.Asset = asset;

                    if (!string.IsNullOrEmpty(assetText))
                        Instance.DiscordRichPresence.AssetText = assetText;

                    if (!string.IsNullOrEmpty(smallAsset))
                        Instance.DiscordRichPresence.SmallAsset = smallAsset;

                    if (!string.IsNullOrEmpty(smallAssetTest))
                        Instance.DiscordRichPresence.SmallAssetText = smallAssetTest;

                    if (!string.IsNullOrEmpty(status))
                        Instance.DiscordRichPresence.Status = status;

                    if (!string.IsNullOrEmpty(status) || !string.IsNullOrEmpty(smallAssetTest) || !string.IsNullOrEmpty(smallAsset))
                    {
                        Instance.DiscordRichPresence.Commit();
                        return true;
                    }

                    return false;
                }
            ));
        }
    }
}
