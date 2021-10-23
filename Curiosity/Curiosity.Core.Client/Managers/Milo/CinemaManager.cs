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


namespace Curiosity.Core.Client.Managers.Milo
{
    public class CinemaManager : Manager<CinemaManager>
    {
        // Movie Theatre 	new Vector3(-1427.299, -245.1012, 16.8039);

        Position posEnter = new Position(-1423.533f, -214.2786f, 46.50037f, 357.6964f);
        Position posExit = new Position(-1437.539f, -245.3138f, 16.80255f, 279.8171f);
        Vector3 scale = new Vector3(1.5f, 1.5f, 0.5f);

        private const float MinY = -89f, MaxY = 89f;

        NativeUI.Marker markerEnter;
        NativeUI.Marker markerExit;

        Camera movieCamera;
        Vector3 cameraPos = new Vector3(-1426.849f, -251.2769f, 17.96699f);

        public async override void Begin()
        {
            Logger.Info($"Started Cinema Manager");

            await Session.Loading();

            markerEnter = new NativeUI.Marker(MarkerType.VerticalCylinder, posEnter.AsVector(true), scale, 10f, System.Drawing.Color.FromArgb(255, 135, 206, 235));
            markerExit = new NativeUI.Marker(MarkerType.VerticalCylinder, posExit.AsVector(true), scale, 2f, System.Drawing.Color.FromArgb(255, 135, 206, 235));

            NativeUI.MarkersHandler.AddMarker(markerEnter);
            NativeUI.MarkersHandler.AddMarker(markerExit);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnCinemaTeleportTick()
        {
            string message = $"Movie Theatre.";
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

        async Task OnToggleMovieCamera()
        {
            bool isCameraActive = movieCamera is not null;

            string msg = $"Press ~INPUT_CONTEXT~ to use the set camera.";
            if (isCameraActive)
                msg = $"Press ~INPUT_CONTEXT~ to exit the camera.~n~" +
                    $"Cameras ~INPUT_SELECT_WEAPON_UNARMED~ ~INPUT_SELECT_WEAPON_MELEE~ ~INPUT_SELECT_WEAPON_SHOTGUN~";

            if (!markerExit.IsInRange)
                Screen.DisplayHelpTextThisFrame(msg);

            if (Game.IsControlJustPressed(0, Control.Context) && !markerExit.IsInRange)
            {
                if (movieCamera is null)
                {
                    CreateCamera();
                    return;
                }

                if (movieCamera is not null)
                {
                    DestroyCamera();
                }
            }

            if (Game.IsControlJustPressed(0, Control.SelectWeaponUnarmed) && movieCamera is not null)
            {
                movieCamera.Position = new Vector3(-1426.849f, -251.2769f, 17.96699f);
            }

            if (Game.IsControlJustPressed(0, Control.SelectWeaponMelee) && movieCamera is not null)
            {
                movieCamera.Position = new Vector3(-1426.573f, -252.7998f, 19.66913f);
            }

            if (Game.IsControlJustPressed(0, Control.SelectWeaponShotgun) && movieCamera is not null)
            {
                movieCamera.Position = new Vector3(-1426.642f, -242.0524f, 19.60905f);
            }
        }

        async Task OnNoClipCheckRotationTick()
        {
            try
            {
                if (movieCamera == null)
                {
                    await BaseScript.Delay(100);
                    return;
                }

                var rightAxisX = Game.GetDisabledControlNormal(0, (Control)220);
                var rightAxisY = Game.GetDisabledControlNormal(0, (Control)221);

                if (!(Math.Abs(rightAxisX) > 0) && !(Math.Abs(rightAxisY) > 0)) return;
                var rotation = movieCamera.Rotation;
                rotation.Z += rightAxisX * -10f;

                var yValue = rightAxisY * -5f;
                if (rotation.X + yValue > MinY && rotation.X + yValue < MaxY)
                    rotation.X += yValue;
                movieCamera.Rotation = rotation;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void DestroyCamera()
        {
            movieCamera.Delete();
            movieCamera = null;
            World.RenderingCamera = null;

            World.RenderingCamera = null;
            Cache.PlayerPed.IsPositionFrozen = false;
            Cache.PlayerPed.IsCollisionEnabled = true;
            Cache.PlayerPed.CanRagdoll = true;
            Cache.PlayerPed.IsVisible = true;
            Cache.PlayerPed.Opacity = 255;
            Cache.PlayerPed.Task.ClearAllImmediately();

            Instance.DetachTickHandler(OnNoClipCheckRotationTick);
        }

        void CreateCamera()
        {
            movieCamera = World.CreateCamera(cameraPos, GameplayCamera.Rotation, 75f);
            World.RenderingCamera = movieCamera;

            Cache.PlayerPed.IsPositionFrozen = true;
            Cache.PlayerPed.IsCollisionEnabled = false;
            Cache.PlayerPed.Opacity = 0;
            Cache.PlayerPed.CanRagdoll = false;
            Cache.PlayerPed.IsVisible = false;
            Cache.PlayerPed.Task.ClearAllImmediately();

            Instance.AttachTickHandler(OnNoClipCheckRotationTick);
        }

        private async Task MovePlayer(bool enterCinema = false)
        {
            await Cache.PlayerPed.FadeOut();
            await ScreenInterface.FadeOut(1000);

            WorldManager worldManager = WorldManager.GetModule();

            Cache.PlayerPed.IsCollisionEnabled = false;

            Position pos = enterCinema ? posExit : posEnter;

            if (enterCinema)
            {
                Instance.DiscordRichPresence.Status = $"Watching movies...";
                worldManager.LockAndSetTime(9, 0);
                worldManager.LockAndSetWeather(WeatherType.CLEAR);
                Instance.AttachTickHandler(OnToggleMovieCamera);
            }
            else
            {
                Instance.DiscordRichPresence.Status = $"Roaming Los Santos...";
                worldManager.UnlockTime();
                worldManager.UnlockAndUpdateWeather();
                Instance.DetachTickHandler(OnToggleMovieCamera);
            }
            Instance.DiscordRichPresence.Commit();

            PlayerOptionsManager.GetModule().DisableWeapons(enterCinema);

            Dictionary<string, BlipData> blips = BlipManager.GetModule().AllBlips;
            foreach (KeyValuePair<string, BlipData> kvp in blips)
            {
                BlipData blip = kvp.Value;
                foreach (Blip b in blip.Blips)
                {
                    SetBlipHiddenOnLegend(b.Handle, enterCinema);
                    SetBlipAlpha(b.Handle, enterCinema ? 0 : 255);
                }
            }

            WorldManager.GetModule().UpdateWeather(true);

            Cache.PlayerPed.IsCollisionEnabled = true;

            SetPedCoordsKeepVehicle(Cache.PlayerPed.Handle, pos.X, pos.Y, pos.Z);

            await BaseScript.Delay(1500);
            await ScreenInterface.FadeIn(1000);
            await Cache.PlayerPed.FadeIn();
        }
    }
}
