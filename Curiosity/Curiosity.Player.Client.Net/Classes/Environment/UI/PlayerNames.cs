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
        static internal float MarkerDistance = 25;
        static internal float MarkerVehicleDistance = 50;

        static public void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
            //Client.GetInstance().RegisterEventHandler("onClientResourceStop", new Action<string>(OnCientResourceStop));
        }

        static internal bool ShouldShowName(CitizenFX.Core.Player player)
        {
            bool isCloseEnough;
            if (!player.Character.IsInVehicle())
            {
                isCloseEnough = Math.Sqrt(player.Character.Position.DistanceToSquared(Game.PlayerPed.Position)) < MarkerDistance;
            }
            else
            {
                isCloseEnough = Math.Sqrt(player.Character.Position.DistanceToSquared(Game.PlayerPed.Position)) < MarkerVehicleDistance;
            }
            bool isSneaking = player.Character.IsInStealthMode || player.Character.IsInCover() || Function.Call<bool>(Hash.IS_PED_USING_SCENARIO, player.Character.Handle, "WORLD_HUMAN_SMOKING") /*|| (player.Character.IsInVehicle() && player.Character.CurrentVehicle.Speed < 3.0)*/;
            bool isCurrentPlayer = (Game.Player == player);
            if (isCloseEnough && !isSneaking && !isCurrentPlayer)
                return true;
            return false;
        }

        static internal async Task OnTick()
        {
            try
            {
                if (CinematicMode.DoHideHud) return;

                MarkerPlayers = new PlayerList().Where(ShouldShowName);
                List<CitizenFX.Core.Player> playerList = MarkerPlayers.ToList();
                playerList.OrderBy(p => p.Character.Position.DistanceToSquared(Game.PlayerPed.Position)).Select((player, rank) => new { player, rank }).ToList().ForEach(async p => await ShowName(p.player));
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR PlayerNames: {ex.Message}");
            }

            await Task.FromResult(0);
        }

        //static internal async void OnCientResourceStop(string resourceName)
        //{

        //}

        static internal async Task ShowName(CitizenFX.Core.Player player)
        {
            if (!API.NetworkIsPlayerActive(player.Handle) && Game.Player.Handle == player.Handle) return;

            int gamerTagId = API.CreateMpGamerTag(player.Character.Handle, player.Name, false, false, "", 0);

            if (!API.NetworkIsPlayerActive(player.Handle))
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
            }
            else if (CinematicMode.DoHideHud)
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
            }
            else if (API.HasEntityClearLosToEntity(Game.PlayerPed.Handle, player.Character.Handle, 17))
            {
                API.SetMpGamerTagName(gamerTagId, player.Name);
                API.SetMpGamerTagVisibility(gamerTagId, 0, true);
            }
            else
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
            }

            await Task.FromResult(0);
        }
    }
}
