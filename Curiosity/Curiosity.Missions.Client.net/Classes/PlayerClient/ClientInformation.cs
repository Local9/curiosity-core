using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Global.Shared.net.Data;
using System;

namespace Curiosity.Missions.Client.net.Classes.PlayerClient
{
    static class ClientInformation
    {
        static Client client = Client.GetInstance();
        public static PlayerInformationModel playerInfo = new PlayerInformationModel();

        public static Privilege privilege;

        public static async void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Player:InternalInformation", new Action<string>(PlayerInfo));
        }

        static async void PlayerInfo(string json)
        {
            playerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInformationModel>(json);

            if (privilege != (Privilege)playerInfo.RoleId)
            {
                privilege = (Privilege)playerInfo.RoleId;
            }

            await BaseScript.Delay(0);
        }
    }
}
