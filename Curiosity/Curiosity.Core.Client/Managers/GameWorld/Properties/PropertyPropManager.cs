using CitizenFX.Core.UI;
using Curiosity.Core.Client.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties
{
    public class PropMemory
    {
        public Vector3 Position;
        public Vector3 Rotation;

        public PropMemory(Prop prop)
        {
            Position = prop.Position;
            Rotation = prop.Rotation;
        }

        public PropMemory(Vector3 pos, Vector3 rot)
        {
            Position = pos;
            Rotation = rot;
        }
    }

    public class PropertyPropManager : Manager<PropertyPropManager>
    {
        int _currentInterior = 0;

        static List<string> _emitters = new()
        {
            "se_mp_apt_1_1",
            "se_mp_apt_10_1",
            "se_mp_apt_11_1",
            "se_mp_apt_12_1",
            "se_mp_apt_13_1",
            "se_mp_apt_14_1",
            "se_mp_apt_15_1",
            "se_mp_apt_16_1",
            "se_mp_apt_17_1",
            "se_mp_apt_2_1",
            "se_mp_apt_3_1",
            "se_mp_apt_4_1",
            "se_mp_apt_5_1",
            "se_mp_apt_6_1",
            "se_mp_apt_7_1",
            "se_mp_apt_8_1",
            "se_mp_apt_9_1",
            "se_mp_apt_1_2",
            "se_mp_apt_10_2",
            "se_mp_apt_11_2",
            "se_mp_apt_12_2",
            "se_mp_apt_13_2",
            "se_mp_apt_14_2",
            "se_mp_apt_15_2",
            "se_mp_apt_16_2",
            "se_mp_apt_17_2",
            "se_mp_apt_2_2",
            "se_mp_apt_3_2",
            "se_mp_apt_4_2",
            "se_mp_apt_5_2",
            "se_mp_apt_6_2",
            "se_mp_apt_7_2",
            "se_mp_apt_8_2",
            "se_mp_apt_9_2",
            "se_mp_apt_1_3",
            "se_mp_apt_10_3",
            "se_mp_apt_11_3",
            "se_mp_apt_12_3",
            "se_mp_apt_13_3",
            "se_mp_apt_14_3",
            "se_mp_apt_15_3",
            "se_mp_apt_16_3",
            "se_mp_apt_17_3",
            "se_mp_apt_3_3",
            "se_mp_apt_4_3",
            "se_mp_apt_5_3",
            "se_mp_apt_6_3",
            "se_mp_apt_7_3",
            "se_mp_apt_8_3",
            "se_mp_apt_9_3",
            "se_mp_apt_2_3",
            "se_mp_apt_new_1_2",
            "se_mp_apt_new_2_2",
            "se_mp_apt_new_3_2",
            "se_mp_apt_new_4_2",
            "se_mp_apt_new_5_2",
            "se_mp_apt_new_1_1",
            "se_mp_apt_new_2_1",
            "se_mp_apt_new_3_1",
            "se_mp_apt_new_4_1",
            "se_mp_apt_new_5_1",
            "se_mp_apt_new_4_3",
            "se_mp_apt_new_1_3",
            "se_mp_apt_new_2_3",
            "se_mp_apt_new_3_3",
            "se_mp_apt_new_5_3",
        };

        static Prop _closestProp = null;
        static List<Model> _closestProps;
        
        static List<Model> _televisionModels;
        static bool _isTelevisionOn;
        static bool _isTelevisionRemoteActive;
        static bool _isSubtitleOn;
        static float _televisionVolume;
        static int _teleivisionChannel;
        static int _RenderTargetId;
        static int _exRenderTargetId;
        static Camera _televisionCamera;
        static List<int> _televisionChannels;
        
        static Scaleform _scaleform;
        
        static List<Model> _radioModels;
        static bool _isRadioOn;
        static int _radioChannel;
        static Prop _radioEmitter;

        static List<Model> _bongModels;
        static int _bongTaskState;
        static int _bongState;
        static Prop _bongProp;
        static Prop _lighterProp;
        static PropMemory _bongPropMemory;
        static PropMemory _lighterPropMemory;

        public override void Begin()
        {
            Init();
        }

        void Init()
        {
            List<Model> modelList1 = new List<Model>();
            modelList1.Add(new Model("prop_tv_flat_01"));
            modelList1.Add(new Model("hei_heist_str_avunitl_03"));
            modelList1.Add(new Model("apa_mp_h_str_avunitm_03"));
            modelList1.Add(new Model("apa_mp_h_str_avunits_01"));
            modelList1.Add(new Model("apa_mp_h_str_avunits_04"));
            modelList1.Add(new Model("prop_tv_03"));
            modelList1.Add(new Model("apa_mp_h_str_avunitm_01"));
            modelList1.Add(new Model("apa_mp_h_str_avunitl_01_b"));
            modelList1.Add(new Model("apa_mp_h_str_avunitl_04"));
            modelList1.Add(new Model("ex_prop_ex_tv_flat_01"));
            modelList1.Add(new Model("v_res_mm_audio"));
            modelList1.Add(new Model("prop_mp3_dock"));
            modelList1.Add(new Model("prop_boombox_01"));
            modelList1.Add(new Model("prop_tapeplayer_01"));
            modelList1.Add(new Model("v_res_fh_speakerdock"));
            modelList1.Add(new Model("prop_radio_01"));
            modelList1.Add(new Model("prop_bong_01"));
            _closestProps = modelList1;
            List<Model> modelList2 = new List<Model>();
            modelList2.Add(new Model("prop_tv_flat_01"));
            modelList2.Add(new Model("hei_heist_str_avunitl_03"));
            modelList2.Add(new Model("apa_mp_h_str_avunitm_03"));
            modelList2.Add(new Model("apa_mp_h_str_avunits_01"));
            modelList2.Add(new Model("apa_mp_h_str_avunits_04"));
            modelList2.Add(new Model("prop_tv_03"));
            modelList2.Add(new Model("apa_mp_h_str_avunitm_01"));
            modelList2.Add(new Model("apa_mp_h_str_avunitl_01_b"));
            modelList2.Add(new Model("apa_mp_h_str_avunitl_04"));
            modelList2.Add(new Model("ex_prop_ex_tv_flat_01"));
            _televisionModels = modelList2;
            _isTelevisionOn = false;
            _isTelevisionRemoteActive = false;
            _isSubtitleOn = false;
            _televisionVolume = 0.0f;
            _teleivisionChannel = 0;
            _televisionChannels = new List<int>() { 0, 1 };
            _scaleform = new Scaleform("instructional_buttons");
            List<Model> modelList3 = new List<Model>();
            modelList3.Add(new Model("v_res_mm_audio"));
            modelList3.Add(new Model("prop_mp3_dock"));
            modelList3.Add(new Model("prop_boombox_01"));
            modelList3.Add(new Model("prop_tapeplayer_01"));
            modelList3.Add(new Model("v_res_fh_speakerdock"));
            modelList3.Add(new Model("prop_radio_01"));
            modelList3.Add(new Model("prop_player_phone_01"));
            _radioModels = modelList3;
            _isRadioOn = false;
            _radioChannel = 0;
            _radioEmitter = null;
            List<Model> modelList4 = new List<Model>();
            modelList4.Add(new Model("prop_bong_01"));
            _bongModels = modelList4;
            _bongTaskState = -1;
            _bongState = 1;
            _bongProp = null;
            _lighterProp = null;
        }

        [TickHandler]
        private async Task OnPropertyTaskManager()
        {
            int interior = GetInteriorFromEntity(Game.PlayerPed.Handle);
            if (_currentInterior != interior)
            {
                _currentInterior = interior;
                if (_currentInterior == 0)
                {
                    // turn off
                    Instance.DetachTickHandler(PropOnTick);
                }
                if (_currentInterior > 0)
                {
                    // turn on
                    Instance.AttachTickHandler(PropOnTick);
                    Init();
                }
            }
        }

        public static async Task PropOnTick()
        {
            Vector3 position = Game.PlayerPed.Position;
            Prop[] nearbyProps = World.GetAllProps();
            _closestProp = World.GetClosest<Prop>(position, nearbyProps);
            
            // await TellyTick();
            await RadioTick();
            // await BongTick(); // Needs performance edits to work in C#
        }

        public static void DrawTVControlInstructionalButtons()
        {
            _scaleform.CallFunction("CLEAR_ALL", new object[0]);
            _scaleform.CallFunction("CREATE_CONTAINER", new object[0]);
            _scaleform.CallFunction("SET_DATA_SLOT", new object[3] { 0, GetControlInstructionalButton(2, 218, 1), Game.GetGXTEntry("HUD_INPUT75") });
            _scaleform.CallFunction("SET_DATA_SLOT", new object[3] { 1, GetControlInstructionalButton(2, 222, 1), Game.GetGXTEntry("HUD_INPUT69") });
            _scaleform.CallFunction("SET_DATA_SLOT", new object[3] { 2, GetControlInstructionalButton(2, 51, 1), Game.GetGXTEntry("HUD_INPUT82")});
            _scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", new object[1] { -1 });
            _scaleform.Render2D();
        }

        public static async void TurnOffTV(string tvscreen)
        {
            SetTvChannel(0);
            await BaseScript.Delay(500);
            if (!IsNamedRendertargetRegistered(tvscreen)) return;
            ReleaseNamedRendertarget(tvscreen);
        }

        public static void RenderTV(int renderId)
        {
            SetTextRenderId(renderId);
            SetScriptGfxDrawOrder(4);
            SetScriptGfxDrawBehindPausemenu(true);
            DrawTvChannel(.5f, .5f, 1f, 1f, 0f, 255, 255, 255, 255);
            SetTextRenderId(GetDefaultScriptRendertargetRenderId());
        }

        // Create TV

        public static async Task TellyTick()
        {
            if (_closestProp.IsPropInList(_televisionModels))
            {
                Vector3 position = _closestProp.Position;
                if (Game.PlayerPed.IsInRangeOf(position, 4.0f))
                {
                    if (!_isTelevisionOn)
                    {
                        Screen.DisplayHelpTextThisFrame(Game.GetGXTEntry("MPTV_GRGE"));
                        if (Game.IsControlJustPressed(0, (Control)51))
                        {
                            _RenderTargetId = await _closestProp.TurnOnTV("tvscreen", _teleivisionChannel, _televisionVolume);
                            _exRenderTargetId = await _closestProp.TurnOnTV("ex_tvscreen", _teleivisionChannel, _televisionVolume);
                            _isTelevisionOn = true;
                            return;
                        }
                    }
                    else
                    {
                        if (!_isTelevisionRemoteActive)
                            Screen.DisplayHelpTextThisFrame(Game.GetGXTEntry("TV_HLP5"));
                        if (Game.IsControlJustPressed(0, (Control)51))
                        {
                            TurnOffTV("tvscreen");
                            TurnOffTV("ex_tvscreen");
                            _isTelevisionOn = false;
                            _isTelevisionRemoteActive = false;
                            await BaseScript.Delay(0);
                        }
                        else if (Game.IsControlJustPressed(0, (Control)222) && !_isTelevisionRemoteActive)
                        {
                            _isTelevisionRemoteActive = true;
                            return;
                        }
                    }
                    if (_isTelevisionRemoteActive)
                    {
                        if (Game.IsControlJustPressed(0, (Control)218))
                        {
                            switch (_teleivisionChannel)
                            {
                                case 0:
                                    _teleivisionChannel = 1;
                                    break;
                                case 1:
                                    _teleivisionChannel = 0;
                                    break;
                            }
                            SetTvChannel(_teleivisionChannel);
                            return;
                        }
                        else if (Game.IsControlJustPressed(0, (Control)222) && _isTelevisionRemoteActive)
                        {
                            _isTelevisionRemoteActive = false;
                            return;
                        }
                    }
                }
                else if (_isTelevisionRemoteActive)
                    _isTelevisionRemoteActive = false;
            }
            if (_isTelevisionOn)
            {
                if (_closestProp.IsPropInList(_televisionModels))
                    Interface.ScreenInterface.Draw3DText(_closestProp.Position, $"{_RenderTargetId}", 30);
                RenderTV(_RenderTargetId);
                RenderTV(_exRenderTargetId);
            }
            if (_isTelevisionRemoteActive)
                DrawTVControlInstructionalButtons();
            if (!Equals(_closestProp, null))
                return;
            _isTelevisionRemoteActive = false;
        }

        public static async Task TurnOnRadio()
        {
            _radioChannel = GetPlayerRadioStationIndex();
            UpdateRadio((RadioStation)_radioChannel);

            foreach (string em in _emitters)
                SetStaticEmitterEnabled(em, true);
        }

        private static void TurnOffRadio()
        {
            foreach (string em in _emitters)
            {
                SetEmitterRadioStation(em, "OFF");
                SetStaticEmitterEnabled(em, false);
            }
        }

        public static void UpdateRadio(RadioStation station)
        {
            if (station == (RadioStation)22)
                station = 0;
            if (station == (RadioStation)256)
                station = 0;
            _radioChannel = (int)station;

            string radioStationName = GetRadioStationName(_radioChannel);

            foreach(string em in _emitters)
                SetEmitterRadioStation(em, radioStationName);
        }

        public static void DrawRadioControlInstructionalButtons()
        {
            _scaleform.CallFunction("CLEAR_ALL", new object[0]);
            _scaleform.CallFunction("CREATE_CONTAINER", new object[0]);
            _scaleform.CallFunction("SET_DATA_SLOT", new object[3] { 0, GetControlInstructionalButton(2, 222, 1), string.Format("{0} ({1})", Game.GetGXTEntry("HUD_INPUT80"), Game.GetGXTEntry(GetRadioStationName(_radioChannel))) });
            _scaleform.CallFunction("SET_DATA_SLOT", new object[3] { 1, GetControlInstructionalButton(2, 51, 1), Game.GetGXTEntry("HUD_INPUT82") });
            _scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", new object[1] { -1 });
            _scaleform.Render2D();
        }

        public static async Task RadioTick()
        {
            if (!_closestProp.IsPropInList(_radioModels))
                return;
            if (!Equals(_radioEmitter, null))
                _radioEmitter.Position = _closestProp.Position;

            Vector3 position = _closestProp.Position;

            if (!Game.PlayerPed.IsInRangeOf(position, 2.0f))
                return;

            if (!_isRadioOn)
            {
                Screen.DisplayHelpTextThisFrame(Game.GetGXTEntry("MPRD_CTXT"));
                if (!Game.IsControlJustPressed(0, (Control)51))
                    return;
                TurnOnRadio();
                UpdateRadio((RadioStation)_radioChannel);
                _isRadioOn = true;
                await BaseScript.Delay(0);
            }
            else
            {
                DrawRadioControlInstructionalButtons();
                if (Game.IsControlJustPressed(0, (Control)222))
                {
                    UpdateRadio((RadioStation)checked(_radioChannel + 1));
                    await BaseScript.Delay(0);
                }
                else
                {
                    if (!Game.IsControlJustPressed(0, (Control)51))
                        return;
                    TurnOffRadio();
                    UpdateRadio((RadioStation)(int)byte.MaxValue);
                    _isRadioOn = false;
                    await BaseScript.Delay(0);
                }
            }
        }

        public static async Task BongTick()
        {
            if (_closestProp.IsPropInList(_bongModels))
            {
                Vector3 position = (_closestProp).Position;
                if (Game.PlayerPed.IsInRangeOf(position, 2.0f) && _bongTaskState == -1)
                {
                    Screen.DisplayHelpTextThisFrame(Game.GetGXTEntry("SA_BONG2"));
                    if (Game.IsControlJustPressed(0, (Control)51))
                    {
                        _bongProp = _closestProp;
                        _bongPropMemory = new PropMemory(_bongProp);
                        _lighterProp = new Prop(GetClosestObjectOfType(_bongProp.Position.X, _bongProp.Position.Y, _bongProp.Position.Z, 10f, (uint)GetHashKey("p_cs_lighter_01"), false, false, true));
                        _lighterPropMemory = new PropMemory(_lighterProp);
                        _bongTaskState = 0;
                    }
                }
            }
            switch (_bongTaskState)
            {
                case 0:
                    if (_bongState > 4)
                    {
                        _bongState = 1;
                        Game.PlayerPed.Task.PlayAnimation("anim@safehouse@bong", string.Format("bong_stage{0}", (object)_bongState));
                    }
                    else
                    {
                        Game.PlayerPed.Task.PlayAnimation("anim@safehouse@bong", string.Format("bong_stage{0}", (object)_bongState));
                        checked { ++_bongState; }
                    }
                    await BaseScript.Delay(1500);

                    _bongProp.AttachTo(Game.PlayerPed, Game.PlayerPed.Bones[60309].Position, Vector3.Zero);
                    _lighterProp.AttachTo(Game.PlayerPed, Game.PlayerPed.Bones[28422].Position, new Vector3(0.0f, 0.0f, -0.05f));
                    _bongTaskState = 1;
                    break;
                case 1:
                    if (!_bongProp.IsAttachedTo(Game.PlayerPed))
                        break;
                    await BaseScript.Delay(10000);
                    _bongProp.Detach();
                    _bongProp.Position = _bongPropMemory.Position;
                    _bongProp.Rotation = _bongPropMemory.Rotation;
                    _lighterProp.Detach();
                    _lighterProp.Position = _lighterPropMemory.Position;
                    _lighterProp.Rotation = _lighterPropMemory.Rotation;
                    _bongTaskState = 2;
                    break;
                case 2:
                    StartScreenFx("DrugsDrivingIn", 5000, false, true);
                    StartScreenFx("DrugsDrivingOut", 5000, false, true);
                    _bongProp = null;
                    _lighterProp = null;
                    _bongTaskState = -1;
                    break;
            }
        }

        static async Task StartScreenFx(string fxName, int duration, bool looped, bool stopAfterDuration)
        {
            AnimpostfxPlay(fxName, duration, looped);
            await BaseScript.Delay(duration);
            if (!stopAfterDuration)
                return;
            AnimpostfxStop(fxName);
        }
    }
}
