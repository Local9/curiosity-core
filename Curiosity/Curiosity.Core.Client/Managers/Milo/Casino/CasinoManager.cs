using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.Milo.Casino
{
    public class CasinoManager : Manager<CasinoManager>
    {
        private const string AUDIO_SCENE = "DLC_VW_Casino_General";
        Position posEnter = new Position(924.4668f, 46.7468f, 81.10635f);
        Position posExit = new Position(1089.974f, 206.0144f, -48.99975f);
        Vector3 scale = new Vector3(1.25f, 1.25f, 0.5f);

        NativeUI.Marker markerEnter;
        NativeUI.Marker markerExit;

        public async override void Begin()
        {
            Logger.Info($"Started Casino Manager");

            await Session.Loading();

            markerEnter = new NativeUI.Marker(MarkerType.VerticalCylinder, posEnter.AsVector(true), scale, 10f, System.Drawing.Color.FromArgb(255, 135, 206, 235), placeOnGround: true);
            markerExit = new NativeUI.Marker(MarkerType.VerticalCylinder, posExit.AsVector(true), scale, 2f, System.Drawing.Color.FromArgb(255, 135, 206, 235), placeOnGround: true);

            NativeUI.MarkersHandler.AddMarker(markerEnter);
            NativeUI.MarkersHandler.AddMarker(markerExit);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnCasinoTeleporterTick()
        {
            string message = $"Casino.";
            Vector3 notificationPosition = Vector3.Zero;

            if (markerEnter.IsInRange)
            {
                message = $"Enter {message}";
                notificationPosition = markerEnter.Position;

                if (markerEnter.IsInMarker)
                    NativeUI.Notifications.ShowHelpNotification($"Press ~INPUT_CONTEXT~ to enter the {message}");

                if (markerEnter.IsInMarker && Game.IsControlPressed(0, Control.Context))
                {
                    MovePlayer(true);
                }
            }

            if (markerExit.IsInRange)
            {
                message = $"Exit {message}";
                notificationPosition = markerExit.Position;

                if (markerExit.IsInMarker)
                    NativeUI.Notifications.ShowHelpNotification($"Press ~INPUT_CONTEXT~ to leave the {message}");

                if (markerExit.IsInMarker && Game.IsControlPressed(0, Control.Context))
                {
                    MovePlayer();
                }
            }

            if (notificationPosition != Vector3.Zero)
            {
                notificationPosition.Z = notificationPosition.Z + 1f;
                NativeUI.Notifications.ShowFloatingHelpNotification(message, notificationPosition);
            }
        }

        private async Task MovePlayer(bool enterCasino = false)
        {
            await Cache.PlayerPed.FadeOut();
            await ScreenInterface.FadeOut(1000);

            WorldManager worldManager = WorldManager.GetModule();

            Cache.PlayerPed.IsCollisionEnabled = false;

            Position pos = enterCasino ? posExit : posEnter;

            int interiorId = GetInteriorAtCoords(1089.974f, 206.0144f, -48.99975f);

            if (enterCasino)
            {
                Instance.DiscordRichPresence.Status = $"Betting at the Casino";
                worldManager.LockAndSetTime(12, 1);
                worldManager.LockAndSetWeather(WeatherType.EXTRASUNNY);
                AudioSettings();

                RequestIpl("vw_casino_main");
                CasinoTurnTable.Init();
                CasinoLuckyWheel.Init();
            }
            else
            {
                Instance.DiscordRichPresence.Status = $"Roaming Los Santos";
                worldManager.UnlockTime();
                worldManager.UnlockAndUpdateWeather();

                if (IsAudioSceneActive("DLC_VW_Casino_General"))
                {
                    StopAudioScene(AUDIO_SCENE);
                }

                RemoveIpl("vw_casino_main");
                CasinoTurnTable.Dispose();
                CasinoLuckyWheel.Dispose();
            }
            Instance.DiscordRichPresence.Commit();

            PlayerOptionsManager.GetModule().DisableWeapons(enterCasino);

            Dictionary<string, BlipData> blips = BlipManager.GetModule().AllBlips;
            foreach (KeyValuePair<string, BlipData> kvp in blips)
            {
                BlipData blip = kvp.Value;
                foreach (Blip b in blip.Blips)
                {
                    SetBlipHiddenOnLegend(b.Handle, enterCasino);
                }
            }

            if (IsValidInterior(interiorId))
                RefreshInterior(interiorId);

            WorldManager.GetModule().UpdateWeather(true);

            Cache.PlayerPed.IsCollisionEnabled = true;

            SetEntityCoords(Cache.PlayerPed.Handle, pos.X, pos.Y, pos.Z, true, false, false, true);

            await BaseScript.Delay(1500);
            await ScreenInterface.FadeIn(1000);
            await Cache.PlayerPed.FadeIn();
        }

        static async void AudioSettings()
        {
            bool hasRequestedAudio = false;
            int numberOfPasses = 0;

            if (!IsAudioSceneActive("DLC_VW_Casino_General"))
            {
                StartAudioScene("DLC_VW_Casino_General");
            }

            while (!hasRequestedAudio)
            {
                await BaseScript.Delay(100);

                if ((((RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false)
                    && RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_01", false))
                    && RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_02", false))
                    && RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_03", false))
                    && LoadStream("casino_walla", "DLC_VW_Casino_Interior_Sounds"))
                {
                    hasRequestedAudio = true;
                }

                if (numberOfPasses > 5) break;

                numberOfPasses++;
            }
        }
    }
}
