using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Core.Server.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class MarqueeManager : Manager<MarqueeManager>
    {
        long gameTimer = GetGameTimer();
        int fiveMinutes = (1000 * 60) * 5;
        int marqueeMessageIndex = 0;

        List<string> MarqueeMessages = new List<string>()
        {
            "<span class='text-info font-weight-bold'>Forums</span>: forums.lifev.net",
            "<span class='text-info font-weight-bold'>Discord</span>: discord.lifev.net",
            "Please use reports to notify us of rule breakers via the player list",
            "Please remember to be friendly to one another",
            "Found a bug or two? want to help get rid of them? Report them to our forums: <span class='text-info font-weight-bold'>forums.lifev.net</span>",
            "Make sure to read our rules and guides",
            "Support us via <span class='text-info font-weight-bold'>patreon.com/lifev</span>",
            "Remember you can change some of our binds in settings",
        };

        public override void Begin() { }

        [TickHandler]
        private async Task OnMarqueeTask()
        {
            if ((GetGameTimer() - fiveMinutes) > gameTimer)
            {
                gameTimer = GetGameTimer();
                string marqueeMessage = MarqueeMessages[marqueeMessageIndex];
                EventSystem.GetModule().SendAll("ui:marquee", marqueeMessage);

                marqueeMessageIndex++;
                if (marqueeMessageIndex >= MarqueeMessages.Count) marqueeMessageIndex = 0;
            }
            await BaseScript.Delay(1000);
        }
    }
}
