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
        PlayerOptionsManager PlayerOptionsManager => PlayerOptionsManager.GetModule();
        JobManager _JobManager => JobManager.GetModule();

        public Dictionary<int, WorldPlayer> WorldPlayers = new Dictionary<int, WorldPlayer>();
        const string COMMAND_ARREST = "lv_police_arrest";

        public override void Begin()
        {
            RegisterKeyMapping(COMMAND_ARREST, "POLICE: Arrest Player", "keyboard", "");
            RegisterCommand(COMMAND_ARREST, new Action(OnArrestNearestPlayer), false);
        }

        private async void OnArrestNearestPlayer()
        {
            if (!_JobManager.IsOfficer)
            {
                return;
            }

            if (Game.PlayerPed.IsInVehicle())
            {
                Interface.Notify.Alert($"You must exit the vehicle to arrest the player.");
                return;
            }

            Dictionary<int, WorldPlayer> players = new Dictionary<int, WorldPlayer>(WorldPlayers);

            foreach (KeyValuePair<int, WorldPlayer> kvp in players)
            {
                WorldPlayer worldPlayer = kvp.Value;
                Player player = kvp.Value.Player;

                if (player == Game.Player) continue; // ignore self
                if (!Game.PlayerPed.IsInRangeOf(player.Character.Position, 10f)) continue;
                if (player.Character.IsInVehicle()) continue; // they must be outside a vehicle
                if (!worldPlayer.IsWanted) continue;

                bool hasLos = HasEntityClearLosToEntity(Game.PlayerPed.Handle, player.Character.Handle, 17);
                if (!hasLos) continue;

                bool res = await EventSystem.Request<bool>("police:suspect:jailed", player.ServerId);
                if (res)
                    Interface.Notify.Success($"{worldPlayer.Name} Jailed.");
            }
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
