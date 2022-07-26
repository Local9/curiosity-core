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
            "se_dlc_apt_yacht_bar",
            "se_dlc_apt_yacht_bedroom",
            "se_dlc_apt_yacht_exterior_01",
            "se_dlc_apt_yacht_exterior_02",
            "se_dlc_apt_yacht_exterior_03",
            "se_dlc_apt_yacht_exterior_04",
            "se_dlc_apt_yacht_bedroom_02",
            "se_dlc_apt_yacht_bedroom_03",
            "se_dlc_apt_custom_bedroom",
            "se_dlc_apt_custom_heist_room",
            "se_dlc_apt_custom_living_room",
            "se_dlc_apt_stilts_a_living_room",
            "se_dlc_apt_stilts_a_bedroom",
            "se_dlc_apt_stilts_a_heist_room",
            "se_dlc_apt_stilts_b_living_room",
            "se_dlc_apt_stilts_b_heist_room",
            "se_dlc_apt_stilts_b_bedroom",
            "se_mp_ap_rad_v_apart_midspaz_lounge",
            "se_mp_ap_rad_v_studio_lo_living",
            "se_mp_garage_m_radio",
            "SE_MP_GARAGE_S_RADIO",
            "SE_MP_GARAGE_L_RADIO",
            "SE_Script_Placed_Prop_Emitter_Boombox",
            "dlc_h3_arcade_main_area_music_emitter",
            "dlc_h3_arcade_planning_room_radio_emitter",
            "se_h4_dlc_int_02_h4_main_front_01",
            "se_h4_dlc_int_02_h4_main_front_02",
            "se_h4_dlc_int_02_h4_main_bar",
            "se_h4_dlc_int_02_h4_lobby",
            "se_h4_dlc_int_02_h4_bogs",
            "se_h4_dlc_int_02_h4_entrance_doorway",
            "se_h4_dlc_int_02_h4_main_room_cutscenes",
            "se_sf_dlc_garage_sec_room_f1_01",
            "se_sf_dlc_garage_sec_room_f2_01",
            "se_sf_dlc_garage_sec_room_gf_01",
            "se_sf_dlc_garage_sec_room_stairs_01",
            "se_sf_dlc_office_sec_apartment_01",
            "se_sf_dlc_office_sec_hacker_01",
            "se_sf_dlc_office_sec_backstairs_01",
            "se_sf_dlc_office_sec_main_01",
            "se_sf_dlc_office_sec_franklin_office",
            "se_sf_dlc_office_sec_player_office",
            "se_sf_dlc_studio_sec_studio_01",
            "se_sf_dlc_studio_sec_writers_01",
            "se_sf_dlc_studio_sec_studio_02",
            "se_sf_dlc_studio_sec_lobby_01",
            "se_dlc_sec_weed_warehouse_radio",
        };

        static Prop _closestProp = null;        
        static Scaleform _scaleform;
        
        static List<Model> _radioModels;
        static bool _isRadioOn;
        static int _radioChannel;

        public override void Begin()
        {
            Init();
        }

        void Init()
        {
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
    }
}
