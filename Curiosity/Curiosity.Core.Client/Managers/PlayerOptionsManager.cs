using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using NativeUI;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class PlayerOptionsManager : Manager<PlayerOptionsManager>
    {
        DateTime passiveModeDisabled;
        public bool IsPassiveModeEnabledCooldown = false;
        public bool IsPassive = false;
        DateTime playerKilledSelf;
        public bool IsKillSelfEnabled { get; internal set; } = true;
        public int CostOfKillSelf = 500;
        public int NumberOfTimesKillSelf = 0;
        public bool IsScubaGearEnabled = false;
        public ePlayerJobs CurrentJob = ePlayerJobs.UNEMPLOYED;
        NotificationManager NotificationManager => NotificationManager.GetModule();

        int jobStateBagHandler = 0;

        public override void Begin()
        {
            jobStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_JOB, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnPlayerJobStateChange));
        }

        private void OnPlayerJobStateChange(string bag, string key, dynamic jobId, int reserved, bool replicated)
        {
            Logger.Debug($"CurrentJob: {CurrentJob}");
            CurrentJob = (ePlayerJobs)jobId;
        }

        public void ToggleScubaEquipment()
        {
            IsScubaGearEnabled = !IsScubaGearEnabled;

            SetEnableScuba(Cache.PlayerPed.Handle, IsScubaGearEnabled);
            Cache.PlayerPed.SetConfigFlag((int)ePedConfigFlags.CPED_CONFIG_FLAG_IsScuba, IsScubaGearEnabled);
            Cache.PlayerPed.DrownsInWater = !IsScubaGearEnabled;

            // SetEnableScubaGearLight(entity.Id, scubaEnabled); // this is a light attachment

            if (!IsScubaGearEnabled)
            {
                NotificationManager.Info($"Scuba Equipment Removed");
                ClearPedScubaGearVariation(Cache.PlayerPed.Handle);
                return;
            }
            NotificationManager.Info($"Scuba Equipment Applied");
            SetPedScubaGearVariation(Cache.PlayerPed.Handle);
        }

        public async void SetPlayerPassiveOnStart(bool isPassive)
        {
            await Session.Loading();

            API.ClearPlayerWantedLevel(Game.Player.Handle);

            Game.PlayerPed.IsInvincible = false;

            if (!isPassive)
            {
                Cache.PlayerPed.CanBeDraggedOutOfVehicle = true;
                API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 1f);
                API.NetworkSetFriendlyFireOption(true);
                API.SetMaxWantedLevel(5);
                Logger.Debug($"Passive Mode Disabled");
            }

            if (isPassive)
            {
                Cache.PlayerPed.CanBeDraggedOutOfVehicle = false;
                API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 0.5f);
                API.NetworkSetFriendlyFireOption(false);
                API.SetMaxWantedLevel(0);
                Logger.Debug($"Passive Mode Enabled");
            }

            Game.Player.State.Set(StateBagKey.PLAYER_PASSIVE, isPassive, true);
            Cache.Character.IsPassive = isPassive;
            IsPassive = isPassive;
        }

        public async void TogglePlayerPassive(bool isPassive)
        {
            await Session.Loading();

            API.ClearPlayerWantedLevel(Game.Player.Handle);

            Game.PlayerPed.IsInvincible = false;

            if (Game.Player.WantedLevel > 0)
            {
                Interface.Notify.Info($"Passive Mode: Cannot enable when wanted.");
                return;
            }

            if (!isPassive)
            {
                Cache.PlayerPed.CanBeDraggedOutOfVehicle = true;
                API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 1f);
                API.NetworkSetFriendlyFireOption(true);

                passiveModeDisabled = DateTime.UtcNow.AddMinutes(2);
                Instance.AttachTickHandler(PassiveCooldownTick);
                API.SetMaxWantedLevel(5);
                IsPassiveModeEnabledCooldown = true;

                API.SetMaxWantedLevel(5);

                Logger.Debug($"Passive Mode Disabled");
            }

            if (isPassive)
            {
                Cache.PlayerPed.CanBeDraggedOutOfVehicle = false;
                API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 0.5f);
                API.NetworkSetFriendlyFireOption(false);
                API.SetMaxWantedLevel(0);
                Logger.Debug($"Passive Mode Enabled");
            }

            NetworkSetPlayerIsPassive(isPassive);
            SetLocalPlayerAsGhost(isPassive);

            Game.Player.State.Set(StateBagKey.PLAYER_PASSIVE, isPassive, true);
            Cache.Character.IsPassive = isPassive;
            IsPassive = isPassive;
        }

        public async Task PassiveCooldownTick()
        {
            if (passiveModeDisabled < DateTime.UtcNow)
            {
                Instance.DetachTickHandler(PassiveCooldownTick);
                IsPassiveModeEnabledCooldown = false;
                Interface.Notify.Info($"Passive Mode can now be changed.");
            }
            else
            {
                const int interval = 45;

                DateTime finalDate = passiveModeDisabled;
                string timeSpanLeft = (finalDate - DateTime.UtcNow).ToString(@"mm\:ss");

                TextTimerBar textTimerBar = new TextTimerBar("Passive Mode Cooldown", timeSpanLeft);
                textTimerBar.Draw(interval);

                Screen.Hud.HideComponentThisFrame(HudComponent.AreaName);
                Screen.Hud.HideComponentThisFrame(HudComponent.StreetName);
                Screen.Hud.HideComponentThisFrame(HudComponent.VehicleName);
            }
        }

        public async void KillSelf()
        {
            int randomEvent = Utility.RANDOM.Next(3);

            Cache.PlayerPed.IsInvincible = false; // Well, you gotta die!

            //if (randomEvent == 1)
            if (randomEvent == 1)
            {
                Cache.PlayerPed.Task.PlayAnimation("mp_suicide", "pill", 8f, -1, AnimationFlags.None);
                await BaseScript.Delay(2500);
                Cache.PlayerPed.Kill();
            }
            else if (randomEvent == 0)
            {
                Cache.PlayerPed.Weapons.Give((WeaponHash)453432689, 1, true, true);
                Cache.PlayerPed.Task.PlayAnimation("mp_suicide", "pistol", 8f, -1, AnimationFlags.None);
                await BaseScript.Delay(1000);
                Function.Call((Hash)7592965275345899078, Cache.PlayerPed.Handle, 0, 0, 0, false);
                Cache.PlayerPed.Kill();
            }
            else
            {
                Function.Call(Hash.GIVE_WEAPON_TO_PED, Cache.PlayerPed.Handle, -1569615261, 1, true, true);
                Model plasticCup = new Model("apa_prop_cs_plastic_cup_01");
                await plasticCup.Request(10000);

                Prop prop = await World.CreateProp(plasticCup, Cache.PlayerPed.Position, false, false);

                int boneIdx = API.GetPedBoneIndex(Cache.PlayerPed.Handle, 28422);
                API.AttachEntityToEntity(prop.Handle, Cache.PlayerPed.Handle, API.GetPedBoneIndex(API.GetPlayerPed(-1), 58868), 0.0f, 0.0f, 0.0f, 5.0f, 0.0f, 70.0f, true, true, false, false, 2, true);

                Cache.PlayerPed.Task.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8f, -1, AnimationFlags.None);

                await BaseScript.Delay(1500);
                Cache.PlayerPed.Kill();
                prop.Detach();
            }

            playerKilledSelf = DateTime.Now;
            NumberOfTimesKillSelf++;
            PluginManager.Instance.AttachTickHandler(PlayerKilledSelfCooldownTick);

            EventSystem.GetModule().Send("character:killed:self");
        }

        public async Task PlayerKilledSelfCooldownTick()
        {
            if (DateTime.Now.Subtract(playerKilledSelf).TotalMinutes >= 5)
            {
                Instance.DetachTickHandler(PlayerKilledSelfCooldownTick);
                IsKillSelfEnabled = true;
            }
            else
            {
                IsKillSelfEnabled = false;
            }

            await BaseScript.Delay(1500);
        }

        bool disableWeapons = false;

        [TickHandler(SessionWait = true)]
        private async Task OnDisablePlayerWeapons()
        {
            if (!disableWeapons)
            {
                await BaseScript.Delay(500);
                return;
            }

            DisablePlayerFiring(Game.Player.Handle, disableWeapons);
            DisableControlAction(0, 22, disableWeapons);
            DisableControlAction(2, 22, disableWeapons);
            DisableControlAction(0, 24, disableWeapons);
            DisableControlAction(2, 24, disableWeapons);
            DisableControlAction(0, 25, disableWeapons);
            DisableControlAction(2, 25, disableWeapons);
            DisableControlAction(0, (int)Control.SelectWeapon, disableWeapons);
            DisableControlAction(2, (int)Control.SelectWeapon, disableWeapons);
            DisableControlAction(0, (int)Control.Cover, disableWeapons);
            DisableControlAction(2, (int)Control.Cover, disableWeapons);
            DisableControlAction(0, 257, disableWeapons);
            DisableControlAction(2, 257, disableWeapons);
            DisableControlAction(0, 263, disableWeapons);
            DisableControlAction(2, 263, disableWeapons);
            DisableControlAction(0, 264, disableWeapons);
            DisableControlAction(2, 264, disableWeapons);
            DisableControlAction(0, (int)Control.Jump, disableWeapons);
            DisableControlAction(2, (int)Control.Jump, disableWeapons);
            DisableControlAction(0, (int)Control.Duck, disableWeapons);
            DisableControlAction(2, (int)Control.Duck, disableWeapons);
            API.BlockWeaponWheelThisFrame();

            if (Cache.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed)
                Cache.PlayerPed.Weapons.Select(WeaponHash.Unarmed, true);
        }

        public void DisableWeapons(bool state)
        {
            disableWeapons = state;
        }
    }
}
