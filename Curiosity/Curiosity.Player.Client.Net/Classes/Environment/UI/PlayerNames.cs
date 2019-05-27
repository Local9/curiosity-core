using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    static class PlayerNames
    {
        static internal IEnumerable<CitizenFX.Core.Player> MarkerPlayers;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static internal bool ShouldShowMarker(CitizenFX.Core.Player player)
        {
            bool isSneaking = player.Character.IsInStealthMode || player.Character.IsInCover() || Function.Call<bool>(Hash.IS_PED_USING_SCENARIO, player.Character.Handle, "WORLD_HUMAN_SMOKING") /*|| (player.Character.IsInVehicle() && player.Character.CurrentVehicle.Speed < 3.0)*/;
            bool isCurrentPlayer = (Game.Player == player);
            if (!isSneaking && !isCurrentPlayer)
                return true;
            return false;
        }

        static internal async Task OnTick()
        {
            try
            {
                if (CinematicMode.DoHideHud) return;

                MarkerPlayers = new PlayerList().Where(ShouldShowMarker);
                List<CitizenFX.Core.Player> playerList = MarkerPlayers.ToList();
                playerList.OrderBy(p => p.Character.Position.DistanceToSquared(Game.PlayerPed.Position)).Select((player, rank) => new { player, rank }).ToList().ForEach(p => API.CreateMpGamerTag(p.player.Handle, p.player.Name, false, false, "", 0));
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR PlayerNames: {ex.Message}");
            }

            await Task.FromResult(0);
        }
    }
}
