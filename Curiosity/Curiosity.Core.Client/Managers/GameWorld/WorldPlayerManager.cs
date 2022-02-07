using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.GameWorld
{
    public class WorldPlayerManager : Manager<WorldPlayerManager>
    {
        public Dictionary<int, WorldPlayer> WorldPlayers = new Dictionary<int, WorldPlayer>();

        public override void Begin()
        {

        }

        [TickHandler(SessionWait = true)]
        private async Task OnInteriorChecks()
        {
            int interiorId = GetInteriorFromEntity(Game.PlayerPed.Handle);
            if (interiorId > 0)
            {
                HideMinimapExteriorMapThisFrame();
            }

            SetInteriorZoomLevelIncreased(interiorId > 0);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWorldPlayerManagerTick()
        {
            try
            {
                foreach (Player player in Instance.PlayerList)
                {
                    if (player == Game.Player) continue;

                    if (!WorldPlayers.ContainsKey(player.ServerId))
                    {
                        WorldPlayers.Add(player.ServerId, new WorldPlayer(player));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"OnWorldPlayerPassiveTick");
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnWorldPlayersProcess()
        {
            if (WorldPlayers.Count == 0) return;

            foreach (KeyValuePair<int, WorldPlayer> keyValuePair in WorldPlayers.ToArray())
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
                WorldPlayers.Remove(serverHandle);
            }
        }
    }
}
