using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Environment
{
    class MarqueeMessages
    {
        static Client client = Client.GetInstance();
        static Random random = new Random();

        static List<string> messages = new List<string>()
        {
            "Forums: forums.lifev.net",
            "Discord: discord.lifev.net",
            "Find any bugs? Please report them to our forums: forums.lifev.net",
            "Would like to join our discord? discord.lifev.net",
            "Please use reports to notify us of rule breakers",
            "Donation information can be found on our discord: discord.lifev.net",
            "Please remember to be friendly to one another",
            "To read our rules use /rules"
        };

        static public void Init()
        {
            client.RegisterTickHandler(OnMarqueeTask);
        }

        static async Task OnMarqueeTask()
        {
            await Client.Delay((1000 * 60) * 5); // 5mins
            MarqueeMessage msg = new MarqueeMessage() { marquee = messages[random.Next(messages.Count - 1)] };
            string json = JsonConvert.SerializeObject(msg);
            SendNuiMessage(json);
        }
    }

    class MarqueeMessage
    {
        public string marquee;
    }
}
