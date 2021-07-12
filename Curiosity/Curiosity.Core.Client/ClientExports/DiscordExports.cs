using Curiosity.Core.Client.Managers;
using System;

namespace Curiosity.Core.Client.ClientExports
{
    public class DiscordExports : Manager<DiscordExports>
    {
        public override void Begin()
        {
            Instance.ExportDictionary.Add("DiscordSetAsset", new Func<string, string, string>(
                (asset, text) =>
                {
                    Instance.DiscordRichPresence.Asset = asset;
                    Instance.DiscordRichPresence.AssetText = text;
                    Instance.DiscordRichPresence.Commit();
                    return null;
                }
            ));

            Instance.ExportDictionary.Add("DiscordSetSmallAsset", new Func<string, string, string>(
                (asset, text) =>
                {
                    Instance.DiscordRichPresence.SmallAsset = asset;
                    Instance.DiscordRichPresence.SmallAssetText = text;
                    Instance.DiscordRichPresence.Commit();
                    return null;
                }
            ));

            Instance.ExportDictionary.Add("DiscordSetStatus", new Func<string, string>(
                (status) =>
                {
                    Instance.DiscordRichPresence.Status = status;
                    Instance.DiscordRichPresence.Commit();
                    return null;
                }
            ));
        }
    }
}
