using CitizenFX.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.Server.net.Enums;

namespace Curiosity.Server.net.Classes
{
    static class SessionManager
    {
        public static Dictionary<string, Session> PlayerList = new Dictionary<string, Session>();

        public static void Init()
        {
            Server.GetInstance().RegisterTickHandler(UpdateSessions);
        }

        static async Task UpdateSessions()
        {
            while (true)
            {
                foreach(KeyValuePair<string, Session> playerItem in PlayerList)
                {
                    Session session = playerItem.Value;

                    session.User = await Database.DatabaseUsers.GetUserWithCharacterAsync(playerItem.Value.License);
                    session.Privilege = (Privilege)session.User.RoleId;
                    session.SetBankAccount(session.User.BankAccount);
                    session.SetWallet(session.User.Wallet);

                    session.Player.TriggerEvent("curiosity:Client:Bank:UpdateWallet", session.Wallet);
                    await BaseScript.Delay(0);
                    session.Player.TriggerEvent("curiosity:Client:Bank:UpdateBank", session.BankAccount);
                    await BaseScript.Delay(0);

                    await BaseScript.Delay(500);
                }
                await BaseScript.Delay((1000 * 60) * 2);
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

        public static Player GetPlayer(long userId)
        {
            lock (PlayerList)
            {
                return PlayerList.Select(x => x.Value).Where(x => x.UserID.Equals(userId)).First().Player;
            }
        }

        public static void UpdateUser(string netId, Entity.User user)
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
