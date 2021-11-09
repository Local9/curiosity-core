using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers.GameWorld
{
    public class WorldPlayerManager : Manager<WorldPlayerManager>
    {
        Dictionary<int, WorldPlayer> players = new Dictionary<int, WorldPlayer>();

        public override void Begin()
        {
            
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWorldPlayerManagerTick()
        {
            try
            {
                foreach (Player player in Instance.PlayerList)
                {
                    if (player == Game.Player) continue;

                    if (!players.ContainsKey(player.ServerId))
                    {
                        players.Add(player.ServerId, new WorldPlayer(player));
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Debug(ex, $"OnWorldPlayerPassiveTick");
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWorldPlayersProcess()
        {
            if (players.Count == 0) return;

            foreach (KeyValuePair<int, WorldPlayer> keyValuePair in players.ToArray())
            {
                int serverHandle = keyValuePair.Key;
                WorldPlayer player = keyValuePair.Value;

                if (!player.IsReady) continue;

                if (!player.Exists)
                {
                    goto REMOVE_PLAYER;
                }

                continue;

            REMOVE_PLAYER:
                player.Dispose();
                players.Remove(serverHandle);
            }
        }
    }
}
