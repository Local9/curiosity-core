using Curiosity.Systems.Client.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
