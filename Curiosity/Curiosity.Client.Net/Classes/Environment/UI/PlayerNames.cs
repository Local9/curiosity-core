﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.PlayerClasses;
using Curiosity.Client.net.Extensions;
using Curiosity.Global.Shared.Data;
using Curiosity.Shared.Client.net;
using Newtonsoft.Json;
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

        static internal List<PlayerSessionItem> playerSessionItems = new List<PlayerSessionItem>();

        static internal float namePlateDistance = 250f;
        static internal float namePlateVehicleDistance = 250f;

        static int GameTime;
        static int PlayerNameUpdate = (1000 * 10);

        static bool isSpectating = false;

        static public void Init()
        {
            GameTime = API.GetGameTimer();

            client.RegisterTickHandler(OnPlayerNamesTick);
            client.RegisterEventHandler("curioisty:UI:IsSpectating", new Action<bool>(OnIsSpectating));

            client.RegisterEventHandler("curioisty:client:player:name:update", new Action<string>(OnPlayerNameUpdate));
        }

        private static void OnPlayerNameUpdate(string json)
        {
            PlayerSessionItem playerSessionItem = JsonConvert.DeserializeObject<PlayerSessionItem>(json);

            if (playerSessionItem.Disconnected)
            {
                playerSessionItems.Remove(playerSessionItem);
                return;
            }

            playerSessionItems.Add(playerSessionItem);
            Log.Verbose($"Received player data for {playerSessionItem.Username}");

            int serverHandle = int.Parse(playerSessionItem.ServerId);
            
            int playerEntity = API.GetPlayerFromServerId(serverHandle);

            Player player = new Player(playerEntity);

            if (player == null) return;

            if (player.Character.Exists())
            {
                Log.Verbose($"Appended player data for {playerSessionItem.Username}");
                if (playerSessionItem.IsStaff)
                {
                    Decorators.Set(player.Character.Handle, Decorators.DECOR_PLAYER_STAFF, playerSessionItem.IsStaff);
                    return;
                }
            }
        }

        static void OnIsSpectating(bool isSpec)
        {
            if (!PlayerInformation.IsStaff()) return;

            isSpectating = isSpec;
        }

        static internal bool ShouldShowName(CitizenFX.Core.Player player)
        {
            if (isSpectating)
            {
                return true;
            }

            bool isCloseEnough;

            if (CinematicMode.DoHideHud) return false;

            if (!player.Character.IsVisible) return false;

            if (!API.IsEntityVisible(player.Character.Handle)) return false;

            if (!player.Character.IsInVehicle())
            {
                isCloseEnough = player.Character.IsInRangeOf(Game.PlayerPed.Position, namePlateDistance);
            }
            else
            {
                isCloseEnough = player.Character.IsInRangeOf(Game.PlayerPed.Position, namePlateVehicleDistance);
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
                if (!Client.ShowPlayerNames)
                {
                    if (Client.StaffShowPlayerNames)
                    {
                        // carry on
                    }
                    else
                    {
                        return;
                    }
                }

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

        static internal async Task ShowName(Player player)
        {
            if (!API.NetworkIsPlayerActive(player.Handle) && Game.Player.Handle == player.Handle) return;

            bool staffMember = Decorators.GetBoolean(player.Character.Handle, Decorators.DECOR_PLAYER_STAFF);
            string staffTag = staffMember ? "[STAFF]" : string.Empty;

            int gamerTagId = API.CreateMpGamerTag(player.Character.Handle, player.Name, false, staffMember, staffTag, staffMember ? 1 : 0);

            if (CinematicMode.DoHideHud)
            {
                API.SetMpGamerTagVisibility(gamerTagId, 0, false);
                return;
            }

            if (staffMember)
            {
                API.CreateMpGamerTagWithCrewColor(player.Character.Handle, player.Name, false, staffMember, staffTag, staffMember ? 1 : 0, 255, 215, 0);
            }

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
            else if (API.HasEntityClearLosToEntity(Game.PlayerPed.Handle, player.Character.Handle, 17) || isSpectating)
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
