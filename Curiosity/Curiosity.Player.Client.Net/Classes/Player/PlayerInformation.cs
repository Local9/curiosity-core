using CitizenFX.Core;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Models;
using System;

namespace Curiosity.Client.net.Classes.Player
{
    static class PlayerInformation
    {
        public static Privilege privilege;

        static PlayerInformationModel playerInfo = new PlayerInformationModel();

        public static async void Init()
        {
            Client.GetInstance().RegisterEventHandler("curiosity:Client:Player:GetInformation", new Action<string>(PlayerInfo));
            await BaseScript.Delay(10000);
            PeriodicCheck();
        }

        static async void PlayerInfo(string json)
        {
            playerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInformationModel>(json);

            privilege = (Privilege)playerInfo.RoleId;
        }

        public static bool IsAdmin()
        {
            return privilege.Has(Privilege.IsAdmin);
        }

        public static bool IsDeveloper()
        {
            return privilege.Has(Privilege.IsDeveloper);
        }

        static private async void PeriodicCheck()
        {
            while (true)
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Player:GetInformation");
                await BaseScript.Delay(10000);
            }
        }
    }
}
