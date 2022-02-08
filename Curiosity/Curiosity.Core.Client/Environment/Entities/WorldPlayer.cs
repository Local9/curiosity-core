using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Environment.Entities
{
    public class WorldPlayer
    {
        PluginManager pluginManager => PluginManager.Instance;
        PlayerOptionsManager playerOptions => PlayerOptionsManager.GetModule();
        EventSystem eventSystem => EventSystem.GetModule();
        NotificationManager notificationManager => NotificationManager.GetModule();

        public Player Player;
        public string Name;
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

        public int WantedLevel = 0;
        int wantedLevelStateBagHandler = -1;

        public int GroupId;
        int groupStateBagHandler = -1;
        bool _sameGroup;

        public int ClientGroupId;
        int clientGroupStateBagHandler = -1;

        public int PlayerJob;
        int playerJobStateBagHandler = -1;
        public bool IsOfficer => PlayerJob == (int)ePlayerJobs.POLICE_OFFICER;

        public WorldPlayer(Player player)
        {
            Player = player;
            Name = player.Name;
            PlayerHandle = player.Handle;
            PedHandle = player.Character.Handle;
            IsPassive = player.State.Get(StateBagKey.PLAYER_PASSIVE) ?? false;
            IsWanted = player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;
            GroupId = player.State.Get(StateBagKey.PLAYER_GROUP) ?? -1;
            PlayerJob = player.State.Get(StateBagKey.PLAYER_JOB) ?? -1;
            int myGroupId = Game.Player.State.Get(StateBagKey.PLAYER_GROUP) ?? -1;

            if (myGroupId > -1 && GroupId > -1)
            {
                _sameGroup = myGroupId == GroupId;
            }

            pluginManager.AttachTickHandler(OnPlayerChanges);
            pluginManager.AttachTickHandler(OnPlayerRevive);

            passiveStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_PASSIVE, $"player:{Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerPassiveChange));
            wantedStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_POLICE_WANTED, $"player:{Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerWantedChange));
            wantedLevelStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_WANTED_LEVEL, $"player:{Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerWantedLevelChange));
            groupStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_GROUP, $"player:{Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStatePlayerGroupChange));
            clientGroupStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_GROUP, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStateClientPlayerGroupChange));
            playerJobStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_JOB, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnStateClientPlayerJobChange));

            if (player.Character.AttachedBlip is null)
            {
                _blip = player.Character.AttachBlip();
                _blipHandle = _blip.Handle;
                Utilities.SetCorrectBlipSprite(PedHandle, _blipHandle, IsWanted, _sameGroup, IsOfficer);
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
                RemoveStateBagChangeHandler(wantedLevelStateBagHandler);
                RemoveStateBagChangeHandler(groupStateBagHandler);
                RemoveStateBagChangeHandler(clientGroupStateBagHandler);
                RemoveStateBagChangeHandler(playerJobStateBagHandler);

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
            catch (Exception ex)
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
            IsPassive = isPassive;
        }

        private void OnStatePlayerWantedChange(string bag, string key, dynamic isWanted, int reserved, bool replicated)
        {
            IsWanted = isWanted;
        }

        private void OnStatePlayerWantedLevelChange(string bag, string key, dynamic level, int reserved, bool replicated)
        {
            WantedLevel = level;
        }

        private void OnStatePlayerGroupChange(string bag, string key, dynamic groupId, int reserved, bool replicated)
        {
            GroupId = groupId;
        }

        private void OnStateClientPlayerGroupChange(string bag, string key, dynamic groupId, int reserved, bool replicated)
        {
            ClientGroupId = groupId;
        }

        private void OnStateClientPlayerJobChange(string bag, string key, dynamic job, int reserved, bool replicated)
        {
            PlayerJob = job;
        }

        // This is mainly for things that update, blips, passive, etc
        private async Task OnPlayerChanges()
        {
            try
            {
                _sameGroup = GroupId == ClientGroupId;
                Utilities.SetCorrectBlipSprite(PedHandle, _blipHandle, IsWanted, _sameGroup, IsOfficer);
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
            if (IsWanted && !wasWanted && WantedLevel >= 3)
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

        private void UpdatePlayerCollisionStates() // need to change this to work differently
        {
            // Updated based on : https://github.com/justalemon/SimplePassive

            Vehicle gamePlayerVehicle = Game.PlayerPed.CurrentVehicle;
            Vehicle gamePlayerVehicleHooked = gamePlayerVehicle?.GetHookedVehicle();

            Vehicle otherVehicle = PlayerPed.CurrentVehicle;
            Vehicle otherHookedVehicle = otherVehicle?.GetHookedVehicle();

            bool disableCollision = IsPassive || playerOptions.IsPassive;

            int alpha = disableCollision && !GetIsTaskActive(PlayerPed.Handle, (int)eTaskTypeIndex.CTaskExitVehicle) && gamePlayerVehicle != otherVehicle ? 180 : 255;
            // PlayerPed.SetAlpha(alpha);
            otherVehicle?.SetAlpha(alpha);
            otherHookedVehicle?.SetAlpha(alpha);

            if (disableCollision)
            {
                if (otherVehicle != null &&
                        IsPedInVehicle(otherVehicle.Handle, PlayerPed.Handle, false) &&
                        otherVehicle.GetPedOnSeat(VehicleSeat.Driver) != PlayerPed)
                {
                    // do nothing
                }
                else
                {
                    // Local Player vs Other Player
                    GamePlayerPed.DisableCollisionsThisFrame(PlayerPed);
                    // Local Player vs Other Vehicle (if present)
                    GamePlayerPed.DisableCollisionsThisFrame(otherVehicle);
                    // Local Player vs Other Hooked (if present)
                    GamePlayerPed.DisableCollisionsThisFrame(otherHookedVehicle);

                    // Local Vehicle vs Other Player
                    gamePlayerVehicle?.DisableCollisionsThisFrame(PlayerPed);
                    // Local Vehicle vs Other Vehicle (if present)
                    gamePlayerVehicle?.DisableCollisionsThisFrame(otherVehicle);
                    // Local Vehicle vs Other Hooked (if present)
                    gamePlayerVehicle?.DisableCollisionsThisFrame(otherHookedVehicle);

                    // Local Hooked vs Other Player
                    gamePlayerVehicleHooked?.DisableCollisionsThisFrame(PlayerPed);
                    // Local Hooked vs Other Vehicle (if present)
                    gamePlayerVehicleHooked?.DisableCollisionsThisFrame(otherVehicle);
                    // Local Hooked vs Other Hooked (if present)
                    gamePlayerVehicleHooked?.DisableCollisionsThisFrame(otherHookedVehicle);


                    // Other Player vs Local Player
                    PlayerPed.DisableCollisionsThisFrame(GamePlayerPed);
                    // Other Player vs Local Vehicle (if present)
                    PlayerPed.DisableCollisionsThisFrame(gamePlayerVehicle);
                    // Other Player vs Local Hooked (if present)
                    PlayerPed.DisableCollisionsThisFrame(gamePlayerVehicleHooked);
                    // Disable cam collision for other ped
                    DisableCamCollisionForEntity(PlayerPed.Handle);

                    // Other Vehicle vs Local Player
                    otherVehicle?.DisableCollisionsThisFrame(GamePlayerPed);
                    // Other Vehicle vs Local Vehicle (if present)
                    otherVehicle?.DisableCollisionsThisFrame(gamePlayerVehicle);
                    // Other Vehicle vs Local Hooked (if present)
                    otherVehicle?.DisableCollisionsThisFrame(gamePlayerVehicleHooked);
                    // Disable cam collision for other vehicle
                    if (otherVehicle != null)
                        DisableCamCollisionForEntity(otherVehicle.Handle);

                    // Other Hooked vs Local Player
                    otherHookedVehicle?.DisableCollisionsThisFrame(GamePlayerPed);
                    // Other Hooked vs Local Vehicle (if present)
                    otherHookedVehicle?.DisableCollisionsThisFrame(gamePlayerVehicle);
                    // Other Hooked vs Local Hooked (if present)
                    otherHookedVehicle?.DisableCollisionsThisFrame(gamePlayerVehicleHooked);
                    // Disable cam collision for other vehicle trailer if hooked
                    if (otherHookedVehicle != null)
                        DisableCamCollisionForEntity(otherHookedVehicle.Handle);
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
