using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Core.Client.Managers;

namespace Curiosity.Core.Client.Environment.Entities
{
    class WorldPlayer
    {
        PluginManager PluginManager => PluginManager.Instance;
        PlayerOptionsManager PlayerOptions => PlayerOptionsManager.GetModule();

        public int GameTagHandle;
        public Player Player;
        private Ped PlayerPed => Player.Character;
        public bool IsPassive;
        int isPassiveStateBagHandler = -1;

        public WorldPlayer(Player player)
        {
            Player = player;
            IsPassive = player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
            PluginManager.AttachTickHandler(OnPlayerPassive);
            isPassiveStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_PASSIVE, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerPassiveChange));
        }

        public void Dispose()
        {
            PluginManager.DetachTickHandler(OnPlayerPassive);
            RemoveStateBagChangeHandler(isPassiveStateBagHandler);
        }

        private void OnStatePlayerPassiveChange(string bag, string key, dynamic isPassive, int reserved, bool replicated)
        {
            IsPassive = isPassive;
        }

        private async Task OnPlayerPassive()
        {
            try
            {
                if (IsPassive || PlayerOptions.IsPassive)
                {
                    if (Game.PlayerPed.IsInRangeOf(PlayerPed.Position, 35))
                    {
                        bool playerInVehicle = PlayerPed.IsInVehicle();
                        bool currentPlayerInVehicle = Game.PlayerPed.IsInVehicle();

                        if (playerInVehicle)
                            PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed, false);

                        if (currentPlayerInVehicle)
                            Game.PlayerPed.CurrentVehicle.SetNoCollision(PlayerPed, false);

                        if (playerInVehicle && currentPlayerInVehicle)
                        {
                            Game.PlayerPed.CurrentVehicle.SetNoCollision(PlayerPed.CurrentVehicle, false);
                            PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // just don't crash
            }
        }
    }
}
