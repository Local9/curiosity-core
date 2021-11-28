using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Data;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Environment.Entities
{
    class WorldPlayer
    {
        PluginManager pluginManager => PluginManager.Instance;
        PlayerOptionsManager playerOptions => PlayerOptionsManager.GetModule();
        EventSystem eventSystem => EventSystem.GetModule();
        NotificationManager notificationManager => NotificationManager.GetModule();

        public Player Player;
        private Player GamePlayer => Game.Player;
        private Ped PlayerPed => Player.Character;
        private Ped GamePlayerPed => GamePlayer.Character;
        public int PedHandle;
        public int PlayerHandle;
        public bool IsReady;

        private Blip _blip;
        private int _blipHandle;

        public bool Exists => DoesEntityExist(PedHandle);
        public Vector3 Position => GetEntityCoords(PedHandle, false);

        public bool IsPassive;
        int passiveStateBagHandler = -1;

        public bool IsWanted;
        int wantedStateBagHandler = -1;

        public WorldPlayer(Player player)
        {
            Player = player;
            PlayerHandle = player.Handle;
            PedHandle = player.Character.Handle;
            IsPassive = player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
            IsWanted = player.State.Get(StateBagKey.PLAYER_IS_WANTED) ?? false;
            pluginManager.AttachTickHandler(OnPlayerChanges);
            pluginManager.AttachTickHandler(OnPlayerRevive);
            passiveStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_PASSIVE, $"player:{Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerPassiveChange));
            wantedStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_IS_WANTED, $"player:{Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerWantedChange));

            if (player.Character.AttachedBlip is null)
            {
                _blip = player.Character.AttachBlip();
                _blipHandle = _blip.Handle;
                Utilities.SetCorrectBlipSprite(PedHandle, _blipHandle, IsWanted);
                SetBlipCategory(_blipHandle, 7);
                SetBlipPriority(_blipHandle, 11);
                SetBlipNameToPlayerName(_blipHandle, player.Handle);

                UpdateBlipString();
            }

            Logger.Debug($"Player '{Player.Name}' Created");
            IsReady = true;
        }

        public void Dispose()
        {
            try
            {
                pluginManager.DetachTickHandler(OnPlayerChanges);
                pluginManager.DetachTickHandler(OnPlayerRevive);
                RemoveStateBagChangeHandler(passiveStateBagHandler);
                RemoveStateBagChangeHandler(wantedStateBagHandler);

                bool playerInVehicle = PlayerPed.IsInVehicle();
                bool currentPlayerInVehicle = Game.PlayerPed.IsInVehicle();

                if (playerInVehicle)
                {
                    PlayerPed.CurrentVehicle.ResetOpacity();
                    PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed, true);
                }

                if (currentPlayerInVehicle)
                {
                    GamePlayerPed.CurrentVehicle.ResetOpacity();
                    GamePlayerPed.CurrentVehicle.SetNoCollision(PlayerPed, true);
                }

                if (playerInVehicle && currentPlayerInVehicle)
                {
                    GamePlayerPed.CurrentVehicle.SetNoCollision(PlayerPed.CurrentVehicle, true);
                    PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, true);
                }

                Logger.Debug($"Player '{Player.Name}' Disposed");
            }
            catch(Exception ex)
            {
                Logger.Debug(ex, $"Dispose");
            }
        }

        public void UpdateBlipString()
        {
            string key = $"PB_{Player.Name}";
            AddTextEntry(key, Player.Name);

            BeginTextCommandSetBlipName(key);
            AddTextComponentSubstringPlayerName(key);
            EndTextCommandSetBlipName(_blipHandle);
        }

        private void OnStatePlayerPassiveChange(string bag, string key, dynamic isPassive, int reserved, bool replicated)
        {
            Logger.Debug($"bag: {bag}, key: {key}, isPassive: {isPassive}, replicated: {replicated}");
            IsPassive = isPassive;
        }

        private void OnStatePlayerWantedChange(string bag, string key, dynamic isWanted, int reserved, bool replicated)
        {
            Logger.Debug($"bag: {bag}, key: {key}, isPassive: {isWanted}, replicated: {replicated}");
            IsWanted = isWanted;
        }

        // This is mainly for things that update, blips, passive, etc
        private async Task OnPlayerChanges()
        {
            try
            {
                Utilities.SetCorrectBlipSprite(PedHandle, _blipHandle, IsWanted);
                UpdateBlipString();
                UpdatePlayerCollisionStates();
                UpdatePlayerWantedState();
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"OnPlayerPassive");
            }
        }

        bool wasWanted = false;
        private void UpdatePlayerWantedState()
        {
            if (IsWanted && !wasWanted)
            {
                wasWanted = true;
                Blip playerBlip = PlayerPed.AttachedBlip;
                if (playerBlip != null)
                {
                    playerBlip.Color = BlipColor.Red;
                }
            }

            if (!IsWanted && wasWanted)
            {
                wasWanted = false;
                Blip playerBlip = PlayerPed.AttachedBlip;
                if (playerBlip != null)
                {
                    playerBlip.Color = BlipColor.White;
                }
            }
        }

        private void UpdatePlayerCollisionStates()
        {
            bool playerInVehicle = PlayerPed.IsInVehicle();
            bool currentPlayerInVehicle = Game.PlayerPed.IsInVehicle();

            if (IsPassive || playerOptions.IsPassive)
            {
                if (Vector3.Distance(Game.PlayerPed.Position, PlayerPed.Position) < 15f)
                {
                    if (playerInVehicle)
                    {
                        PlayerPed.CurrentVehicle.Opacity = 200;
                        PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed, false);
                    }

                    if (currentPlayerInVehicle)
                    {
                        GamePlayerPed.CurrentVehicle.Opacity = 200;
                        GamePlayerPed.CurrentVehicle.SetNoCollision(PlayerPed, false);
                    }

                    if (playerInVehicle && currentPlayerInVehicle)
                    {
                        GamePlayerPed.CurrentVehicle.SetNoCollision(PlayerPed.CurrentVehicle, false);
                        PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, false);
                    }
                }
                else
                {
                    if (playerInVehicle)
                    {
                        PlayerPed.CurrentVehicle.ResetOpacity();
                        PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed, true);
                    }

                    if (currentPlayerInVehicle)
                    {
                        GamePlayerPed.CurrentVehicle.ResetOpacity();
                        GamePlayerPed.CurrentVehicle.SetNoCollision(PlayerPed, true);
                    }

                    if (playerInVehicle && currentPlayerInVehicle)
                    {
                        GamePlayerPed.CurrentVehicle.SetNoCollision(PlayerPed.CurrentVehicle, true);
                        PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed.CurrentVehicle, true);
                    }
                }
            }
            else
            {
                if (playerInVehicle && PlayerPed.CurrentVehicle.Opacity < 230)
                {
                    PlayerPed.CurrentVehicle.ResetOpacity();
                    PlayerPed.CurrentVehicle.SetNoCollision(Game.PlayerPed, true);
                }

                if (currentPlayerInVehicle && GamePlayerPed.CurrentVehicle.Opacity < 230)
                {
                    GamePlayerPed.CurrentVehicle.ResetOpacity();
                    GamePlayerPed.CurrentVehicle.SetNoCollision(PlayerPed, true);
                }
            }
        }

        // Revive is kept seperate
        private async Task OnPlayerRevive()
        {
            try
            {
                if (!NetworkIsPlayerActive(PlayerHandle)) goto WAIT_2500;
                if (!DoesEntityExist(PedHandle)) goto WAIT_2500;
                if (!IsEntityDead(PedHandle)) goto WAIT_2500;
                if (Vector3.Distance(GamePlayerPed.Position, PlayerPed.Position) > 2f) goto WAIT_2500;

                if (GamePlayerPed.IsInVehicle())
                {
                    Screen.DisplayHelpTextThisFrame($"Cannot be in a vehicle when trying to revive.");
                    goto WAIT_ZERO;
                }

                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to attempt revive.");

                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    Screen.DisplayHelpTextThisFrame($"Attempting to revive player.");
                    ExportMessage exportMessage = await eventSystem.Request<ExportMessage>("character:revive:other", Player.ServerId);

                    if (!exportMessage.success)
                    {
                        notificationManager.Error(exportMessage.error);
                    }

                    if (exportMessage.success)
                    {
                        Screen.DisplayHelpTextThisFrame($"Player has been revived.");
                    }
                    await BaseScript.Delay(5000);
                }
                goto WAIT_ZERO;

            WAIT_2500:
                await BaseScript.Delay(2500);

            WAIT_ZERO:
                await BaseScript.Delay(0);
            }
            catch (Exception ex)
            {
                Logger.Debug(ex, $"OnPlayerRevive");
            }
        }
    }
}
