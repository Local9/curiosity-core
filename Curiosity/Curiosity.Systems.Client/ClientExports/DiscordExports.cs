using Curiosity.Systems.Client.Managers;
using System;

namespace Curiosity.Systems.Client.ClientExports
{
    public class DiscordExports : Manager<DiscordExports>
    {
        public override void Begin()
        {
            Curiosity.ExportRegistry.Add("DiscordSetStatus", new Func<string, string>(
                (status) =>
                {
                    Curiosity.DiscordRichPresence.Status = status;
                    Curiosity.DiscordRichPresence.Commit();
                    return null;
                }
            ));
        }
    }
}
