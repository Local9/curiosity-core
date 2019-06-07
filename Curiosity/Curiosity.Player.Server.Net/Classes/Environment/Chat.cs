using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Server.net.Classes.Environment
{
    class Chat
    {
        static public void Init()
        {
            API.RegisterCommand("announce", new Action<int, List<object>, string>(Announcement), false);
        }

        static void Announcement(int playerHandle, List<object> arguments, string raw)
        {
            if (!SessionManager.PlayerList.ContainsKey($"{playerHandle}")) return;

            Session session = SessionManager.PlayerList[$"{playerHandle}"];

            if (session.Privilege == Enums.Privilege.USER) return;

            List<string> args = arguments.Cast<string>().ToList();
            Server.TriggerClientEvent("curiosity:Client:Scalefrom:Announce", String.Join(" ", args));
        }
    }
}
