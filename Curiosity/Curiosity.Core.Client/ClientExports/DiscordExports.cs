using Curiosity.Core.Client.Managers;
using System;

namespace Curiosity.Core.Client.ClientExports
{
    public class DiscordExports : Manager<DiscordExports>
    {
        public override void Begin()
        {
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
