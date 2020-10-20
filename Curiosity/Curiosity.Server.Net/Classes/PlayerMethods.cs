using CitizenFX.Core;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.Entity;

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
            server.RegisterEventHandler("curiosity:Server:Player:Instance", new Action<CitizenFX.Core.Player>(PlayerInstanced));
            // Saves Data
            server.RegisterEventHandler("curiosity:Server:Player:SaveLocation", new Action<CitizenFX.Core.Player, float, float, float>(OnSaveLocation));
            // Internal Events
            server.RegisterEventHandler("curiosity:Server:Player:GetUserId", new Action<CitizenFX.Core.Player>(GetUserId));
            // admin methods
            server.RegisterEventHandler("curiosity:Server:Player:Kick", new Action<CitizenFX.Core.Player, string, string>(AdminKickPlayer));
            server.RegisterEventHandler("curiosity:Server:Player:Report", new Action<CitizenFX.Core.Player, string, string>(ReportingPlayer));
            server.RegisterEventHandler("curiosity:Server:Player:Ban", new Action<CitizenFX.Core.Player, string, string, bool, int>(AdminBanPlayer));
            server.RegisterEventHandler("curiosity:Server:Player:AfkKick", new Action<CitizenFX.Core.Player>(AfkKick));

            ///////// JOBS //////////
            server.RegisterEventHandler("curiosity:Server:Player:Job", new Action<CitizenFX.Core.Player, int>(OnPlayerJob));
            server.RegisterEventHandler("curiosity:Server:Player:police:dispatchToggle", new Action<CitizenFX.Core.Player, bool>(OnBlockDispatchMessages));
            server.RegisterEventHandler("curiosity:Server:Player:Backup", new Action<CitizenFX.Core.Player, int, float, float, float>(OnBackupRequest));

            server.RegisterEventHandler("curiosity:Server:Player:Revive", new Action<CitizenFX.Core.Player, string, bool>(OnPlayerRevive));
        }

        private static void OnBlockDispatchMessages([FromSource] CitizenFX.Core.Player player, bool toggle)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;
            Session session = SessionManager.PlayerList[player.Handle];
            session.JobMessages = toggle;
        }

        static void OnPlayerRevive([FromSource]CitizenFX.Core.Player player, string playerToRevive, bool killedByPlayer)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;
                if (!SessionManager.PlayerList.ContainsKey(playerToRevive)) return;

                Session playerRevivingSession = SessionManager.PlayerList[player.Handle];
                Session playerToReviveSession = SessionManager.PlayerList[playerToRevive];

                if (playerRevivingSession == null) return;
                if (playerToReviveSession == null) return;

                if (playerToReviveSession.Wallet < 500)
                {
                    if (!Server.isLive)
                        Log.Verbose($"Player {playerToReviveSession.Name}[{playerToReviveSession.NetId}] not enough cash to revive");

                    Helpers.Notifications.Advanced($"Not enough cash", $"Cannot revive, player doesn't have enough to pay you.", 221, player);
                    Helpers.Notifications.Advanced($"Not enough cash", $"Cannot revive, you do not have enough to pay for a revive.", 221, playerToReviveSession.Player);
                }
                else
                {
                    if (!Server.isLive)
                        Log.Verbose($"Player {playerRevivingSession.Name}[{playerRevivingSession.NetId}] reviving {playerToReviveSession.Name}[{playerToReviveSession.NetId}]");

                    if (!killedByPlayer)
                    {
                        Bank.IncreaseCashInternally(playerRevivingSession.Player.Handle, 50);
                        Bank.DecreaseCashInternally(playerToReviveSession.Player.Handle, 50);
                    }
                    else
                    {
                        Bank.DecreaseCashInternally(playerRevivingSession.Player.Handle, 500);
                    }

                    playerToReviveSession.Player.TriggerEvent("curiosity:Client:Player:Revive");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Revive > Critical Error: {ex.Message}");
                Log.Error($"{ex}");
            }
        }

        static void OnPlayerJob([FromSource]CitizenFX.Core.Player player, int job)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

            Session session = SessionManager.PlayerList[player.Handle];

            session.SetJob((Job)job);
        }

        static void OnBackupRequest([FromSource]CitizenFX.Core.Player player, int code, float x, float y, float z)
        {
            Server.TriggerClientEvent("curiosity:Client:Player:BackupNotification", player.Name, code, x, y, z);
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
                case Privilege.COMMUNITYMANAGER:
                    canKick = true;
                    break;
                default:
                    canKick = false;
                    break;
            }

            return canKick;
        }

        async static void AfkKick([FromSource]CitizenFX.Core.Player player)
        {
            player.Drop("You have been removed for being AFK");
            await Server.Delay(0);
        }

        async static void PlayerInstanced([FromSource]CitizenFX.Core.Player player)
        {
            player.Drop("Instanced Client, please reconnect");
            await Server.Delay(0);
        }

        async static void ReportingPlayer([FromSource]CitizenFX.Core.Player player, string playerHandleBeingReported, string reason)
        {
            try
            {
                if (!Server.isLive)
                    Log.Verbose($"Report by {player.Name}, handle: {playerHandleBeingReported}, reason: {reason}");

                Session session = SessionManager.PlayerList[player.Handle];

                if (!SessionManager.PlayerList.ContainsKey(playerHandleBeingReported)) return;

                Session sessionOfPlayerBeingReported = SessionManager.PlayerList[playerHandleBeingReported];

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

                    Database.DatabaseUsers.LogReport(sessionOfPlayerBeingReported.UserID, session.UserID, int.Parse(reasonData[0]), sessionOfPlayerBeingReported.User.CharacterId);

                    List<Session> sessions = SessionManager.PlayerList.Select(x => x.Value).Where(p => p.IsAdmin == true).ToList();

                    foreach (Session adminSession in sessions)
                    {
                        Helpers.Notifications.Advanced($"User being reported", $"~g~Name: ~w~{nameOfPlayerBeingReported}~n~~g~Reason: ~w~{text[1].Trim()}~n~~g~By: ~w~{player.Name}", 17, adminSession.Player);
                        await Server.Delay(1);
                    }

                    DiscordWrapper.SendDiscordReportMessage(player.Name, nameOfPlayerBeingReported, text[1].Trim());
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "ReportingPlayer", $"{ex}");
                Log.Error($"ReportingPlayer -> {ex.Message}");
            }
        }

        async static void AdminKickPlayer([FromSource]CitizenFX.Core.Player player, string playerHandleToKick, string reason)
        {
            try
            {
                if (!Server.isLive)
                    Log.Verbose($"Kick by {player.Name}, handle: {playerHandleToKick}, reason: {reason}");

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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "AdminKickPlayer", $"{ex}");
                Log.Error($"AdminKickPlayer -> {ex.Message}");
            }
        }

        async static void AdminBanPlayer([FromSource]CitizenFX.Core.Player player, string playerHandleToBan, string reason, bool perm, int duration)
        {
            try
            {
                if (!Server.isLive)
                    Log.Verbose($"Ban by {player.Name}, handle: {playerHandleToBan}, reason: {reason}, perm: {perm}, duration: {duration}");

                Session session = SessionManager.PlayerList[player.Handle];

                if (!IsStaff(session.Privilege)) return;

                if (!SessionManager.PlayerList.ContainsKey(playerHandleToBan)) return;

                Session sessionOfPlayerToBan = SessionManager.PlayerList[playerHandleToBan];

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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "AdminBanPlayer", $"{ex}");
                Log.Error($"AdminBanPlayer -> {ex.Message}");
            }
        }

        async static void GetInformation([FromSource]CitizenFX.Core.Player player)
        {
            try
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
                session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
                await BaseScript.Delay(0);
                session.Player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
                await BaseScript.Delay(0);
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "GetInformation", $"{ex}");
            }
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
                playerInformation.Skills = await Database.DatabaseUsersSkills.GetSkills(session.User.CharacterId);

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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "SendUpdatedInformation", $"{ex}");
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
                await BaseScript.Delay(100);

                string license = player.Identifiers[Server.LICENSE_IDENTIFIER];
                ulong discordId = ulong.Parse(player.Identifiers[Server.DISCORD_IDENTIFIER]);
                string playerName = player.Name;

                if (string.IsNullOrEmpty(license))
                {
                    throw new Exception("LICENSE MISSING");
                }

                if (SessionManager.PlayerList.ContainsKey(license))
                {
                    player.Drop("Duplicate account detected.");
                    return;
                }

                GlobalEntity.User user = await Business.BusinessUser.GetUserAsync(license, discordId, player);
                await BaseScript.Delay(3000);

                if (user == null)
                {
                    player.Drop("Sorry there was an error when creating your account and the development team have been informed, please try again later.");
                    Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "User Not Found", "SetupPlayerAsync -> User Failed to Create", $"\nDiscordID: {discordId}\nLicense: {license}\nPlayer: {playerName}");
                    return;
                }

                player.TriggerEvent("curiosity:Client:Player:Setup", Newtonsoft.Json.JsonConvert.SerializeObject(user));
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
                player.TriggerEvent("curiosity:Client:Player:SessionActivated", Server.showPlayerBlips, Server.showPlayerLocation, Server.minutesAFK);
                Log.Success($"session.Activate() -> {session.Name}");
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "OnPlayerSetup", $"{ex}");
                Log.Error($"OnPlayerSetup -> {ex.Message}");
                player.Drop("Sorry there was a critical error when creating your account, please try again.");
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

                await Business.BusinessUser.SavePlayerLocationAsync(player.Handle, x, y, z);
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "OnSaveLocation", $"{ex}");
                Log.Error($"OnSaveLocation -> {ex.Message}");
            }
        }

        async static void GetUserRole([FromSource]CitizenFX.Core.Player player)
        {
            try
            {
                string license = player.Identifiers[Server.LICENSE_IDENTIFIER];
                ulong discord = ulong.Parse(player.Identifiers[Server.DISCORD_IDENTIFIER]);

                if (string.IsNullOrEmpty(license))
                {
                    throw new Exception("LICENSE MISSING");
                }

                if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

                Session session = SessionManager.PlayerList[player.Handle];

                session.User = await Business.BusinessUser.GetUserAsync(license, discord, session.Player);

                player.TriggerEvent("curiosity:Client:Player:Role", session.User.Role);
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "GetUserRole", $"{ex}");
                Log.Error($"GetUserRole() -> {player.Name} - {ex.Message}");
            }
        }

        async static void GetUserId([FromSource]CitizenFX.Core.Player player)
        {
            try
            {
                string license = player.Identifiers[Server.LICENSE_IDENTIFIER];

                if (string.IsNullOrEmpty(license))
                {
                    throw new Exception("LICENSE MISSING");
                }

                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    player.TriggerEvent("curiosity:Client:Player:UserId", null);
                    return;
                }
                long userId = Classes.SessionManager.GetUserId($"{player.Handle}");
                player.TriggerEvent("curiosity:Client:Player:UserId", userId);
                await BaseScript.Delay(0);
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "GetUserId", $"{ex}");
                Log.Error($"GetUserId() -> {ex.Message}");
            }
        }
    }

    class PlayerInformation
    {
        public string Handle;
        public long UserId;
        public long CharacterId;
        public int RoleId;
        public int Wallet;
        public int BankAccount;
        public ConcurrentDictionary<string, GlobalEntity.Skills> Skills;
    }
}
