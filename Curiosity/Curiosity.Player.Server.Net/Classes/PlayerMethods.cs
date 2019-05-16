using CitizenFX.Core;
using System;
using System.Collections.Generic;

namespace Curiosity.Server.net.Classes
{
    class PlayerMethods
    {
        public static void Init()
        {
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Player:GetInformation", new Action<Player>(GetInformation));
        }

        async static void GetInformation([FromSource]Player player)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

            Session session = SessionManager.PlayerList[player.Handle];

            PlayerInformation playerInformation = new PlayerInformation();
            playerInformation.Handle = session.NetId;
            playerInformation.UserId = session.UserID;
            playerInformation.RoleId = (int)session.Privilege;
            playerInformation.Wallet = session.Wallet;
            playerInformation.BankAccount = session.BankAccount;
            playerInformation.Skills = await Database.DatabaseUsersSkills.GetSkills(session.UserID);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerInformation);

            player.TriggerEvent("curiosity:Client:Player:GetInformation", json);

            await BaseScript.Delay(0);
        }
    }

    class PlayerInformation
    {
        public string Handle;
        public long UserId;
        public int RoleId;
        public int Wallet;
        public int BankAccount;
        public Dictionary<string, int> Skills;
    }
}
