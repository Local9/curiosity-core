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
            "Forums: forums.lifev.net",
            "Discord: discord.lifev.net",
            "Please use reports to notify us of rule breakers via the player list",
            "Please remember to be friendly to one another",
            "Find any bugs? Please report them to our forums: forums.lifev.net",
            "To read our rules Press the HOME key",
            "Support us via patreon.com/lifev",
            "To read our guides press the [HOME] key",
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
