using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.Milo.Casino
{
    public class CasinoManager : Manager<CasinoManager>
    {
        private const string AUDIO_STREAM_CASINO_GENERAL = "DLC_VW_Casino_General";
        Position posEnter = new Position(924.4668f, 46.7468f, 81.10635f);
        Position posExit = new Position(1089.974f, 206.0144f, -48.99975f);
        Vector3 scale = new Vector3(1.25f, 1.25f, 0.5f);

        NativeUI.Marker markerEnter;
        NativeUI.Marker markerExit;

        private bool isInCasino = false;
        private bool showBigWin = false;
        WorldManager WorldManager = WorldManager.GetModule();

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

            Cache.PlayerPed.IsCollisionEnabled = false;

            Position pos = enterCasino ? posExit : posEnter;

            int interiorId = GetInteriorAtCoords(1089.974f, 206.0144f, -48.99975f);

            if (enterCasino)
            {
                Instance.DiscordRichPresence.Status = $"Betting at the Casino";
                WorldManager.LockAndSetTime(12, 1);
                WorldManager.LockAndSetWeather(WeatherType.EXTRASUNNY);

                isInCasino = true;

                RequestIpl("vw_casino_main");
                CasinoTurnTable.Init();
                CasinoLuckyWheel.Init();
            }
            else
            {
                Instance.DiscordRichPresence.Status = $"Roaming Los Santos";
                WorldManager.UnlockTime();
                WorldManager.UnlockAndUpdateWeather();

                isInCasino = false;

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
                    SetBlipAlpha(b.Handle, enterCasino ? 0 : 255);
                }
            }

            if (IsValidInterior(interiorId))
                RefreshInterior(interiorId);

            WorldManager.GetModule().UpdateWeather(true);

            Cache.PlayerPed.IsCollisionEnabled = true;

            SetEntityCoords(Cache.PlayerPed.Handle, pos.X, pos.Y, pos.Z, true, false, false, true);

            await BaseScript.Delay(1500);

            if (isInCasino)
            {
                Instance.AttachTickHandler(RenderWalls);
                Instance.AttachTickHandler(AudioSettings);
            }

            await ScreenInterface.FadeIn(1000);
            await Cache.PlayerPed.FadeIn();
        }

        private async Task AudioSettings()
        {
            try
            {
                async void requestAudio()
                {
                    while (!RequestScriptAudioBank("DLC_VINEWOOD/CASINO_GENERAL", false))
                    {
                        await BaseScript.Delay(100);
                    }
                    while (!RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_01", false))
                    {
                        await BaseScript.Delay(100);
                    }
                    while (!RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_02", false))
                    {
                        await BaseScript.Delay(100);
                    }
                    while (!RequestScriptAudioBank("DLC_VINEWOOD/CASINO_SLOT_MACHINES_03", false))
                    {
                        await BaseScript.Delay(100);
                    }
                }
                requestAudio();

                while (isInCasino)
                {
                    if (!IsStreamPlaying() && LoadStream("casino_walla", "DLC_VW_Casino_Interior_Sounds"))
                        PlayStreamFromPosition(945.85f, 41.58f, 75.82f);

                    if (IsStreamPlaying() && !IsAudioSceneActive(AUDIO_STREAM_CASINO_GENERAL))
                    {
                        StartAudioScene(AUDIO_STREAM_CASINO_GENERAL);
                    }

                    await BaseScript.Delay(1000);
                }

                if (IsStreamPlaying())
                    StopStream();

                if (IsAudioSceneActive(AUDIO_STREAM_CASINO_GENERAL))
                    StopAudioScene(AUDIO_STREAM_CASINO_GENERAL);

                Instance.DetachTickHandler(AudioSettings);
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "AudioSettings");
                Instance.DetachTickHandler(AudioSettings);
            }
        }

        private async Task RenderWalls()
        {
            try
            {
                int attempts = 0;
                RequestStreamedTextureDict("Prop_Screen_Vinewood", false);
                while (!HasStreamedTextureDictLoaded("Prop_Screen_Vinewood"))
                {
                    await BaseScript.Delay(100);
                    RequestStreamedTextureDict("Prop_Screen_Vinewood", false);
                    if (attempts > 10) break;
                    attempts++;
                }

                RegisterNamedRendertarget("casinoscreen_01", false);
                uint renderTarget = (uint)GetHashKey("vw_vwint01_video_overlay");
                LinkNamedRendertarget(renderTarget);
                int videoWallRenderTarget = GetNamedRendertargetRenderId("casinoscreen_01");
                int lastUpdatedTvChannel = 0;
                string casinoVideo = "CASINO_SNWFLK_PL";
                bool isWinter = await WorldManager.IsWinter();
                bool isHalloween = await WorldManager.IsHalloween();

                while (isInCasino)
                {
                    await BaseScript.Delay(0);

                    if (videoWallRenderTarget != 0)
                    {
                        int currentTime = GetGameTimer();
                        if (showBigWin)
                        {
                            SetVideoWall("CASINO_WIN_PL", true);
                            lastUpdatedTvChannel = GetGameTimer() - 33666;
                            showBigWin = false;

                            Instance.DetachTickHandler(RenderWalls);
                        }
                        else
                        {
                            if ((currentTime - lastUpdatedTvChannel) >= 42666)
                            {
                                if (isWinter)
                                    casinoVideo = "CASINO_SNWFLK_PL";

                                if (isHalloween)
                                    casinoVideo = "CASINO_HLW_PL";

                                SetVideoWall(casinoVideo);
                                lastUpdatedTvChannel = currentTime;
                            }
                        }

                        SetTextRenderId(videoWallRenderTarget);
                        SetScriptGfxDrawOrder(4);
                        SetScriptGfxDrawBehindPausemenu(true);
                        DrawInteractiveSprite("Prop_Screen_Vinewood", "BG_Wall_Colour_4x4", 0.25f, 0.5f, 0.5f, 1.0f, 0.0f, 255, 255, 255, 255);
                        DrawTvChannel(0.5f, 0.5f, 1.0f, 1.0f, 0.0f, 255, 255, 255, 255);
                        SetTextRenderId(GetDefaultScriptRendertargetRenderId());
                    }
                    else
                    {
                        videoWallRenderTarget = GetNamedRendertargetRenderId("casinoscreen_01");
                        await BaseScript.Delay(100);
                    }
                }

                ReleaseNamedRendertarget("casinoscreen_01");
                videoWallRenderTarget = 0;
                showBigWin = false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "RenderWalls");
                Instance.DetachTickHandler(RenderWalls);
            }
        }

        void SetVideoWall(string playlist, bool forceRefresh = false)
        {
            Logger.Debug($"playlist: {playlist}");
            SetTvChannelPlaylist(0, playlist, true);
            SetTvAudioFrontend(false);
            SetTvVolume(-100.0f);

            if (forceRefresh)
                SetTvChannel(-1);

            SetTvChannel(0);
        }
    }
}
