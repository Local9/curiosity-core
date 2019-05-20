using CitizenFX.Core;
using System;
using System.Collections.Generic;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes
{
    class PlayerMethods
    {
        static Server server;

        public static void Init()
        {
            server = Server.GetInstance();

            server.RegisterEventHandler("curiosity:Server:Player:GetInformation", new Action<Player>(GetInformation));

            server.RegisterEventHandler("curiosity:Server:Player:Setup", new Action<Player>(OnSetupPlayer));
            server.RegisterEventHandler("curiosity:Server:Player:GetRole", new Action<Player>(GetUserRole));
            // Saves Data
            server.RegisterEventHandler("curiosity:Server:Player:SaveLocation", new Action<Player, float, float, float>(OnSaveLocation));
            // Internal Events
            server.RegisterEventHandler("curiosity:Server:Player:GetRoleId", new Action<int>(GetUserRoleId));
            server.RegisterEventHandler("curiosity:Server:Player:GetUserId", new Action<Player>(GetUserId));
        }

        async static void GetInformation([FromSource]Player player)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

            Session session = SessionManager.PlayerList[player.Handle];

            PlayerInformation playerInformation = new PlayerInformation();
            playerInformation.Handle = session.NetId;
            playerInformation.UserId = session.UserID;
            playerInformation.CharacterId = session.User.CharacterId;
            playerInformation.RoleId = (int)session.Privilege;
            playerInformation.Wallet = session.Wallet;
            playerInformation.BankAccount = session.BankAccount;
            playerInformation.Skills = await Database.DatabaseUsersSkills.GetSkills(session.User.CharacterId);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerInformation);

            player.TriggerEvent("curiosity:Client:Player:GetInformation", json);

            await BaseScript.Delay(0);
        }

        async static void OnSetupPlayer([FromSource]Player player)
        {
            await SetupPlayerAsync(player);
        }

        async static Task SetupPlayerAsync(Player player)
        {
            try
            {
                await BaseScript.Delay(3000);

                string license = player.Identifiers[Server.LICENSE_IDENTIFIER];

                if (string.IsNullOrEmpty(license))
                {
                    throw new Exception("LICENSE MISSING");
                }

                Entity.User user = await Business.BusinessUser.GetUserAsync(license);
                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Player:Setup", user.UserId, user.RoleId, user.Role, user.PosX, user.PosY, user.PosZ);
                await BaseScript.Delay(0);

                Session session = new Session(player);

                session.UserID = user.UserId;
                session.Privilege = (Enums.Privilege)user.RoleId;
                session.LocationId = user.LocationId;
                session.IncreaseWallet(user.Wallet);
                session.IncreaseBankAccount(user.BankAccount);

                session.User = user;

                session.Activate();

                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Rank:SetInitialXpLevels", user.LifeExperience, true, true);
                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Player:SessionCreated", user.UserId);
                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);

                Log.Success($"session.Activate() -> {session.Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"OnPlayerSetup -> {ex.Message}");
            }
        }

        async static void OnSaveLocation([FromSource]Player player, float x, float y, float z)
        {
            try
            {
                string license = player.Identifiers[Server.LICENSE_IDENTIFIER];

                if (string.IsNullOrEmpty(license))
                {
                    throw new Exception("LICENSE MISSING");
                }

                await Business.BusinessUser.SavePlayerLocationAsync(license, x, y, z);
            }
            catch (Exception ex)
            {
                Log.Error($"OnSaveLocation -> {ex.Message}");
            }
        }

        async static void GetUserRole([FromSource]Player player)
        {
            string license = player.Identifiers[Server.LICENSE_IDENTIFIER];

            if (string.IsNullOrEmpty(license))
            {
                throw new Exception("LICENSE MISSING");
            }

            Session session = SessionManager.PlayerList[player.Handle];

            session.User = await Business.BusinessUser.GetUserAsync(license);

            player.TriggerEvent("curiosity:Client:Player:Role", session.User.Role);
        }

        async static void GetUserRoleId(int playerHandle)
        {
            Player player = new PlayerList()[playerHandle];
            string license = player.Identifiers[Server.LICENSE_IDENTIFIER];
            Entity.User user = await Business.BusinessUser.GetUserAsync(license);
            player.TriggerEvent("curiosity:Server:Player:RoleId", user.RoleId);
        }

        async static void GetUserId([FromSource]Player player)
        {
            if (!Classes.SessionManager.PlayerList.ContainsKey(player.Handle))
            {
                player.TriggerEvent("curiosity:Client:Player:UserId", null);
                return;
            }
            long userId = Classes.SessionManager.GetUserId($"{player.Handle}");
            player.TriggerEvent("curiosity:Client:Player:UserId", userId);
            await BaseScript.Delay(0);
        }
    }

    class PlayerInformation
    {
        public string Handle;
        public int UserId;
        public int CharacterId;
        public int RoleId;
        public int Wallet;
        public int BankAccount;
        public Dictionary<string, int> Skills;
    }
}
