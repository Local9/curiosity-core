﻿using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Systems.Library.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Extensions;
using CitizenFX.Core.UI;
using Curiosity.Systems.Library.Models;
using Curiosity.Core.Client.Events;

namespace Curiosity.Core.Client.Environment.Entities
{
    class WorldPlayer
    {
        PluginManager pluginManager => PluginManager.Instance;
        PlayerOptionsManager playerOptions => PlayerOptionsManager.GetModule();
        EventSystem eventSystem => EventSystem.GetModule();
        NotificationManager notificationManager => NotificationManager.GetModule();

        public Player Player;
        private Ped PlayerPed => Player.Character;

        public bool IsPassive;
        int isPassiveStateBagHandler = -1;

        public WorldPlayer(Player player)
        {
            Player = player;
            IsPassive = player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
            pluginManager.AttachTickHandler(OnPlayerPassive);
            pluginManager.AttachTickHandler(OnPlayerRevive);
            isPassiveStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_PASSIVE, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerPassiveChange));
        }

        public void Dispose()
        {
            pluginManager.DetachTickHandler(OnPlayerPassive);
            pluginManager.DetachTickHandler(OnPlayerRevive);
            RemoveStateBagChangeHandler(isPassiveStateBagHandler);

            bool playerInVehicle = PlayerPed.IsInVehicle();
            bool currentPlayerInVehicle = Game.PlayerPed.IsInVehicle();

            if (playerInVehicle)
            {
                PlayerPed.CurrentVehicle.ResetOpacity();
                PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed, true);
            }

            if (currentPlayerInVehicle)
            {
                Game.PlayerPed.CurrentVehicle.ResetOpacity();
                Game.PlayerPed.CurrentVehicle.SetNoCollision(PlayerPed, true);
            }

            if (playerInVehicle && currentPlayerInVehicle)
            {
                Game.PlayerPed.CurrentVehicle.SetNoCollision(PlayerPed.CurrentVehicle, true);
                PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, true);
            }
        }

        private void OnStatePlayerPassiveChange(string bag, string key, dynamic isPassive, int reserved, bool replicated)
        {
            IsPassive = isPassive;
        }

        private async Task OnPlayerPassive()
        {
            try
            {
                if (IsPassive || playerOptions.IsPassive)
                {
                    if (Vector3.Distance(Game.PlayerPed.Position, PlayerPed.Position) < 15f)
                    {
                        bool playerInVehicle = PlayerPed.IsInVehicle();
                        bool currentPlayerInVehicle = Game.PlayerPed.IsInVehicle();

                        if (playerInVehicle)
                        {
                            PlayerPed.CurrentVehicle.Opacity = 200;
                            PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed, false);
                        }

                        if (currentPlayerInVehicle)
                        {
                            Game.PlayerPed.CurrentVehicle.Opacity = 200;
                            Game.PlayerPed.CurrentVehicle.SetNoCollision(PlayerPed, false);
                        }

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
                Logger.Debug(ex, $"OnPlayerPassive");
            }
        }

        private async Task OnPlayerRevive()
        {
            if (!NetworkIsPlayerActive(Player.Handle)) goto WAIT_2500;
            if (!PlayerPed.Exists()) goto WAIT_2500;
            if (!PlayerPed.IsDead) goto WAIT_2500;
            if (Vector3.Distance(Game.PlayerPed.Position, PlayerPed.Position) > 2f) goto WAIT_2500;

            if (Game.PlayerPed.IsInVehicle())
            {
                Screen.DisplayHelpTextThisFrame($"Cannot be in a vehicle when trying to revive.");
                goto WAIT_ZERO;
            }

            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to attempt revive.");

            if (Game.IsControlJustPressed(0, Control.Context))
            {
                ExportMessage exportMessage = await eventSystem.Request<ExportMessage>("character:revive:other", Player.ServerId);

                if (!exportMessage.success)
                {
                    notificationManager.Error(exportMessage.error);
                }
                goto WAIT_2500;
            }
            goto WAIT_ZERO;

        WAIT_2500:
            await BaseScript.Delay(2500);
            return;

        WAIT_ZERO:
            await BaseScript.Delay(0);
            return;
        }
    }
}
