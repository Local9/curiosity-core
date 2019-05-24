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
            //Server.GetInstance().RegisterTickHandler(UpdateSessions);
        }

        static async Task UpdateSessions()
        {
            while (true)
            {
                foreach(KeyValuePair<string, Session> playerItem in PlayerList)
                {
                    Session session = playerItem.Value;

                    session.User = await Database.DatabaseUsers.GetUserAsync(playerItem.Value.License);
                    session.Privilege = (Privilege)session.User.RoleId;
                    
                    await BaseScript.Delay(50);
                }
                await BaseScript.Delay(3000000);
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
