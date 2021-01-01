using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client.Managers
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
            Instance.ExportRegistry.Add("Marquee", new Func<string, bool>(
                (message) =>
                {
                    SendNui(message);
                    return true;
                }));

            EventSystem.Attach("ui:marquee", new EventCallback(metadata =>
            {
                string message = metadata.Find<string>(0);
                SendNui(message);
                return true;
            }));
        }

        [TickHandler(SessionWait = true)]
        private async Task OnMarqueeTask()
        {
            if (DateTime.Now.Subtract(lastRun).TotalMinutes > 5)
            {
                lastRun = DateTime.Now;
                string marquee = MarqueeMessages[Utility.RANDOM.Next(MarqueeMessages.Count - 1)];
                SendNui(marquee);
            }
        }

        private void SendNui(string message)
        {
            string json = new JsonBuilder()
                .Add("operation", "MARQUEE")
                .Add("message", message.ToUpper())
                .Build();

            API.SendNuiMessage(json);
        }
    }
}
