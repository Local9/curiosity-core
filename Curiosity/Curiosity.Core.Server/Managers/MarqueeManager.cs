using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class MarqueeManager : Manager<MarqueeManager>
    {
        DateTime lastRun = DateTime.Now;
        int marqueeMessageIndex = 0;

        List<string> MarqueeMessages = new List<string>()
        {
            "<span style='color:dodgerblue; font-weight: bold'>Forums:</span> <span style='color:#ff9900; font-weight: bold'>forums.lifev.net</span>",
            "<span style='color:dodgerblue; font-weight: bold'>Discord:</span> <span style='color:#ff9900; font-weight: bold'>discord.lifev.net</span>",
            "Find any bugs? Please report them to our forums: <span style='color:#ff9900; font-weight: bold'>forums.lifev.net</span>",
            "Please use reports to notify us of rule breakers",
            "Please remember to be friendly to one another",
            "To read our rules Press the <span style='color:dodgerblue; font-weight: bold'>F11</span> or <span style='color:dodgerblue; font-weight: bold'>HOME</span> key",
            "<span style='color:dodgerblue; font-weight: bold'>Support us:</span> <Span color='#ff9900; font-weight: bold'>patreon.com/lifev</span>",
            "To read our guides press the <span style='color:dodgerblue; font-weight: bold'>F11</span> or <span style='color:dodgerblue; font-weight: bold'>HOME</span> key",
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
                string marqueeMessage = MarqueeMessages[marqueeMessageIndex];
                EventSystem.GetModule().SendAll("ui:marquee", marqueeMessage);

                marqueeMessageIndex++;
                if (marqueeMessageIndex >= MarqueeMessages.Count) marqueeMessageIndex = 0;
            }
            await BaseScript.Delay(1000);
        }
    }
}
