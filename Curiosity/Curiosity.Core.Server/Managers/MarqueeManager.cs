using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class MarqueeManager : Manager<MarqueeManager>
    {
        DateTime lastRun = DateTime.Now;

        List<string> MarqueeMessages = new List<string>()
        {
            "<span style='color:dodgerblue'>Forums:</span> <span style='color:ff9900'>forums.lifev.net</span>",
            "<span style='color:dodgerblue'>Discord:</span> <span style='color:#ff9900'>discord.lifev.net</span>",
            "Find any bugs? Please report them to our forums: <span style='color:#ff9900'>forums.lifev.net</span>",
            "Please use reports to notify us of rule breakers",
            "Please remember to be friendly to one another",
            "To read our rules Press the <span style='color:dodgerblue'>F11</span> or <span style='color:dodgerblue'>HOME</span> key",
            "<span style='color:dodgerblue'>Support us:</span> <Span color='#ff9900'>patreon.com/lifev</span>",
            "To read our guides press the <span style='color:dodgerblue'>F11</span> or <span style='color:dodgerblue'>HOME</span> key",
        };

        public override void Begin()
        {
        }

        [TickHandler]
        private async Task OnMarqueeTask()
        {
            if (DateTime.Now.Subtract(lastRun).TotalMinutes >= 5)
            {
                lastRun = DateTime.Now;
                string marqueeMessage = MarqueeMessages[Utility.RANDOM.Next(MarqueeMessages.Count - 1)];
                Logger.Debug($"Sending Marquee Message: {marqueeMessage}");
                EventSystem.GetModule().SendAll("ui:marquee", marqueeMessage);
            }
            await BaseScript.Delay(1000);
        }
    }
}
