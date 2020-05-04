using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.FiveM;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Managers
{
    public class SystemManager : Manager<SystemManager>
    {
        public override void Begin()
        {
            Curiosity.EventRegistry["onResourceStop"] += new Action<string>(OnResourceStop);

            EventSystem.GetModule().Attach("server:playerList", new EventCallback(metadata =>
            {
                List<FiveMPlayer> players = new List<FiveMPlayer>();

                foreach(Player player in CuriosityPlugin.PlayersList)
                {
                    FiveMPlayer fiveMPlayer = new FiveMPlayer();
                    fiveMPlayer.ServerHandle = player.Handle;
                    fiveMPlayer.PlayerName = player.Name;

                    players.Add(fiveMPlayer);
                }

                FiveMPlayerList fiveMPlayerList = new FiveMPlayerList();
                fiveMPlayerList.Players = players;

                return fiveMPlayerList;
            }));
        }

        private void OnResourceStop(string resourceName)
        {
            if (resourceName != API.GetCurrentResourceName()) return;

            Logger.Info($"Stopping Curiosity Systems");
        }
    }
}
