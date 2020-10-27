using CitizenFX.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GlobalEntity = Curiosity.Global.Shared.Entity;

namespace Curiosity.Client.net.Classes.PlayerClasses
{
    static class Skills
    {
        public static Dictionary<string, GlobalEntity.Skills> playerSkills = new Dictionary<string, GlobalEntity.Skills>();
        public static DateTime LastUpdate;

        static public async void Init()
        {
            Client.GetInstance().RegisterEventHandler("curiosity:Player:Skills:Get", new Action<string>(UpdateSkillsList));
            PeriodicCheck();
        }

        static async void UpdateSkillsList(string json)
        {
            playerSkills = JsonConvert.DeserializeObject<Dictionary<string, GlobalEntity.Skills>>(json);
        }

        static private async void PeriodicCheck()
        {
            await BaseScript.Delay(10000);
            while (true)
            {
                LastUpdate = DateTime.Now;
                BaseScript.TriggerServerEvent("curiosity:Server:Skills:Get");
                await BaseScript.Delay(60000);
            }
        }
    }
}
