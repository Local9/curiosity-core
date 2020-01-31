using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Server.net.Classes.Environment
{
    class Scoreboard
    {
        static Dictionary<int, dynamic[]> list = new Dictionary<int, dynamic[]>();
        static Server server = Server.GetInstance();

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Scoreboard:MaxPlayers", new Action<CitizenFX.Core.Player>(OnGetMaxPlayers));
            server.RegisterEventHandler("curiosity:Server:Scoreboard:SetPlayerRow", new Action<int, string, int, bool>(OnSetPlayerConfig));
        }

        static async void OnGetMaxPlayers([FromSource]CitizenFX.Core.Player player)
        {
            player.TriggerEvent("curiosity:Client:Scoreboard:MaxPlayers", int.Parse(GetConvar("sv_maxclients", "32").ToString()));
            var pl = Server.players;
            foreach (CitizenFX.Core.Player p in pl)
            {
                if (list.ContainsKey(int.Parse(p.Handle)))
                {
                    var listItem = list[int.Parse(p.Handle)];
                    var p1 = listItem[0];
                    var p2 = listItem[1];
                    var p3 = listItem[2];
                    var p4 = listItem[3];
                    player.TriggerEvent("curiosity:Client:Scoreboard:SetPlayerRow", p1, p2, p3, p4);
                    await Server.Delay(1);
                }
            }
        }

        static void OnSetPlayerConfig(int playerServerId, string crewName, int jobPoints = -1, bool showJobPointsIcon = false)
        {
            if (playerServerId > 0)
            {
                list[playerServerId] = new dynamic[4] { playerServerId, crewName ?? "", jobPoints, showJobPointsIcon };
                BaseScript.TriggerClientEvent("curiosity:Client:Scoreboard:SetPlayerRow", playerServerId, crewName ?? "", jobPoints, showJobPointsIcon);
            }
        }
    }
}
