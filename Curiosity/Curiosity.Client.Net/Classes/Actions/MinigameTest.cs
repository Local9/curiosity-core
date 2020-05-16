using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Actions
{
    class MinigameTest
    {
        static int WorldCameraId;
        static bool IsWorldCameraSetup = false;
        static int ScaleformHandle;
        static int BlipHandle;

        static bool IsLeftOrRightPressed = false;

        static bool IsLeftPressed = false;
        static bool WasLeftPressed = false;
        static float MoveLeft = 0f;

        static bool IsRightPressed = false;
        static bool WasRightPressed = false;
        static float MoveRight = 0f;

        static bool IsUpPressed = false;
        static bool WasUpPressed = false;
        static float MoveUp = 0f;

        static bool IsDownPressed = false;
        static bool WasDownPressed = false;
        static float MoveDown = 0f;

        public static async void Start()
        {
            IsLeftOrRightPressed = false;

            IsLeftPressed = false;
            WasLeftPressed = false;
            MoveLeft = 0f;

            IsRightPressed = false;
            WasRightPressed = false;
            MoveRight = 0f;

            IsUpPressed = false;
            WasUpPressed = false;
            MoveUp = 0f;

            IsDownPressed = false;
            WasDownPressed = false;
            MoveDown = 0f;

            BlipHandle = AddBlipForCoord(100f, 100f, 30f);

            if (IsScreenFadedOut())
            {
                DoScreenFadeIn(500);
            }

            if (HasForceCleanupOccurred(3))
            {
                CleanUp();
            }

            StartAudioScene("dlc_xm_orbital_cannon_camera_active_scene");

            ScaleformHandle = RequestScaleformMovie("ORBITAL_CANNON_CAM");
            while (!HasScaleformMovieLoaded(ScaleformHandle))
            {
                await Client.Delay(0);
            }

            BeginScaleformMovieMethod(ScaleformHandle, "SET_ZOOM_LEVEL");
            ScaleformMovieMethodAddParamFloat(0f);
            EndScaleformMovieMethod();

            while (true)
            {
                if (!IsWorldCameraSetup)
                {
                    IsWorldCameraSetup = true;
                    WorldCameraId = CreateCam("DEFAULT_SCRIPTED_CAMERA", false);
                    if (DoesCamExist(WorldCameraId))
                    {
                        SetCamCoord(WorldCameraId, -8.8511f, 6835.003f, 400f);
                        SetCamRot(WorldCameraId, -90f, 0f, 0f, 2);
                        SetCamFov(WorldCameraId, 100f);
                        SetCamActive(WorldCameraId, true);
                        RenderScriptCams(true, false, 3000, true, false);
                    }
                    SetBlipSprite(BlipHandle, 66);
                }
                DrawScaleformMovieFullscreen(ScaleformHandle, 255, 255, 255, 200, 0);

                CaptureInputs(ScaleformHandle);
                await Client.Delay(0);
            }
        }

        static void CaptureInputs(int scaleform)
        {
            if (!IsLeftOrRightPressed)
            {
                if (IsControlPressed(2, 189) || IsControlPressed(2, 190))
                {
                    IsLeftOrRightPressed = true;
                    Settimera(0);
                }
            }
            else if (Timera() > 50)
            {
                IsLeftOrRightPressed = false;
            }

            if (!IsLeftOrRightPressed)
            {
                if (IsControlJustPressed(2, 189))
                {
                    IsLeftPressed = true;
                    MoveLeft = 1f;
                }

                if (IsControlPressed(2, 189))
                {
                    IsLeftPressed = true;
                    MoveLeft = 1f;
                }

                if (!IsControlPressed(2, 189))
                {
                    IsLeftPressed = false;
                    MoveLeft = 0f;
                }

                if (!IsLeftPressed == WasLeftPressed)
                {
                    WasLeftPressed = IsLeftPressed;
                    BeginScaleformMovieMethod(scaleform, "SET_INPUT_EVENT");
                    ScaleformMovieMethodAddParamInt(10);
                    ScaleformMovieMethodAddParamFloat(MoveLeft);
                    EndScaleformMovieMethod();
                }

                if (IsControlJustPressed(2, 190))
                {
                    IsRightPressed = true;
                    MoveRight = 1f;
                }

                if (IsControlPressed(2, 190))
                {
                    IsRightPressed = true;
                    MoveRight = 1f;
                }

                if (!IsControlPressed(2, 190))
                {
                    IsRightPressed = false;
                    MoveRight = 0f;
                }

                if (!IsRightPressed == WasRightPressed)
                {
                    WasRightPressed = IsRightPressed;
                    BeginScaleformMovieMethod(scaleform, "SET_INPUT_EVENT");
                    ScaleformMovieMethodAddParamInt(11);
                    ScaleformMovieMethodAddParamFloat(MoveRight);
                    EndScaleformMovieMethod();
                }

                if (IsControlJustPressed(2, 188))
                {
                    IsUpPressed = true;
                    MoveUp = 1f;
                }

                if (IsControlPressed(2, 188))
                {
                    IsUpPressed = true;
                    MoveUp = 1f;
                }

                if (!IsControlPressed(2, 188))
                {
                    IsUpPressed = false;
                    MoveUp = 0f;
                }

                if (!IsUpPressed == WasUpPressed)
                {
                    WasUpPressed = IsUpPressed;
                    BeginScaleformMovieMethod(scaleform, "SET_INPUT_EVENT");
                    ScaleformMovieMethodAddParamInt(8);
                    ScaleformMovieMethodAddParamFloat(MoveUp);
                    EndScaleformMovieMethod();
                }

                if (IsControlJustPressed(2, 187))
                {
                    IsDownPressed = true;
                    MoveDown = 1f;
                }

                if (IsControlPressed(2, 187))
                {
                    IsDownPressed = true;
                    MoveDown = 1f;
                }

                if (!IsControlPressed(2, 187))
                {
                    IsDownPressed = false;
                    MoveDown = 0f;
                }

                if (!IsDownPressed == WasDownPressed)
                {
                    WasDownPressed = IsDownPressed;
                    BeginScaleformMovieMethod(scaleform, "SET_INPUT_EVENT");
                    ScaleformMovieMethodAddParamInt(9);
                    ScaleformMovieMethodAddParamFloat(MoveDown);
                    EndScaleformMovieMethod();
                }
            }
        }

        static void CleanUp()
        {
            if (DoesCamExist(WorldCameraId))
            {
                DestroyCam(WorldCameraId, false);
            }
            RenderScriptCams(false, false, 3000, true, false);
            SetGamePaused(false);
            ClearHelp(true);
            SetScaleformMovieAsNoLongerNeeded(ref ScaleformHandle);
            TerminateThisThread();
        }
    }
}
