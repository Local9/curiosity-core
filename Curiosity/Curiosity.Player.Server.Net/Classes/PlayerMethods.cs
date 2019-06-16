using CitizenFX.Core;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Server.net.Classes
{
    class PlayerMethods
    {
        static Server server;

        public static void Init()
        {
            server = Server.GetInstance();

            server.RegisterEventHandler("curiosity:Server:Player:GetInformation", new Action<CitizenFX.Core.Player>(GetInformation));

            server.RegisterEventHandler("curiosity:Server:Player:Setup", new Action<CitizenFX.Core.Player>(OnSetupPlayer));
            server.RegisterEventHandler("curiosity:Server:Player:GetRole", new Action<CitizenFX.Core.Player>(GetUserRole));
            // Saves Data
            server.RegisterEventHandler("curiosity:Server:Player:SaveLocation", new Action<CitizenFX.Core.Player, float, float, float>(OnSaveLocation));
            // Internal Events
            server.RegisterEventHandler("curiosity:Server:Player:GetRoleId", new Action<int>(GetUserRoleId));
            server.RegisterEventHandler("curiosity:Server:Player:GetUserId", new Action<CitizenFX.Core.Player>(GetUserId));
            // admin methods
            server.RegisterEventHandler("curiosity:Server:Player:Kick", new Action<CitizenFX.Core.Player, string, string>(AdminKickPlayer));
            server.RegisterEventHandler("curiosity:Server:Player:Report", new Action<CitizenFX.Core.Player, string, string>(ReportingPlayer));
            server.RegisterEventHandler("curiosity:Server:Player:Ban", new Action<CitizenFX.Core.Player, string, string, bool, int>(AdminBanPlayer));
        }

        static bool IsStaff(Privilege privilege)
        {
            bool canKick = false;

            switch (privilege)
            {
                case Privilege.MODERATOR:
                case Privilege.DEVELOPER:
                case Privilege.ADMINISTRATOR:
                case Privilege.PROJECTMANAGER:
                case Privilege.SENIORADMIN:
                case Privilege.HEADADMIN:
                    canKick = true;
                    break;
                default:
                    canKick = false;
                    break;
            }

            return canKick;
        }

        async static void ReportingPlayer([FromSource]CitizenFX.Core.Player player, string playerHAndleBeingReported, string reason)
        {
            try
            {
                Session session = SessionManager.PlayerList[player.Handle];

                if (!IsStaff(session.Privilege)) return;

                if (!SessionManager.PlayerList.ContainsKey(playerHAndleBeingReported)) return;

                Session sessionOfPlayerBeingReported = SessionManager.PlayerList[playerHAndleBeingReported];

                if (sessionOfPlayerBeingReported.UserID == session.UserID)
                {
                    Helpers.Notifications.Advanced($"Really??", $"Please don't try to report yourself.", 221, player);
                    return;
                }

                if (IsStaff(sessionOfPlayerBeingReported.Privilege))
                {
                    Helpers.Notifications.Advanced($"Sigh...", $"Staff members are protected, please raise this in Discord.", 221, player);
                    return;
                }

                await Server.Delay(0);

                if (sessionOfPlayerBeingReported == null)
                {
                    Log.Warn("ReportingPlayer -> Player not found");
                }
                else
                {
                    string nameOfPlayerBeingReported = sessionOfPlayerBeingReported.Name;

                    if (nameOfPlayerBeingReported.Length > 20)
                    {
                        nameOfPlayerBeingReported = string.Format("{0}...", nameOfPlayerBeingReported.Substring(0, 20));
                    }

                    string[] reasonData = reason.Split('|');
                    string[] text = reasonData[1].Split(':');

                    // Database.DatabaseUsers.LogKick(sessionOfPlayerBeingReported.UserID, session.UserID, int.Parse(reasonData[0]), sessionOfPlayerBeingReported.User.CharacterId);

                    Helpers.Notifications.Advanced($"Reporting User", $"~g~Name: ~w~{nameOfPlayerBeingReported}~n~~g~Reason: ~w~{text[1].Trim()}~n~~g~By: ~w~{player.Name}", 17);

                    DiscordWrapper.SendDiscordReportMessage(player.Name, nameOfPlayerBeingReported, text[1].Trim());
                }
            }
            catch (Exception ex)
            {
                Log.Error($"ReportingPlayer -> {ex.Message}");
            }
        }

        async static void AdminKickPlayer([FromSource]CitizenFX.Core.Player player, string playerHandleToKick, string reason)
        {
            try
            {
                Session session = SessionManager.PlayerList[player.Handle];

                if (!IsStaff(session.Privilege)) return;

                if (!SessionManager.PlayerList.ContainsKey(playerHandleToKick)) return;

                Session sessionOfPlayerToKick = SessionManager.PlayerList[playerHandleToKick];

                if (sessionOfPlayerToKick.UserID == session.UserID)
                {
                    Helpers.Notifications.Advanced($"Really??", $"Please don't try to kick yourself.", 221, player);
                    return;
                }

                if (IsStaff(sessionOfPlayerToKick.Privilege))
                {
                    Helpers.Notifications.Advanced($"Sigh...", $"Staff members are protected.", 221, player);
                    return;
                }

                await Server.Delay(0);

                if (sessionOfPlayerToKick == null)
                {
                    Log.Warn("AdminKickPlayer -> Player not found");
                }
                else
                {
                    string nameOfPlayerBeingKicked = sessionOfPlayerToKick.Name;

                    if (nameOfPlayerBeingKicked.Length > 20)
                    {
                        nameOfPlayerBeingKicked = string.Format("{0}...", nameOfPlayerBeingKicked.Substring(0, 20));
                    }

                    string[] reasonData = reason.Split('|');
                    string[] text = reasonData[1].Split(':');

                    Database.DatabaseUsers.LogKick(sessionOfPlayerToKick.UserID, session.UserID, int.Parse(reasonData[0]), sessionOfPlayerToKick.User.CharacterId);

                    sessionOfPlayerToKick.Drop($"{reasonData[1]} by {player.Name}");

                    Helpers.Notifications.Advanced($"Kicked User", $"~g~Name: ~w~{nameOfPlayerBeingKicked}~n~~g~Reason: ~w~{text[1].Trim()}~n~~g~By: ~w~{player.Name}", 17);

                    DiscordWrapper.SendDiscordStaffMessage(player.Name, nameOfPlayerBeingKicked, "Kick", text[1].Trim(), string.Empty);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"AdminKickPlayer -> {ex.Message}");
            }
        }

        async static void AdminBanPlayer([FromSource]CitizenFX.Core.Player player, string playerHandleToKick, string reason, bool perm, int duration)
        {
            try
            {
                Session session = SessionManager.PlayerList[player.Handle];

                if (!IsStaff(session.Privilege)) return;

                if (!SessionManager.PlayerList.ContainsKey(playerHandleToKick)) return;

                Session sessionOfPlayerToBan = SessionManager.PlayerList[playerHandleToKick];

                if (sessionOfPlayerToBan.UserID == session.UserID)
                {
                    Helpers.Notifications.Advanced($"Wow... Really??", $"Trying to ban yourself?! You really want to be removed from the planet?", 221, player);
                    return;
                }

                if (IsStaff(sessionOfPlayerToBan.Privilege))
                {
                    Helpers.Notifications.Advanced($"Sigh...", $"Staff members are protected.", 221, player);
                    return;
                }

                await Server.Delay(0);

                if (sessionOfPlayerToBan == null)
                {
                    Log.Warn("AdminBanPlayer -> Player not found");
                }
                else
                {
                    string nameOfPlayerBeingBanned = sessionOfPlayerToBan.Name;

                    if (nameOfPlayerBeingBanned.Length > 20)
                    {
                        nameOfPlayerBeingBanned = string.Format("{0}...", nameOfPlayerBeingBanned.Substring(0, 20));
                    }

                    string[] reasonData = reason.Split('|');
                    string[] text = reasonData[1].Split(':');

                    DateTime bannedUntilTimestamp = DateTime.Now.AddDays(duration);

                    Database.DatabaseUsers.LogBan(sessionOfPlayerToBan.UserID, session.UserID, int.Parse(reasonData[0]), sessionOfPlayerToBan.User.CharacterId, perm, bannedUntilTimestamp);

                    string banDuration = string.Format("{0} Days", duration);
                    if (perm)
                        banDuration = "Permanently";

                    Helpers.Notifications.Advanced($"Banned User", $"~g~Name: ~w~{nameOfPlayerBeingBanned}~n~~g~Reason: ~w~{text[1].Trim()}~n~~g~Duration: ~w~{banDuration}~n~~g~By: ~w~{player.Name}", 8);

                    sessionOfPlayerToBan.Drop($"{reasonData[1]} by {player.Name} | Ban Duration: {banDuration}");

                    DiscordWrapper.SendDiscordStaffMessage(player.Name, nameOfPlayerBeingBanned, "Ban", text[1].Trim(), banDuration);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"AdminBanPlayer -> {ex.Message}");
            }
        }

        async static void GetInformation([FromSource]CitizenFX.Core.Player player)
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
            playerInformation.Skills = session.Skills;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerInformation);

            player.TriggerEvent("curiosity:Client:Player:GetInformation", json);
            await BaseScript.Delay(0);
            session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
            await BaseScript.Delay(0);
            session.Player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
            await BaseScript.Delay(0);
        }

        public async static void SendUpdatedInformation(Session session)
        {
            try
            {
                PlayerInformation playerInformation = new PlayerInformation();
                playerInformation.Handle = session.NetId;
                playerInformation.UserId = session.UserID;
                playerInformation.CharacterId = session.User.CharacterId;
                playerInformation.RoleId = (int)session.Privilege;
                playerInformation.Wallet = session.Wallet;
                playerInformation.BankAccount = session.BankAccount;
                playerInformation.Skills = session.Skills;

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerInformation);

                session.Player.TriggerEvent("curiosity:Client:Player:GetInformation", json);
                await BaseScript.Delay(0);
                session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
                await BaseScript.Delay(0);
                session.Player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
                await BaseScript.Delay(0);
            }
            catch (Exception ex)
            {
                Log.Error($"SendUpdatedInformation -> {ex}");
            }
        }

        async static void OnSetupPlayer([FromSource]CitizenFX.Core.Player player)
        {
            await SetupPlayerAsync(player);
        }

        async static Task SetupPlayerAsync(CitizenFX.Core.Player player)
        {
            try
            {
                await BaseScript.Delay(1000);

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
                session.Privilege = (Privilege)user.RoleId;
                session.LocationId = user.LocationId;
                session.IncreaseWallet(user.Wallet);
                session.IncreaseBankAccount(user.BankAccount);

                session.User = user;
                session.Skills = await Database.DatabaseUsersSkills.GetSkills(session.User.CharacterId);

                session.Activate();

                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Rank:SetInitialXpLevels", user.LifeExperience, true, true);
                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Player:SessionCreated", user.UserId);
                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
                await BaseScript.Delay(0);
                player.TriggerEvent("curiosity:Client:Player:SessionActivated");
                Log.Success($"session.Activate() -> {session.Name}");
            }
            catch (Exception ex)
            {
                Log.Error($"OnPlayerSetup -> {ex.Message}");
            }
        }

        async static void OnSaveLocation([FromSource]CitizenFX.Core.Player player, float x, float y, float z)
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

        async static void GetUserRole([FromSource]CitizenFX.Core.Player player)
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
            CitizenFX.Core.Player player = new PlayerList()[playerHandle];
            string license = player.Identifiers[Server.LICENSE_IDENTIFIER];
            Entity.User user = await Business.BusinessUser.GetUserAsync(license);
            player.TriggerEvent("curiosity:Server:Player:RoleId", user.RoleId);
        }

        async static void GetUserId([FromSource]CitizenFX.Core.Player player)
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
        public Dictionary<string, GlobalEntity.Skills> Skills;
    }
}
