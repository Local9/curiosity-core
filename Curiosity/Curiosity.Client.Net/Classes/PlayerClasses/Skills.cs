using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.Entity;

namespace Curiosity.Client.net.Classes.PlayerClasses
{
    static class Skills
    {
        static Client Instance = Client.GetInstance();
        public static Dictionary<string, GlobalEntity.Skills> playerSkills = new Dictionary<string, GlobalEntity.Skills>();
        public static DateTime LastUpdate;
        public static bool _startup = false;

        static public async void Init()
        {
            Client.GetInstance().RegisterEventHandler("curiosity:Player:Skills:Get", new Action<string>(UpdateSkillsList));
            Instance.RegisterTickHandler(PeriodicCheck);
        }

        static async void UpdateSkillsList(string json)
        {
            playerSkills = JsonConvert.DeserializeObject<Dictionary<string, GlobalEntity.Skills>>(json);
        }

        static private async Task PeriodicCheck()
        {
            if (!_startup)
            {
                await BaseScript.Delay(10000);
                _startup = true;
            }

            LastUpdate = DateTime.Now;
            BaseScript.TriggerServerEvent("curiosity:Server:Skills:Get");
            await BaseScript.Delay(60000);
        }
    }
}
