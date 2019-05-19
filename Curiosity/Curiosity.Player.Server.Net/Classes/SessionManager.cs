using CitizenFX.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.Server.net.Enums;

namespace Curiosity.Server.net.Classes
{
    public class SessionManager : BaseScript
    {
        public static Dictionary<string, Session> PlayerList = new Dictionary<string, Session>();

        static SessionManager sessionManager;

        public static SessionManager GetInstance()
        {
            return sessionManager;
        }

        public SessionManager()
        {
            sessionManager = this;
            Server.GetInstance().RegisterTickHandler(UpdateSessions);
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
                    
                    await Delay(50);
                }
                await Delay(120000);
            }
        }

        public static string GetNetId(int userId)
        {
            return PlayerList.Select(x => x.Value).Where(x => x.UserID.Equals(userId)).First().NetId;
        }

        public static long GetUserId(string netId)
        {
            return PlayerList.Select(x => x.Value).Where(x => x.NetId.Equals(netId)).First().UserID;
        }

        public static Player GetPlayer(long userId)
        {
            return PlayerList.Select(x => x.Value).Where(x => x.UserID.Equals(userId)).First().Player;
        }

        //public static bool SessionActive(string netId)
        //{
        //    return PlayerList.ContainsKey(netId);
        //}

        //public static Session GetSession(string netId)
        //{
        //    return PlayerList[netId];
        //}
    }
}
