using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Server.net.Classes
{
    public class SessionManager
    {
        public static Dictionary<string, Session> PlayerList = new Dictionary<string, Session>();

        public static string GetNetId(int userId)
        {
            return PlayerList.Select(x => x.Value).Where(x => x.UserID.Equals(userId)).First().NetId;
        }

        public static int GetUserId(string netId)
        {
            return PlayerList.Select(x => x.Value).Where(x => x.NetId.Equals(netId)).First().UserID;
        }

        public static Player GetPlayer(int userId)
        {
            return PlayerList.Select(x => x.Value).Where(x => x.UserID.Equals(userId)).First().Player;
        }
    }
}
