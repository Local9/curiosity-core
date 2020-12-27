using System;

namespace Curiosity.MissionManager.Client.Managers
{
    public class ExportsManager : Manager<ExportsManager>
    {
        public override void Begin()
        {
            Instance.ExportRegistry.Add("Discord", new Func<string, string, string, string, string, bool>(
                (asset, assetText, smallAsset, smallAssetText, status) =>
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(asset))
                            Instance.DiscordRichPresence.Asset = asset;

                        if (!string.IsNullOrEmpty(assetText))
                            Instance.DiscordRichPresence.AssetText = assetText;

                        if (!string.IsNullOrEmpty(smallAsset))
                            Instance.DiscordRichPresence.SmallAsset = smallAsset;

                        if (!string.IsNullOrEmpty(smallAssetText))
                            Instance.DiscordRichPresence.SmallAssetText = smallAssetText;

                        if (!string.IsNullOrEmpty(status))
                            Instance.DiscordRichPresence.Status = status;

                        Instance.DiscordRichPresence.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
            ));
        }
    }
}
