using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers.GameWorld
{
    public class WorldPlayerPassiveManager : Manager<WorldPlayerPassiveManager>
    {
        Dictionary<int, WorldPlayer> players = new Dictionary<int, WorldPlayer>();

        public override void Begin()
        {
            
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWorldPlayerPassiveTick()
        {
            try
            {
                foreach (Player player in Instance.PlayerList)
                {
                    if (player == Game.Player) continue;

                    if (players.ContainsKey(player.ServerId)) continue;

                    players.Add(player.ServerId, new WorldPlayer(player));
                }

                foreach (KeyValuePair<int, WorldPlayer> keyValuePair in players.ToArray())
                {
                    int serverHandle = keyValuePair.Key;
                    WorldPlayer player = keyValuePair.Value;

                    if (player.Player is null)
                    {
                        goto REMOVE_PLAYER;
                    }

                    if (!player.Player.Character.Exists())
                    {
                        goto REMOVE_PLAYER;
                    }

                    if (Vector3.Distance(Game.PlayerPed.Position, player.Player.Character.Position) > 30f)
                    {
                        goto REMOVE_PLAYER;
                    }

                    continue;

                REMOVE_PLAYER:
                    player.Dispose();
                    players.Remove(serverHandle);
                }
            }
            catch(Exception ex)
            {
                Logger.Debug(ex, $"OnWorldPlayerPassiveTick");
            }
        }
    }
}
