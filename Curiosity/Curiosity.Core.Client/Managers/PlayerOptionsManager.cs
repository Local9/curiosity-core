using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using NativeUI;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class PlayerOptionsManager : Manager<PlayerOptionsManager>
    {
        DateTime passiveModeDisabled;
        public bool IsPassiveModeEnabled = false;
        public bool IsPassiveModeEnabledCooldown = false;
        DateTime playerKilledSelf;
        public bool IsKillSelfEnabled { get; internal set; } = true;
        public int CostOfKillSelf = 500;
        public int NumberOfTimesKillSelf = 0;

        public override void Begin()
        {
            SetPlayerPassive(true);
        }

        public async void SetPlayerPassive(bool isPassive)
        {
            await Session.Loading();

            if (IsPassiveModeEnabled == isPassive) return;

            Game.PlayerPed.IsInvincible = false;

            if (Game.Player.WantedLevel > 0)
            {
                Notify.Info($"Passive Mode: Cannot enable when wanted.");
                return;
            }

            if (IsPassiveModeEnabled != isPassive)
                IsPassiveModeEnabled = isPassive;

            if (!isPassive)
            {
                Cache.PlayerPed.CanBeDraggedOutOfVehicle = true;
                //Cache.PlayerPed.SetConfigFlag(342, false);
                //Cache.PlayerPed.SetConfigFlag(122, false);
                API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 1f);
                //Cache.PlayerPed.CanSwitchWeapons = true;

                // API.NetworkSetPlayerIsPassive(false);
                API.NetworkSetFriendlyFireOption(true);

                passiveModeDisabled = DateTime.UtcNow.AddMinutes(5);
                Instance.AttachTickHandler(PassiveCooldownTick);
                // Instance.DetachTickHandler(DisableWantedStates);
                ResetWantedStates();
                IsPassiveModeEnabledCooldown = true;

                API.SetMaxWantedLevel(5);

                Logger.Debug($"Passive Mode Disabled");
            }
                
            if (isPassive)
            {
                Cache.PlayerPed.CanBeDraggedOutOfVehicle = false;
                //Cache.PlayerPed.SetConfigFlag(342, true);
                //Cache.PlayerPed.SetConfigFlag(122, true);
                API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 0.5f);
                //Cache.PlayerPed.CanSwitchWeapons = false;

                // API.NetworkSetPlayerIsPassive(true);
                API.NetworkSetFriendlyFireOption(false);
                // Instance.AttachTickHandler(DisableWantedStates);
                DisableWantedStates();
                Logger.Debug($"Passive Mode Enabled");
                // Cache.PlayerPed.Weapons.Select(WeaponHash.Unarmed);                   
            }

            Game.PlayerPed.State.Set(StateBagKey.PLAYER_PASSIVE, isPassive, true);

            Logger.Debug($"PassiveMode: {IsPassiveModeEnabled}, Cooldown: {IsPassiveModeEnabledCooldown}");
        }

        public void DisableWantedStates()
        {
            API.ClearPlayerWantedLevel(Game.Player.Handle);
            API.SetMaxWantedLevel(0);
        }

        public void ResetWantedStates()
        {
            API.ClearPlayerWantedLevel(Game.Player.Handle);
            API.SetMaxWantedLevel(5);
        }

        public async Task PassiveCooldownTick()
        {
            if (passiveModeDisabled < DateTime.UtcNow)
            {
                Instance.DetachTickHandler(PassiveCooldownTick);
                IsPassiveModeEnabledCooldown = false;
                Notify.Info($"Passive Mode can now be changed.");
            }
            else
            {
                SizeF res = ScreenTools.ResolutionMaintainRatio;
                Point safe = ScreenTools.SafezoneBounds;

                const int interval = 45;

                DateTime finalDate = passiveModeDisabled;
                string timeSpanLeft = (finalDate - DateTime.UtcNow).ToString(@"mm\:ss");

                PointF left = new PointF(Convert.ToInt32(res.Width) - safe.X - 318, Convert.ToInt32(res.Height) - safe.Y - (100 + (1 * interval)));
                PointF right = new PointF(Convert.ToInt32(res.Width) - safe.X - 20, Convert.ToInt32(res.Height) - safe.Y - (102 + (1 * interval)));
                PointF background = new Point(Convert.ToInt32(res.Width) - safe.X - 248, Convert.ToInt32(res.Height) - safe.Y - (100 + (1 * interval)));

                new UIResText("Passive Mode Cooldown", left, 0.3f).Draw();
                new UIResText($"{timeSpanLeft}", right, 0.5f, Color.FromArgb(255, 255, 255, 255), CitizenFX.Core.UI.Font.ChaletLondon, Alignment.Right).Draw();
                new NativeUI.Sprite("timerbars", "all_black_bg", background, new Size(300, 37), 0f, Color.FromArgb(180, 255, 255, 255)).Draw();

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
            PluginManager.Instance.AttachTickHandler(PlayerKilledSelfCooldownTick);

            EventSystem.GetModule().Send("character:killed:self");
        }

        public async Task PlayerKilledSelfCooldownTick()
        {
            if (DateTime.Now.Subtract(playerKilledSelf).TotalMinutes >= 5)
            {
                Instance.DetachTickHandler(PlayerKilledSelfCooldownTick);
                IsKillSelfEnabled = true;

                NumberOfTimesKillSelf++;
            }
            else
            {
                IsKillSelfEnabled = false;
            }

            await BaseScript.Delay(1500);
        }
    }
}
