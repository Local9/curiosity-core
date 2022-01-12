using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Events;
using Curiosity.Core.Client.Interface;
using CitizenFX.Core.UI;
using NativeUI;

namespace Curiosity.Core.Client.Managers.GameWorld
{
    public class JailManager : Manager<JailManager>
    {
        Vector3 jailStart = new Vector3(1849.58679f, 2605.6687f, 40.5720177f);
        Vector3 jailEnd = new Vector3(1522.98364f, 2598.043f, 90.95463f);
        float width = 400;

        public bool IsJailed = false;
        DateTime jailEndTime;

        public override void Begin()
        {
            EventSystem.Attach("jail:start", new AsyncEventCallback(async metadata =>
            {
                jailEndTime = DateTime.UtcNow.AddMinutes(3);

                float x = metadata.Find<float>(0);
                float y = metadata.Find<float>(1);
                float z = metadata.Find<float>(2);

                await TeleportPlayer(x, y, z);

                Instance.AttachTickHandler(OnJailCheck);
                Instance.AttachTickHandler(OnJailTimerCheck);

                return null;
            }));
        }

        private static async Task TeleportPlayer(float x, float y, float z)
        {
            await ScreenInterface.FadeOut(1000);
            Game.PlayerPed.IsPositionFrozen = true;
            Game.PlayerPed.Position = new Vector3(x, y, z + 10f);
            await BaseScript.Delay(1000);
            float groundZ = z + 10f;
            if (GetGroundZFor_3dCoord(x, y, z, ref groundZ, false))
                z = groundZ;

            Game.PlayerPed.IsPositionFrozen = false;
            Game.PlayerPed.Position = new Vector3(x, y, z);

            await BaseScript.Delay(500);
            await ScreenInterface.FadeIn(2000);
        }

        private async Task OnJailCheck()
        {
            bool isInsideJail = Common.IsEntityInAngledArea(Game.PlayerPed, jailStart, jailEnd, width);
            if (isInsideJail) return; // if they are jailed and still inside, do nothing

            Instance.DetachTickHandler(OnJailTimerCheck);
            Instance.DetachTickHandler(OnJailCheck);
            // if they have left, then we need to inform the police
            EventSystem.Send("jail:player:escape");
        }

        private async Task OnJailTimerCheck()
        {
            string timeSpanLeft = (jailEndTime - DateTime.UtcNow).ToString(@"mm\:ss");

            TextTimerBar textTimerBar = new TextTimerBar("Passive Mode Cooldown", timeSpanLeft);
            textTimerBar.Draw(45);

            Screen.Hud.HideComponentThisFrame(HudComponent.AreaName);
            Screen.Hud.HideComponentThisFrame(HudComponent.StreetName);
            Screen.Hud.HideComponentThisFrame(HudComponent.VehicleName);

            if (jailEndTime < DateTime.UtcNow)
            {
                Instance.DetachTickHandler(OnJailTimerCheck);
                Instance.DetachTickHandler(OnJailCheck);

                TeleportPlayer(1847.085f, 2585.711f, 45.67204f);
                EventSystem.Send("jail:player:timeServed");
            }
        }
    }
}
