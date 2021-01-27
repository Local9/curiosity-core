using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using NativeUI;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Utils
{
    public class PlayerOptions
    {
        static DateTime passiveModeEnabledDate;
        public static bool IsPassiveModeEnabled = true;

        public static void SetPlayerPassive(bool isPassive)
        {
            if (API.NetworkIsGameInProgress())
            {
                if (!isPassive)
                {
                    Game.PlayerPed.CanBeDraggedOutOfVehicle = true;
                    Game.PlayerPed.SetConfigFlag(342, false);
                    Game.PlayerPed.SetConfigFlag(122, false);
                    API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 1f);
                    API.NetworkSetPlayerIsPassive(false);
                    API.NetworkSetFriendlyFireOption(true);

                    passiveModeEnabledDate = DateTime.Now;
                    PluginManager.Instance.AttachTickHandler(PassiveCooldownTick);

                    Game.PlayerPed.CanSwitchWeapons = true;
                }
                else
                {
                    Game.PlayerPed.CanBeDraggedOutOfVehicle = false;
                    Game.PlayerPed.SetConfigFlag(342, true);
                    Game.PlayerPed.SetConfigFlag(122, true);
                    API.SetPlayerVehicleDefenseModifier(Game.Player.Handle, 0.5f);
                    API.NetworkSetPlayerIsPassive(true);
                    API.NetworkSetFriendlyFireOption(false);
                    Game.PlayerPed.Weapons.Select(WeaponHash.Unarmed);

                    Game.PlayerPed.CanSwitchWeapons = false;
                }
            }
        }

        private static async Task PassiveCooldownTick()
        {
            if (DateTime.Now.Subtract(passiveModeEnabledDate).TotalMinutes >= 5)
            {
                PluginManager.Instance.DetachTickHandler(PassiveCooldownTick);
                IsPassiveModeEnabled = true;
            }
            else
            {
                IsPassiveModeEnabled = false;

                SizeF res = ScreenTools.ResolutionMaintainRatio;
                Point safe = ScreenTools.SafezoneBounds;

                const int interval = 45;

                DateTime finalDate = passiveModeEnabledDate;
                string timeSpanLeft = (finalDate.AddMinutes(5) - DateTime.Now).ToString(@"mm\:ss");

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
    }
}
