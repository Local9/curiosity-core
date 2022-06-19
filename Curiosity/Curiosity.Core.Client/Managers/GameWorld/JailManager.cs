using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Utils;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using NativeUI;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.GameWorld
{
    public class JailManager : Manager<JailManager>
    {
        PlayerOptionsManager PlayerOptionsManager => PlayerOptionsManager.GetModule();


        Vector3 jailStart = new Vector3(1812.00183f, 2736.8418f, 40.5720177f);
        Vector3 jailEnd = new Vector3(1591.87036f, 2452.705f, 90.95463f);

        Vector3 jailVehicleStart = new Vector3(1802.85742f, 2617.514f, 43.2783966f);
        Vector3 jailVehicleEnd = new Vector3(1792.96167f, 2617.97119f, 50.8206558f);
        float jailVehicleWidth = 20;

        Vector3 mainJail = new(1669.652f, 2564.316f, 45.56488f);

        float width = 340;

        public bool IsJailed = false;
        DateTime jailEndTime;

        public override void Begin()
        {
            EventSystem.Attach("police:suspect:jail", new AsyncEventCallback(async metadata =>
            {
                jailEndTime = DateTime.UtcNow.AddMinutes(3);

                PlayerOptionsManager.DisableWeapons(true);

                float x = metadata.Find<float>(0);
                float y = metadata.Find<float>(1);
                float z = metadata.Find<float>(2);

                await TeleportPlayer(mainJail.X, mainJail.Y, mainJail.Z);

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
            Vector3 pos = new Vector3(x, y, z);
            Game.PlayerPed.Position = new Vector3(x, y, z);

            if (Game.PlayerPed.IsDead)
            {
                Cache.Player.Character.Revive(new Position(pos.X, pos.Y, pos.Z, Cache.PlayerPed.Heading));
            }

            await BaseScript.Delay(500);
            await ScreenInterface.FadeIn(2000);
        }


        bool lastCheck = false;
        [TickHandler(SessionWait = true)]
        private async Task OnJailWeaponCheck()
        {
            Ped playerPed = Game.PlayerPed;
            bool isInsideJail = Common.IsEntityInAngledArea(playerPed, jailStart, jailEnd, width);

            if (isInsideJail != lastCheck)
            {
                lastCheck = isInsideJail;
                PlayerOptionsManager.DisableWeapons(isInsideJail);

                bool canDeleteVehicle = Common.IsEntityInAngledArea(playerPed, jailVehicleStart, jailVehicleEnd, jailVehicleWidth);

                if (Game.PlayerPed.IsInVehicle() && canDeleteVehicle)
                {
                    Vehicle playerVehicle = Game.PlayerPed.CurrentVehicle;
                    if (playerVehicle.Driver == playerPed)
                    {
                        playerVehicle.Dispose();
                    }
                }
            }
        }

        private async Task OnJailCheck()
        {
            bool isInsideJail = Common.IsEntityInAngledArea(Game.PlayerPed, jailStart, jailEnd, width);
            if (isInsideJail) return; // if they are jailed and still inside, do nothing

            //Instance.DetachTickHandler(OnJailTimerCheck);
            //Instance.DetachTickHandler(OnJailCheck);
            // if they have left, then we need to inform the police
            await TeleportPlayer(mainJail.X, mainJail.Y, mainJail.Z);
        }

        private async Task OnJailTimerCheck()
        {
            string timeSpanLeft = (jailEndTime - DateTime.UtcNow).ToString(@"mm\:ss");

            TextTimerBar textTimerBar = new TextTimerBar("Jail Time Remaining", timeSpanLeft);
            textTimerBar.Draw(45);

            Screen.Hud.HideComponentThisFrame(HudComponent.AreaName);
            Screen.Hud.HideComponentThisFrame(HudComponent.StreetName);
            Screen.Hud.HideComponentThisFrame(HudComponent.VehicleName);

            PlayerOptionsManager.DisableWeapons(true);

            if (DateTime.UtcNow > jailEndTime)
            {
                Instance.DetachTickHandler(OnJailTimerCheck);
                Instance.DetachTickHandler(OnJailCheck);

                StungunManager.GetModule().StungunCount = 0;

                TeleportPlayer(1847.085f, 2585.711f, 45.67204f);
                PlayerOptionsManager.DisableWeapons(false);
                EventSystem.Send("police:player:jail:served");
            }
        }
    }
}
