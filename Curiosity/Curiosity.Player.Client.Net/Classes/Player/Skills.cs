using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace Curiosity.Client.net.Classes.Player
{
    static class Skills
    {
        public static Dictionary<string, int> playerSkills = new Dictionary<string, int>();

        static public async void Init()
        {
            await BaseScript.Delay(10000);
            Client.GetInstance().RegisterEventHandler("curiosity:Player:Skills:Get", new Action<string>(UpdateSkillsList));
            PeriodicCheck();
        }

        static async void UpdateSkillsList(string json)
        {
            playerSkills = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
        }

        static private async void PeriodicCheck()
        {
            while (true)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Skills:Get");
                await BaseScript.Delay(60000);
            }
        }
    }
}
