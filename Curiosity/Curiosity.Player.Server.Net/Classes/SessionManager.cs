using CitizenFX.Core;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Server.net.Classes
{
    public class SessionManager : BaseScript
    {
        public static Dictionary<string, Session> PlayerList = new Dictionary<string, Session>();

        static SessionManager sessionManager;

        public SessionManager GetInstance()
        {
            return sessionManager;
        }

        public SessionManager()
        {
            sessionManager = this;
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
    }
}
