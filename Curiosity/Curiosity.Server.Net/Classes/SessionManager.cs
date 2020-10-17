using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Enums;
using Curiosity.Server.net.Business;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.Entity;

namespace Curiosity.Server.net.Classes
{
    static class SessionManager
    {
        static Server server = Server.GetInstance();

        static long GameTimer;
        const long SESSION_UPDATE = (1000 * 60) * 2;

        public static Dictionary<string, Session> PlayerList = new Dictionary<string, Session>();

        public static void Init()
        {
            server.RegisterTickHandler(UpdateSessions);

            GameTimer = API.GetGameTimer();

            server.RegisterEventHandler("curiosity:Server:SessionManager:GetSessions", new Action(OnGetSessions));
            server.RegisterEventHandler("curiosity:Server:Session:Ping", new Action<CitizenFX.Core.Player>(OnSessionPing));
        }

        static void OnSessionPing([FromSource]CitizenFX.Core.Player player)
        {
            try
            {
                if (!PlayerList.ContainsKey(player.Handle)) return;

                PlayerList[player.Handle].UpdateSessionTimer();
            }
            catch (Exception ex)
            {

            }
        }

        static void OnGetSessions()
        {

        }

        static async Task UpdateSessions()
        {
            while ((API.GetGameTimer() - GameTimer) > SESSION_UPDATE)
            {
                GameTimer = API.GetGameTimer();
                try
                {

                    UpdatePlayersInformation();
                }
                catch (Exception ex)
                {
                    Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "UpdateSessions", $"{ex}");
                    Log.Error($"UpdateSessions() -> {ex.Message}");
                }
            }
        }

        private static async void UpdatePlayersInformation()
        {
            try
            {
                if (!Server.isLive)
                {
                    Log.Verbose($"Running Session Update");
                }

                Dictionary<string, Session> PlayerListCopy = new Dictionary<string, Session>(PlayerList);

                Dictionary<string, Session> SessionsToDrop = new Dictionary<string, Session>();

                foreach (KeyValuePair<string, Session> playerItem in PlayerListCopy)
                {
                    Session session = playerItem.Value;

                    if (!Server.players.Contains(playerItem.Value.Player))
                    {
                        if (PlayerList.ContainsKey(playerItem.Key)) // Safe check
                            PlayerList.Remove(playerItem.Key);

                        if (Queue.session.ContainsKey(playerItem.Value.License))
                        {
                            SessionState sessionState = SessionState.Active;
                            Queue.session.TryRemove(playerItem.Value.License, out sessionState);
                        }
                    }
                    else
                    {

                        GlobalEntity.User User = await Database.DatabaseUsers.GetUserWithCharacterAsync(session.License, session.DiscordId, session.Player);

                        if (User != null)
                        {

                            session.Privilege = (Privilege)User.RoleId;
                            session.SetBankAccount(User.BankAccount);
                            session.SetWallet(User.Wallet);

                            PlayerInformation playerInformation = new PlayerInformation();
                            playerInformation.Handle = session.NetId;
                            playerInformation.UserId = session.UserID;
                            playerInformation.CharacterId = session.User.CharacterId;
                            playerInformation.RoleId = User.RoleId;
                            playerInformation.Wallet = User.Wallet;
                            playerInformation.BankAccount = User.BankAccount;

                            playerInformation.Skills = await Database.DatabaseUsersSkills.GetSkills(session.User.CharacterId);
                            session.Skills = playerInformation.Skills;


                            string json = Newtonsoft.Json.JsonConvert.SerializeObject(playerInformation);

                            session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", User.Wallet);
                            await BaseScript.Delay(0);
                            session.Player.TriggerEvent("curiosity:Client:Bank:UpdateBank", User.BankAccount);
                            await BaseScript.Delay(0);
                            session.Player.TriggerEvent("curiosity:Client:Player:GetInformation", json);
                            await BaseScript.Delay(0);
                        }

                        PlayerList[playerItem.Key] = session;
                    }

                    await BaseScript.Delay(100);
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "UpdatePlayersInformation", $"{ex}");
                Log.Warn($"UpdatePlayersInformation() -> {ex.Message}");
            }
        }

        public static string GetNetId(int userId)
        {
            lock (PlayerList)
            {
                return PlayerList.Select(x => x.Value).Where(x => x.UserID.Equals(userId)).First().NetId;
            }
        }

        public static long GetUserId(string netId)
        {
            lock (PlayerList)
            {
                return PlayerList.Select(x => x.Value).Where(x => x.NetId.Equals(netId)).First().UserID;
            }
        }

        public static CitizenFX.Core.Player GetPlayer(long userId)
        {
            lock (PlayerList)
            {
                return PlayerList.Select(x => x.Value).Where(x => x.UserID.Equals(userId)).First().Player;
            }
        }

        public static void UpdateUser(string netId, GlobalEntity.User user)
        {
            lock (PlayerList)
            {
                PlayerList.Select(x => x.Value).Where(x => x.NetId.Equals(netId)).First().User = user;
            }
        }

        public static bool SessionExists(string playerHandle)
        {
            lock (PlayerList)
            {
                return PlayerList.ContainsKey(playerHandle);
            }
        }

        //public static Session GetSession(string netId)
        //{
        //    return PlayerList[netId];
        //}
    }
}
