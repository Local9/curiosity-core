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
        static Client client = Client.GetInstance();

        static internal IEnumerable<CitizenFX.Core.Player> MarkerPlayers;
        static internal float namePlateDistance = 250;
        static internal float namePlateVehicleDistance = 250;

        static bool isSpectating = false;

        static public void Init()
        {
            client.RegisterTickHandler(OnPlayerNamesTick);
            client.RegisterEventHandler("curioisty:UI:IsSpectating", new Action<bool>(OnIsSpectating));
        }

        static void OnIsSpectating(bool isSpec)
        {
            if (!Player.PlayerInformation.IsStaff()) return;

            isSpectating = isSpec;
        }

        static internal bool ShouldShowName(CitizenFX.Core.Player player)
        {
            if (isSpectating)
            {
                return true;
            }

            bool isCloseEnough;

            if (!player.Character.IsVisible) return false;

            if (!API.IsEntityVisible(player.Character.Handle)) return false;

            if (!player.Character.IsInVehicle())
            {
                isCloseEnough = Math.Sqrt(player.Character.Position.DistanceToSquared(Game.PlayerPed.Position)) < namePlateDistance;
            }
            else
            {
                isCloseEnough = Math.Sqrt(player.Character.Position.DistanceToSquared(Game.PlayerPed.Position)) < namePlateVehicleDistance;
            }
            bool isCurrentPlayer = (Game.Player == player);
            if (isCloseEnough && !isCurrentPlayer)
                return true;
            return false;
        }

        static internal async Task OnPlayerNamesTick()
        {
            try
            {
                if (CinematicMode.DoHideHud) return;

                MarkerPlayers = Client.players.Where(ShouldShowName);
                List<CitizenFX.Core.Player> playerList = MarkerPlayers.ToList();
                playerList.OrderBy(p => p.Character.Position.DistanceToSquared(Game.PlayerPed.Position)).Select((player, rank) => new { player, rank }).ToList().ForEach(async p => await ShowName(p.player));
            }
            catch (Exception ex)
            {
                Log.Error($"ERROR PlayerNames: {ex.Message}");
            }
        }

        static internal async Task ShowName(CitizenFX.Core.Player player)
        {
            if (!API.NetworkIsPlayerActive(player.Handle) && Game.Player.Handle == player.Handle) return;

            bool staffMember = Player.PlayerInformation.IsStaff();
            string staffTag = staffMember ? "[STAFF]" : string.Empty;

            int gamerTagId = API.CreateMpGamerTag(player.Character.Handle, player.Name, false, staffMember, staffTag, 0);

            if (!API.NetworkIsPlayerActive(player.Handle))
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
            }
            else if (player.Character.Opacity == 0)
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
            }
            else if (!API.IsEntityVisible(player.Character.Handle))
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
            }
            else if (!player.Character.IsVisible)
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
            }
            else if (CinematicMode.DoHideHud)
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
            }
            else if (API.HasEntityClearLosToEntity(Game.PlayerPed.Handle, player.Character.Handle, 17) && !isSpectating)
            {
                API.SetMpGamerTagName(gamerTagId, player.Name);
                API.SetMpGamerTagVisibility(gamerTagId, 0, true);
            }
            else
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
            }
        }
    }
}
