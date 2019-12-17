using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Police.Client.net.Environment.Vehicle
{
    enum VehicleDisplay
    {
        FULL_INFO,
        MODEL_PLATE,
        TURNED_OFF
    }

    enum VisionState
    {
        NORMAL,
        NIGHT_MODE,
        THERMAL_VISION
    }

    class PolmavScripts
    {
        static Client client = Client.GetInstance();

        private const double MATH_PI = 3.14159265358979323846264338327950288;

        private const string DECOR_TARGET = "curiosity:spotlight:target";
        private const string DECOR_SPOTLIGHT_X = "curiosity:spotlight:vector:X";
        private const string DECOR_SPOTLIGHT_Y = "curiosity:spotlight:vector:Y";
        private const string DECOR_SPOTLIGHT_Z = "curiosity:spotlight:vector:Z";

        private const string POLMAV_SPOTLIGHT_PAUSE = "curiosity:police:spotlight:pause";
        private const string POLMAV_SPOTLIGHT_MANUAL = "curiosity:police:spotlight:manual";
        private const string POLMAV_SPOTLIGHT_TOGGLE = "curiosity:police:spotlight:toggle";
        private const string POLMAV_SPOTLIGHT_FORWARD = "curiosity:police:spotlight:forward";
        private const string POLMAV_SPOTLIGHT_TRACKING = "curiosity:police:spotlight:tracking";
        private const string POLMAV_SPOTLIGHT_TRACKING_TOGGLE = "curiosity:police:spotlight:tracking:toggle";
        private const string POLMAV_SPOTLIGHT_BRIGHTNESS_UP = "curiosity:police:spotlight:light:up";
        private const string POLMAV_SPOTLIGHT_BRIGHTNESS_DOWN = "curiosity:police:spotlight:light:down";
        private const string POLMAV_SPOTLIGHT_RADIUS_UP = "curiosity:police:spotlight:radius:up";
        private const string POLMAV_SPOTLIGHT_RADIUS_DOWN = "curiosity:police:spotlight:radius:down";

        static float _fovMax = 80.0f;
        static float _fovMin = 5.0f;
        static float _zoomspeed = 3.0f;
        static float _speedLR = 4.0f;
        static float _speedUD = 4.0f;
        static Control _toggleHelicam = Control.Context;
        static Control _toggleVision = Control.Aim;
        static Control _toggleRappel = Control.Duck;
        static Control _toggleSpotlight = Control.PhoneCameraGrid;
        static Control _toggleLockOn = Control.Sprint;
        static Control _toggleDisplay = Control.Cover;
        static Control _keyTurnLightUp = Control.PhoneUp;
        static Control _keyTurnLightDown = Control.PhoneDown;
        static Control _keyTurnLightRadiusDown = Control.PhoneLeft;
        static Control _keyTurnLightRadiusUp = Control.PhoneRight;
        static float _maxTargetDistance = 700;
        static float _brightness = 1.0f;
        static float _spotRadius = 4.0f;
        static string _speedMessure = "mph";
        // Script Settings
        static CitizenFX.Core.Vehicle _targetVehicle;
        static bool _manualSpotlight = false;
        static bool _trackingSpotlight = false;
        static VehicleDisplay _vehicleDisplay = VehicleDisplay.FULL_INFO;
        static bool _helicam = false;
        static VehicleHash _polmav = VehicleHash.Polmav;
        static float _fov = (_fovMax + _fovMin) * 0.5f;
        static float _zoomvalue = 0.0f;
        static VisionState _visionState = VisionState.NORMAL;

        static bool _trackingSpotlight_toggle = false;
        static bool _trackingSpotlight_pause = false;
        static int _trackingSpotlight_target;

        static bool _forwardSpotlight_state = false;
        static bool _manualSpotlight_toggle = false;

        public static void Init()
        {
            DecorRegister(DECOR_SPOTLIGHT_X, 3);
            DecorRegister(DECOR_SPOTLIGHT_Y, 3);
            DecorRegister(DECOR_SPOTLIGHT_Z, 3);
            DecorRegister(DECOR_TARGET, 3);

            client.RegisterTickHandler(OnMainTaskAsync);

            client.RegisterEventHandler(POLMAV_SPOTLIGHT_PAUSE, new Action<bool>(OnPauseTrackingSpotlight));
            client.RegisterEventHandler(POLMAV_SPOTLIGHT_MANUAL, new Action<int>(OnManualSpotlight));
            client.RegisterEventHandler(POLMAV_SPOTLIGHT_TOGGLE, new Action<int>(OnManualSpotlightToggle));

            client.RegisterEventHandler(POLMAV_SPOTLIGHT_FORWARD, new Action<int, bool>(OnForwardSpotlight));
            client.RegisterEventHandler(POLMAV_SPOTLIGHT_TRACKING, new Action<int, int, string, float, float, float>(OnTrackingSpotLight));
            client.RegisterEventHandler(POLMAV_SPOTLIGHT_TRACKING_TOGGLE, new Action<int, bool>(OnTrackingSpotLightToggle));

            client.RegisterEventHandler(POLMAV_SPOTLIGHT_BRIGHTNESS_UP, new Action(OnSpotlightLightUp));
            client.RegisterEventHandler(POLMAV_SPOTLIGHT_BRIGHTNESS_DOWN, new Action(OnSpotlightLightDown));
            client.RegisterEventHandler(POLMAV_SPOTLIGHT_RADIUS_UP, new Action(OnSpotlightRadiusUp));
            client.RegisterEventHandler(POLMAV_SPOTLIGHT_RADIUS_DOWN, new Action(OnSpotlightRadiusDown));
        }

        static async Task OnMainTaskAsync()
        {
            await BaseScript.Delay(0);
            if (IsPlayerInPolmav())
            {
                int playerPed = Game.PlayerPed.Handle;
                int heli = Game.PlayerPed.CurrentVehicle.Handle;

                if (IsHeliHighEnough())
                {
                    if (Game.IsControlJustPressed(0, _toggleHelicam))
                    {
                        PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                        _helicam = true;
                    }

                    if (Game.IsControlJustPressed(0, _toggleRappel))
                    {
                        if (GetPedInVehicleSeat(heli, 1) == playerPed
                            || GetPedInVehicleSeat(heli, 2) == playerPed)
                        {
                            PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                            TaskRappelFromHeli(playerPed, 1);
                        }
                        else
                        {
                            Screen.ShowNotification("~r~Cannot rappel from this seat");
                            PlaySoundFrontend(-1, "5_Second_Timer", "DLC_HEISTS_GENERAL_FRONTEND_SOUNDS", false);
                        }
                    }
                }

                if (!_helicam && GetPedInVehicleSeat(heli, -1) == playerPed && Game.IsControlJustPressed(0, _toggleSpotlight))
                {
                    if (_targetVehicle != null)
                    {
                        if (_trackingSpotlight)
                        {
                            if (!_trackingSpotlight_pause)
                            {
                                _trackingSpotlight_pause = true;
                                BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_PAUSE, _trackingSpotlight_pause);
                            }
                            else
                            {
                                _trackingSpotlight_pause = false;
                                BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_PAUSE, _trackingSpotlight_pause);
                            }
                            PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                        }
                        else
                        {
                            if (_forwardSpotlight_state)
                            {
                                _forwardSpotlight_state = false;
                                BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_FORWARD, Game.Player.ServerId, _forwardSpotlight_state);
                            }

                            _trackingSpotlight_pause = false;
                            _trackingSpotlight = true;

                            BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TRACKING, _targetVehicle.NetworkId, _targetVehicle.Mods.LicensePlate, _targetVehicle.Position.X, _targetVehicle.Position.Y, _targetVehicle.Position.Z);

                            PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                        }
                    }
                    else
                    {
                        if (_trackingSpotlight)
                        {
                            _trackingSpotlight_pause = false;
                            _trackingSpotlight = false;
                            BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TRACKING_TOGGLE);
                        }
                        _forwardSpotlight_state = !_forwardSpotlight_state;
                        BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_FORWARD, _forwardSpotlight_state);
                        PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                    }

                    if (Game.IsControlJustPressed(0, _toggleDisplay) && GetPedInVehicleSeat(heli, -1) == playerPed)
                    {
                        ChangeDisplay();
                    }

                    if (_targetVehicle != null && GetPedInVehicleSeat(heli, -1) == playerPed)
                    {
                        float targetDistance = Game.PlayerPed.CurrentVehicle.Position.Distance(_targetVehicle.Position);
                        if (Game.IsControlJustPressed(0, _toggleLockOn) || targetDistance > _maxTargetDistance)
                        {
                            DecorRemove(_targetVehicle.Handle, DECOR_TARGET);
                            if (_trackingSpotlight)
                            {
                                Client.TriggerServerEvent(POLMAV_SPOTLIGHT_TRACKING_TOGGLE);
                            }
                            
                            _trackingSpotlight = false;
                            _trackingSpotlight_pause = false;
                            _targetVehicle = null;

                            PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                        }
                    }

                    if (_helicam)
                    {
                        SetTimecycleModifier("heliGunCam");
                        SetTimecycleModifierStrength(0.3f);
                        Scaleform scaleform = new Scaleform("HELI_CAM");

                        while (!scaleform.IsLoaded)
                        {
                            await BaseScript.Delay(0);
                        }

                        Vector3 cameraRotation = new Vector3(0f, 0f, Game.PlayerPed.CurrentVehicle.Heading);
                        int cam = CreateCam("DEFAULT_SCRIPTED_FLY_CAMERA", true);
                        Camera camera = new Camera(cam);
                        camera.Rotation = cameraRotation;
                        camera.FieldOfView = _fov;
                        camera.AttachTo(Game.PlayerPed.CurrentVehicle, new Vector3(0f, 0f, -1.5f));
                        RenderScriptCams(true, false, 0, true, false);
                        scaleform.CallFunction("SET_CAM_LOGO", 0);

                        CitizenFX.Core.Vehicle lockedOnVehicle = null;

                        while (_helicam && Game.PlayerPed.IsAlive && IsPlayerInPolmav() && IsHeliHighEnough())
                        {
                            if (Game.IsControlJustPressed(0, _toggleHelicam))
                            {
                                PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                                if (_manualSpotlight && _targetVehicle != null)
                                {
                                    Client.TriggerServerEvent(POLMAV_SPOTLIGHT_TOGGLE);

                                    _trackingSpotlight_pause = false;
                                    _trackingSpotlight = true;

                                    BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TRACKING, Game.Player.ServerId, _targetVehicle.NetworkId, _targetVehicle.Mods.LicensePlate, _targetVehicle.Position.X, _targetVehicle.Position.Y, _targetVehicle.Position.Z);
                                    PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                                }
                                _manualSpotlight = false;
                                _helicam = false;
                            }

                            if (Game.IsControlJustPressed(0, _toggleVision))
                            {
                                PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                                ChangeVision();
                            }

                            if (Game.IsControlJustPressed(0, _toggleSpotlight))
                            {
                                if (_trackingSpotlight)
                                {
                                    _trackingSpotlight_pause = true;
                                    BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_PAUSE, _trackingSpotlight_pause);
                                    _manualSpotlight = !_manualSpotlight;
                                    if (_manualSpotlight)
                                    {
                                        Vector3 rotation = GetCamRot(cam, 2);
                                        Vector3 forwardVector = RotAnglesToVec(rotation);
                                        DecorSetInt(playerPed, DECOR_SPOTLIGHT_X, (int)forwardVector.X);
                                        DecorSetInt(playerPed, DECOR_SPOTLIGHT_Y, (int)forwardVector.Y);
                                        DecorSetInt(playerPed, DECOR_SPOTLIGHT_Z, (int)forwardVector.Z);
                                        PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                                        BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_MANUAL, Game.Player.ServerId);
                                    }
                                    else
                                    {
                                        BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TOGGLE);
                                    }
                                }
                                else if (_forwardSpotlight_state)
                                {
                                    _forwardSpotlight_state = false;
                                    BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_FORWARD, _forwardSpotlight_state);
                                    _manualSpotlight = !_manualSpotlight;
                                    if (_manualSpotlight)
                                    {
                                        PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                                        BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_MANUAL, Game.Player.ServerId);
                                    }
                                    else
                                    {
                                        BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TOGGLE);
                                    }
                                }
                                else
                                {
                                    _manualSpotlight = !_manualSpotlight;
                                    if (_manualSpotlight)
                                    {
                                        PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                                        BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_MANUAL, Game.Player.ServerId);
                                    }
                                    else
                                    {
                                        BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TOGGLE);
                                    }
                                }
                            }

                            if (Game.IsControlJustPressed(0, _keyTurnLightUp))
                            {
                                BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_BRIGHTNESS_UP);
                            }

                            if (Game.IsControlJustPressed(0, _keyTurnLightDown))
                            {
                                BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_BRIGHTNESS_DOWN);
                            }

                            if (Game.IsControlJustPressed(0, _keyTurnLightRadiusUp))
                            {
                                BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_RADIUS_UP);
                            }

                            if (Game.IsControlJustPressed(0, _keyTurnLightRadiusDown))
                            {
                                BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_RADIUS_DOWN);
                            }

                            if (Game.IsControlJustPressed(0, _toggleDisplay))
                            {
                                ChangeDisplay();
                            }

                            if (lockedOnVehicle != null)
                            {
                                if (lockedOnVehicle.Exists())
                                {
                                    camera.PointAt(lockedOnVehicle);
                                    RenderVehicleInfo(lockedOnVehicle);

                                    float targetDistance = Game.PlayerPed.CurrentVehicle.Position.Distance(_targetVehicle.Position);
                                    if (Game.IsControlJustPressed(0, _toggleLockOn) || targetDistance > _maxTargetDistance)
                                    {
                                        PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                                        DecorRemove(_targetVehicle.Handle, DECOR_TARGET);

                                        if (_trackingSpotlight)
                                        {
                                            _trackingSpotlight = false;
                                            BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TRACKING_TOGGLE);
                                        }

                                        _targetVehicle = null;
                                        lockedOnVehicle = null;

                                        camera.StopPointing();
                                    }
                                }
                                else
                                {
                                    lockedOnVehicle = null;
                                    _targetVehicle = null;
                                }
                            }
                            else
                            {
                                _zoomvalue = (float)(1.0 / (_fovMax - _fovMin)) * (_fov - _fovMin);
                                CheckInputRotation(cam, _zoomvalue);
                                CitizenFX.Core.Vehicle vehicleDetected = GetVehicleInView(cam);
                                if (vehicleDetected != null)
                                {
                                    if (vehicleDetected.Exists())
                                    {
                                        RenderVehicleInfo(vehicleDetected);

                                        if (Game.IsControlJustPressed(0, _toggleLockOn))
                                        {
                                            PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
                                            lockedOnVehicle = vehicleDetected;

                                            if (_targetVehicle != null)
                                            {
                                                DecorRemove(_targetVehicle.Handle, DECOR_TARGET);
                                            }

                                            _targetVehicle = lockedOnVehicle;
                                            NetworkRequestControlOfEntity(_targetVehicle.Handle);
                                            SetNetworkIdCanMigrate(_targetVehicle.NetworkId, true);
                                            NetworkRegisterEntityAsNetworked(_targetVehicle.NetworkId);
                                            SetNetworkIdExistsOnAllMachines(_targetVehicle.NetworkId, true);
                                            SetEntityAsMissionEntity(_targetVehicle.Handle, true, true);
                                            DecorSetInt(_targetVehicle.Handle, DECOR_TARGET, 2);

                                            if (_trackingSpotlight)
                                            {
                                                if (!_trackingSpotlight_pause)
                                                {
                                                    _trackingSpotlight_pause = false;
                                                    _trackingSpotlight = true;
                                                    PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);

                                                    BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TRACKING, _targetVehicle.NetworkId, _targetVehicle.Mods.LicensePlate, _targetVehicle.Position.X, _targetVehicle.Position.Y, _targetVehicle.Position.Z);
                                                }
                                                else
                                                {
                                                    _trackingSpotlight_pause = false;
                                                    _trackingSpotlight = false;
                                                }

                                                BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TRACKING_TOGGLE, _trackingSpotlight);
                                            }
                                        }
                                    }
                                }
                            }

                            HandleZoom(cam);
                            HideHudthisFrame();
                            scaleform.CallFunction("SET_ALT_FOV_HEADING", Game.PlayerPed.CurrentVehicle.Position.Z, _zoomvalue, camera.Rotation.Z);
                            scaleform.Render2D();
                            await BaseScript.Delay(0);

                            if (_manualSpotlight)
                            {
                                Vector3 rotation = GetCamRot(cam, 2);
                                Vector3 forwardVector = RotAnglesToVec(rotation);
                                DecorSetInt(playerPed, DECOR_SPOTLIGHT_X, (int)forwardVector.X);
                                DecorSetInt(playerPed, DECOR_SPOTLIGHT_Y, (int)forwardVector.Y);
                                DecorSetInt(playerPed, DECOR_SPOTLIGHT_Z, (int)forwardVector.Z);
                                DrawSpotLight(camera.Position.X, camera.Position.Y, camera.Position.Z, forwardVector.X, forwardVector.Y, forwardVector.Z, 255, 255, 255, 800.0f, _brightness, 10.0f, _spotRadius, 1.0f);
                            }
                            else
                            {
                                Client.TriggerServerEvent(POLMAV_SPOTLIGHT_TOGGLE);
                            }

                        } // WHILE LOOP END

                        if (_manualSpotlight)
                        {
                            _manualSpotlight = false;
                            Client.TriggerServerEvent(POLMAV_SPOTLIGHT_TOGGLE);
                        }
                        _helicam = false;
                        ClearTimecycleModifier();
                        _fov = (float)((_fovMax + _fovMin) * 0.5);
                        RenderScriptCams(false, false, 0, true, false);
                        scaleform.Dispose();
                        camera.Delete();
                        SetNightvision(false);
                        SetSeethrough(false);
                    }

                    if (IsPlayerInPolmav() && _targetVehicle != null && !_helicam && _vehicleDisplay != VehicleDisplay.TURNED_OFF) {
                        RenderVehicleInfo(_targetVehicle);
                    }
                }
            }
        }

        // NetResponse
        static void OnForwardSpotlight(int playerServerId, bool state)
        {
            int playerId = GetPlayerFromServerId(playerServerId);
            int playerPedId = GetPlayerPed(playerId);
            int heli = GetVehiclePedIsIn(playerPedId, false);
            SetVehicleSearchlight(heli, state, false);
        }

        static void OnTrackingSpotlightToggle()
        {
            _trackingSpotlight_toggle = false;
            _trackingSpotlight = false;
        }

        static void OnPauseTrackingSpotlight(bool pause)
        {
            _trackingSpotlight_pause = pause;
        }

        static async void OnTrackingSpotLight(int playerServerId, int targetVehicleId, string targetLicensePlate, float targetPosX, float targetPosY, float targetPosZ)
        {
            int vehicleEntityHandle = NetToVeh(targetVehicleId);
            string vehicleLicensePlate = GetVehicleNumberPlateText(vehicleEntityHandle);

            int closestVehicle = GetClosestVehicle(targetPosX, targetPosY, targetPosZ, 25f, 0, 70);
            string closestVehicleLicensePlate = GetVehicleNumberPlateText(vehicleEntityHandle);

            if (vehicleLicensePlate == targetLicensePlate)
            {
                _trackingSpotlight_target = vehicleEntityHandle;
            }
            else if (closestVehicleLicensePlate == targetLicensePlate)
            {
                _trackingSpotlight_target = closestVehicle;
            }
            else
            {
                CitizenFX.Core.Vehicle vehicleMatch = FindVehicleByPlate(targetLicensePlate);
                if (vehicleMatch != null)
                {
                    _trackingSpotlight_target = vehicleMatch.Handle;
                }
                else
                {
                    _trackingSpotlight_target = 0;
                }
            }

            int playerId = GetPlayerFromServerId(playerServerId);
            int playerPedId = GetPlayerPed(playerId);
            int heli = GetVehiclePedIsIn(playerPedId, false);

            _trackingSpotlight_toggle = true;
            _trackingSpotlight_pause = false;
            _trackingSpotlight = true;

            while (!IsEntityDead(playerPedId) && GetVehiclePedIsIn(playerPedId, false) == heli && _trackingSpotlight_target > 0 && _trackingSpotlight_toggle)
            {
                await Client.Delay(1);
                Vector3 heliCoords = GetEntityCoords(heli, true);
                Vector3 targetCoords = GetEntityCoords(_trackingSpotlight_target, true);
                Vector3 spotlightVector = targetCoords - heliCoords;
                float targetDistance = Vdist(targetCoords.X, targetCoords.Y, targetCoords.Z, heliCoords.X, heliCoords.Y, heliCoords.Z);

                if (_trackingSpotlight_target > 0 && _trackingSpotlight_toggle && !_trackingSpotlight_pause)
                {
                    DrawSpotLight(heliCoords.X, heliCoords.Y, heliCoords.Z, spotlightVector.X, spotlightVector.Y, spotlightVector.Z, 255, 255, 255, (targetDistance + 20f), _brightness, 10.0f, 4.0f, 1.0f);
                }

                if (_trackingSpotlight_target > 0 && _trackingSpotlight_toggle && targetDistance > _maxTargetDistance)
                {
                    // tracking lost
                    DecorRemove(_trackingSpotlight_target, DECOR_TARGET);
                    _targetVehicle = null;
                    _trackingSpotlight = false;
                    BaseScript.TriggerServerEvent(POLMAV_SPOTLIGHT_TRACKING_TOGGLE);
                    _trackingSpotlight_target = 0;
                    break;
                }
            }
        }

        static async void OnManualSpotlight(int playerServerId)
        {
            if (Game.Player.ServerId != playerServerId)
            {
                int playerId = GetPlayerFromServerId(playerServerId);
                int playerPedId = GetPlayerPed(playerId);
                int heli = GetVehiclePedIsIn(playerPedId, false);

                _manualSpotlight_toggle = true;

                while (!IsEntityDead(playerPedId) && GetVehiclePedIsIn(playerPedId, false) == heli && _manualSpotlight_toggle)
                {
                    await BaseScript.Delay(0);
                    Vector3 coords = GetEntityCoords(heli, true);
                    Vector3 spotlightOffset = coords + new Vector3(0f, 0f, -1.5f);
                    int SpotvectorX = DecorGetInt(playerPedId, DECOR_SPOTLIGHT_X);
                    int SpotvectorY = DecorGetInt(playerPedId, DECOR_SPOTLIGHT_Y);
                    int SpotvectorZ = DecorGetInt(playerPedId, DECOR_SPOTLIGHT_Z);
                    if (SpotvectorX > 0)
                    {
                        DrawSpotLight(SpotvectorX, SpotvectorY, SpotvectorZ, SpotvectorX, SpotvectorY, SpotvectorZ, 255, 255, 255, 800.0f, _brightness, 10.0f, _spotRadius, 1.0f);
                    }
                }
                _manualSpotlight_toggle = false;
                DecorSetInt(playerPedId, DECOR_SPOTLIGHT_X, 0);
                DecorSetInt(playerPedId, DECOR_SPOTLIGHT_Y, 0);
                DecorSetInt(playerPedId, DECOR_SPOTLIGHT_Z, 0);
            }
        }

        static void OnManualSpotlightToggle(int playerServerId)
        {
            _manualSpotlight_toggle = false;
        }

        static void OnSpotlightLightUp()
        {
            if (_brightness < 10)
            {
                _brightness = (float)(_brightness + 1.0);
                PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        static void OnSpotlightLightDown()
        {
            if (_brightness > 1.0)
            {
                _brightness = (float)(_brightness - 1.0);
                PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        static void OnSpotlightRadiusUp()
        {
            if (_spotRadius < 10)
            {
                _spotRadius = (float)(_spotRadius + 1.0);
                PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        static void OnSpotlightRadiusDown()
        {
            if (_spotRadius > 4.0)
            {
                _spotRadius = (float)(_spotRadius - 1.0);
                PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            }
        }

        private static void OnTrackingSpotLightToggle(int playerServerId, bool trackingSpotlightState)
        {
            _trackingSpotlight_toggle = trackingSpotlightState;
        }

        static bool IsPlayerInPolmav()
        {
            if (!Game.PlayerPed.IsInVehicle()) return false;

            return (Game.PlayerPed.CurrentVehicle.Model.Hash == (int)_polmav);
        }

        static bool IsHeliHighEnough()
        {
            if (!Game.PlayerPed.IsInVehicle()) return false;

            return Game.PlayerPed.CurrentVehicle.HeightAboveGround > 1.5f;
        }

        static void ChangeVision()
        {
            if (_visionState == VisionState.NORMAL)
            {
                SetNightvision(true);
                _visionState = VisionState.NIGHT_MODE;
            }
            else if (_visionState == VisionState.NIGHT_MODE)
            {
                SetNightvision(false);
                SetSeethrough(true);
                _visionState = VisionState.THERMAL_VISION;
            }
            else
            {
                SetSeethrough(false);
                _visionState = VisionState.NORMAL;
            }
        }

        static void ChangeDisplay()
        {
            if (_vehicleDisplay == VehicleDisplay.FULL_INFO)
            {
                _vehicleDisplay = VehicleDisplay.MODEL_PLATE;
            }
            else if (_vehicleDisplay == VehicleDisplay.MODEL_PLATE)
            {
                _vehicleDisplay = VehicleDisplay.TURNED_OFF;
            }
            else
            {
                _vehicleDisplay = VehicleDisplay.FULL_INFO;
            }
        }

        static void HideHudthisFrame()
        {
            HideHelpTextThisFrame();
            HideHudAndRadarThisFrame();
            HideHudComponentThisFrame(19); // weapon wheel
            HideHudComponentThisFrame(1); // Wanted Stars
            HideHudComponentThisFrame(2); // Weapon icon
            HideHudComponentThisFrame(3); // Cash
            HideHudComponentThisFrame(4); // MP CASH
            HideHudComponentThisFrame(13); // Cash Change
            HideHudComponentThisFrame(11); // Floating Help Text
            HideHudComponentThisFrame(12); // more floating help text
            HideHudComponentThisFrame(15); // Subtitle Text
            HideHudComponentThisFrame(18); // Game Stream
        }

        static void CheckInputRotation(int cameraHandle, float zoomValue)
        {
            float rightAxisX = GetDisabledControlNormal(0, (int)Control.ScriptRightAxisX);
            float rightAxisY = GetDisabledControlNormal(0, (int)Control.ScriptRightAxisY);
            Vector3 roatation = GetCamRot(cameraHandle, 2);

            if (rightAxisX != 0.0 || rightAxisY != 0.0)
            {
                float newZ = (float)(roatation.Z + rightAxisX * -1.0f * (_speedUD) * (zoomValue + 0.1));
                float newX = (float)Math.Max(Math.Min(20.0, roatation.X + rightAxisY*-1.0*(_speedLR)*(zoomValue+0.1)), -89.5);
                SetCamRot(cameraHandle, newX, 0.0f, newZ, 2);
            }
        }

        static void HandleZoom(int cameraHandle)
        {
            if (Game.IsControlJustPressed(0, Control.CursorScrollUp))
            {
                _fov = Math.Max(_fov - _zoomspeed, _fovMin);
            }
            if (Game.IsControlJustPressed(0, Control.CursorScrollDown))
            {
                _fov = Math.Max(_fov + _zoomspeed, _fovMax);
            }
            float currentFov = GetCamFov(cameraHandle);
            if (Math.Abs(_fov-currentFov) < 0.1)
            {
                _fov = currentFov;
            }
            SetCamFov(cameraHandle, (float)(currentFov + (_fov - currentFov) * 0.05));
        }

        static CitizenFX.Core.Vehicle GetVehicleInView(int cameraHandle)
        {
            Vector3 coords = GetCamCoord(cameraHandle);
            Vector3 forwardVector = RotAnglesToVec(GetCamRot(cameraHandle, 2));

            Vector3 endPoint = coords + (forwardVector * 200);

            int raycastHandle = CastRayPointToPoint(coords.X, coords.Y, coords.Z, endPoint.X, endPoint.Y, endPoint.Z, 10, Game.PlayerPed.CurrentVehicle.Handle, 0);
            
            bool hit = false;
            Vector3 endCoords = Vector3.Zero;
            Vector3 surfaceNormalCoords = Vector3.Zero;
            int entityHit = 0;

            GetRaycastResult(raycastHandle, ref hit, ref endCoords, ref surfaceNormalCoords, ref entityHit);

            if (entityHit > 0 && IsEntityAVehicle(entityHit))
            {
                return new CitizenFX.Core.Vehicle(entityHit);
            }
            else
            {
                return default;
            }
        }

        static void RenderVehicleInfo(CitizenFX.Core.Vehicle vehicle)
        {
            if (vehicle == null) return;

            if (!vehicle.Exists()) return;

            double vehSpeed = 0.0;

            Model model = vehicle.Model;
            string vehName = vehicle.DisplayName;
            string licensePlate = vehicle.Mods.LicensePlate;
            if (_speedMessure == "mph")
            {
                vehSpeed = (vehicle.Speed * 2.236936);
            }
            else
            {
                vehSpeed = (vehicle.Speed * 3.6); // Km/h
            }
            SetTextFont(0);
            SetTextProportional(true);
            if (_vehicleDisplay == VehicleDisplay.FULL_INFO)
            {
                SetTextScale(0.0f, 0.49f);
            }
            else if (_vehicleDisplay == VehicleDisplay.MODEL_PLATE)
            {
                SetTextScale(0.0f, 0.55f);
            }
            SetTextColour(255, 255, 255, 255);
            SetTextDropshadow(0, 0, 0, 0, 255);
            SetTextEdge(1, 0, 0, 0, 255);
            SetTextDropShadow();
            SetTextOutline();
            SetTextEntry("STRING");
            if (_vehicleDisplay == VehicleDisplay.FULL_INFO)
            {
                AddTextComponentString($"Speed: {Math.Ceiling(vehSpeed)} {_speedMessure}\nModel: {vehName}\nPlate: {licensePlate}");
            }
            else if (_vehicleDisplay == VehicleDisplay.MODEL_PLATE)
            {
                AddTextComponentString($"Model: {vehName}\nPlate: {licensePlate}");
            }
            DrawText(0.45f, 0.9f);
        }

        // MATHS
        static Vector3 RotAnglesToVec(Vector3 rotation)
        {
            double z = RadFromDeg(rotation.Z);
            double x = RadFromDeg(rotation.X);
            double num = Math.Abs(Math.Cos(x));
            return new Vector3((float)(-Math.Sin(z) * num), (float)(Math.Cos(z) * num), (float)Math.Sin(x));
        }

        static double RadFromDeg(double degrees)
        {
            return degrees * MATH_PI / 180.0f;
        }

        static double DegFromRad(double radians)
        {
            return radians * 180.0 / MATH_PI;
        }

        static CitizenFX.Core.Vehicle FindVehicleByPlate(string licensePlate)
        {
            CitizenFX.Core.Vehicle veh = default;

            World.GetAllVehicles().ToList().ForEach(vehicle =>
            {
                if (vehicle.Mods.LicensePlate == licensePlate)
                {
                    veh = vehicle;
                }
            });

            return veh;
        }
    }
}
