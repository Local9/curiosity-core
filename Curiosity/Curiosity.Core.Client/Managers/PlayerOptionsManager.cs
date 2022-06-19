using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using NativeUI;
using System;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class PlayerOptionsManager : Manager<PlayerOptionsManager>
    {
        DateTime passiveModeDisabled;
        DateTime passiveModeJobCooldown;
        public bool IsPassiveModeCooldownEnabled = false;
        public bool IsPassive = false;
        public bool IsWanted = false;
        DateTime playerKilledSelf;
        public bool IsKillSelfEnabled { get; internal set; } = true;
        public int CostOfKillSelf = 500;
        public int NumberOfTimesKillSelf = 0;
        public bool IsScubaGearEnabled = false;
        public bool WeaponsDisabled = false;
        public ePlayerJobs CurrentJob = ePlayerJobs.UNEMPLOYED;
        NotificationManager NotificationManager => NotificationManager.GetModule();

        int jobStateBagHandler = 0;
        int wantedStateBagHandler = 0;

        public override void Begin()
        {
            jobStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_JOB, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnPlayerJobStateChange));
            wantedStateBagHandler = AddStateBagChangeHandler(StateBagKey.PLAYER_POLICE_WANTED, $"player:{Game.Player.ServerId}", new Action<string, string, dynamic, int, bool>(OnPlayerWantedStateChange));

            Instance.EventRegistry.Add("npwd:PhotoModeStarted", new Action(() =>
            {
                DisplayRadar(false);
            }));

            Instance.EventRegistry.Add("npwd:PhotoModeEnded", new Action(() =>
            {
                DisplayRadar(true);
            }));
        }

        private void OnPlayerWantedStateChange(string bag, string key, dynamic wanted, int reserved, bool replicated)
        {
            if (wanted == true)
                Interface.Notify.Alert($"You're wanted by police.");

            IsWanted = wanted;
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

            bool isWantedByPolice = Game.Player.State.Get(StateBagKey.PLAYER_POLICE_WANTED) ?? false;

            if (isWantedByPolice)
            {
                Notify.Error($"Cannot spawn/change a vehicle when wanted.");
                return;
            }

            if (Game.Player.WantedLevel > 0)
            {
                Interface.Notify.Info($"Passive Mode: Cannot enable when wanted.");
                return;
            }

            if (!isPassive)
            {
                passiveModeDisabled = DateTime.UtcNow.AddSeconds(30);
                IsPassiveModeCooldownEnabled = true;
                Instance.AttachTickHandler(PassiveCooldownTick);
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

                if (!IsPassive)
                {
                    Cache.PlayerPed.CanBeDraggedOutOfVehicle = true;
                    API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 1f);
                    API.NetworkSetFriendlyFireOption(true);
                    API.SetMaxWantedLevel(5);
                    Interface.Notify.Info($"Passive Mode Disabled");
                    Logger.Debug($"Passive Mode Disabled");
                }

                IsPassiveModeCooldownEnabled = false;
                Interface.Notify.Info($"Passive Mode can now be changed.");
            }
            else
            {
                const int interval = 45;

                DateTime finalDate = passiveModeDisabled;
                string timeSpanLeft = (finalDate - DateTime.UtcNow).ToString(@"mm\:ss");

                TextTimerBar textTimerBar = new TextTimerBar("Disabling Passive Mode", timeSpanLeft);
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

        [TickHandler(SessionWait = true)]
        private async Task OnDisablePlayerWeapons()
        {
            if (!WeaponsDisabled)
            {
                await BaseScript.Delay(500);
                return;
            }

            DisablePlayerFiring(Game.Player.Handle, WeaponsDisabled);
            DisableControlAction(0, 140, WeaponsDisabled);
            DisableControlAction(2, 140, WeaponsDisabled);
            DisableControlAction(0, 22, WeaponsDisabled);
            DisableControlAction(2, 22, WeaponsDisabled);
            DisableControlAction(0, 24, WeaponsDisabled);
            DisableControlAction(2, 24, WeaponsDisabled);
            DisableControlAction(0, 25, WeaponsDisabled);
            DisableControlAction(2, 25, WeaponsDisabled);
            DisableControlAction(0, (int)Control.SelectWeapon, WeaponsDisabled);
            DisableControlAction(2, (int)Control.SelectWeapon, WeaponsDisabled);
            DisableControlAction(0, (int)Control.Cover, WeaponsDisabled);
            DisableControlAction(2, (int)Control.Cover, WeaponsDisabled);
            DisableControlAction(0, 257, WeaponsDisabled);
            DisableControlAction(2, 257, WeaponsDisabled);
            DisableControlAction(0, 263, WeaponsDisabled);
            DisableControlAction(2, 263, WeaponsDisabled);
            DisableControlAction(0, 264, WeaponsDisabled);
            DisableControlAction(2, 264, WeaponsDisabled);
            DisableControlAction(0, (int)Control.Jump, WeaponsDisabled);
            DisableControlAction(2, (int)Control.Jump, WeaponsDisabled);
            DisableControlAction(0, (int)Control.Duck, WeaponsDisabled);
            DisableControlAction(2, (int)Control.Duck, WeaponsDisabled);
            API.BlockWeaponWheelThisFrame();

            if (Cache.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed)
                Cache.PlayerPed.Weapons.Select(WeaponHash.Unarmed, true);
        }

        public void DisableWeapons(bool state)
        {
            WeaponsDisabled = state;
        }
    }
}
