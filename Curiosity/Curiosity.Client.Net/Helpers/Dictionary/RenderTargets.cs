using System.Collections.Generic;

namespace Curiosity.Client.net.Helpers.Dictionary
{
    class RenderTargets
    {
        public static Dictionary<string, List<string>> targets = new Dictionary<string, List<string>>();

        public static void Init()
        {
            targets.Add("tvscreen", new List<string> {
                "des_tvsmash_start",
                "des_tvsmash_root",
                "des_tvsmash_end",
                "prop_cs_tv_stand",
                "prop_flatscreen_overlay",
                "prop_laptop_lester2",
                "prop_monitor_02",
                "prop_trev_tv_01",
                "prop_tv_02",
                "prop_tv_03_overlay",
                "prop_tv_06",
                "prop_tv_flat_01",
                "prop_tv_flat_01_screen",
                "prop_tv_flat_02b",
                "prop_tv_flat_03",
                "prop_tv_flat_03b",
                "prop_tv_flat_michael",
                "prop_monitor_w_large",
                "prop_tv_03",
                "prop_tv_flat_02",
                "hei_prop_hst_laptop",
                "hei_bank_heist_laptop",
                "hei_heist_str_avunitl_03",
                "hei_heist_str_avunits_01",
                "hei_heist_str_avunitl_01"
            });
            targets.Add("prop_x17dlc_monitor_wall_01a", new List<string> {
                "xm_prop_x17dlc_monitor_wall_01a"
            });
            targets.Add("cinscreen", new List<string> {
                "prop_big_cin_screen"
            });
            targets.Add("taxi", new List<string> {
                "prop_taxi_meter_1",
                "prop_taxi_meter_2"
            });
            targets.Add("rev_phone", new List<string> {
                "prop_cs_trev_overlay"
            });
            targets.Add("npcphone", new List<string> {
                "prop_phone_cs_frank",
                "prop_phone_proto"
            });
            targets.Add("big_disp", new List<string> {
                "prop_huge_display_01",
                "prop_huge_display_02"
            });
            targets.Add("planning", new List<string> {
                "prop_muster_wboard_01",
                "prop_muster_wboard_02",
                "hei_prop_hei_muster_01"
            });
            targets.Add("prop_battle_touchscreen_rt", new List<string> {
                "ba_prop_battle_hacker_screen"
            });
            targets.Add("prop_x17_p_01", new List<string> {
                "xm_prop_x17_sec_panel_01"
            });
            targets.Add("slow_text", new List<string> {
                "bkr_prop_slow_down"
            });
            targets.Add("prop_clubhouse_laptop_01a", new List<string> {
                "bkr_prop_clubhouse_laptop_01a"
            });
            targets.Add("prop_clubhouse_laptop_square_01a", new List<string> {
                "bkr_prop_clubhouse_laptop_01b"
            });
            targets.Add("big_disp2", new List<string> {
                "prop_projector_overlay"
            });
            targets.Add("pbus_screen", new List<string> {
                "ba_prop_battle_pbus_screen"
            });
            targets.Add("club_computer", new List<string> {
                "ba_prop_battle_club_computer_01"
            });
            targets.Add("club_computer_02", new List<string> {
                "ba_prop_battle_club_computer_02"
            });
            targets.Add("laptop_dj", new List<string> {
                "ba_prop_club_laptop_dj"
            });
            targets.Add("laptop_dj_02", new List<string> {
                "ba_prop_club_laptop_dj_02"
            });
            targets.Add("bikerlogo", new List<string> {
                "bkr_prop_biker_scriptrt_logo"
            });
            targets.Add("bikertable", new List<string> {
                "bkr_prop_biker_scriptrt_table"
            });
            targets.Add("bikerwall", new List<string> {
                "bkr_prop_biker_scriptrt_wall"
            });
            targets.Add("clubname_blackboard_01a", new List<string> {
                "bkr_prop_clubhouse_blackboard_01a"
            });
            targets.Add("memorial_wall_active_01", new List<string> {
                "bkr_prop_rt_memorial_active_01"
            });
            targets.Add("memorial_wall_active_02", new List<string> {
                "bkr_prop_rt_memorial_active_02"
            });
            targets.Add("memorial_wall_active_03", new List<string> {
                "bkr_prop_rt_memorial_active_03"
            });
            targets.Add("memorial_wall_president", new List<string> {
                "bkr_prop_rt_memorial_president"
            });
            targets.Add("memorial_wall_vice_president", new List<string> {
                "bkr_prop_rt_memorial_vice_pres"
            });
            targets.Add("clubhouse_plan_01a", new List<string> {
                "bkr_prop_rt_clubhouse_plan_01a"
            });
            targets.Add("clubhouse_table", new List<string> {
                "bkr_prop_rt_clubhouse_table"
            });
            targets.Add("clubhouse_wall", new List<string> {
                "bkr_prop_rt_clubhouse_wall"
            });
            targets.Add("smug_monitor_01", new List<string> {
                "xm_prop_x17_computer_01",
                "sm_prop_smug_monitor_01"
            });
            targets.Add("tv_flat_01", new List<string> {
                "xm_prop_x17_tv_flat_01",
                "sm_prop_smug_tv_flat_01"
            });
            targets.Add("monitor_02", new List<string> {
                "prop_x17_computer_02"
            });
            targets.Add("tvstand_screen", new List<string> {
                "xm_prop_x17_tv_stand_01a"
            });
            targets.Add("prop_x17_8scrn_01", new List<string> {
                "xm_prop_x17_screens_02a_01"
            });
            targets.Add("prop_x17_8scrn_02", new List<string> {
                "xm_prop_x17_screens_02a_02"
            });
            //targets.Add ("prop_x17_8scrn_03", new List<string> {
            //    "xm_prop_x17_screens_02a_03"
            //});
            targets.Add("prop_x17_8scrn_04", new List<string> {
                "xm_prop_x17_screens_02a_04"
            });
            targets.Add("prop_x17_8scrn_05", new List<string> {
                "xm_prop_x17_screens_02a_05"
            });
            targets.Add("prop_x17_8scrn_06", new List<string> {
                "xm_prop_x17_screens_02a_06"
            });
            targets.Add("prop_x17_8scrn_07", new List<string> {
                "xm_prop_x17_screens_02a_07"
            });
            targets.Add("prop_x17_8scrn_08", new List<string> {
                "xm_prop_x17_screens_02a_08"
            });
            targets.Add("prop_x17_tv_ceil_scn_01", new List<string> {
                "xm_prop_x17_tv_ceiling_scn_01"
            });
            targets.Add("prop_x17_tv_ceil_scn_02", new List<string> {
                "xm_prop_x17_tv_ceiling_scn_02"
            });
            targets.Add("prop_x17_tv_scrn_01", new List<string> {
                "xm_prop_x17_tv_scrn_01"
            });
            targets.Add("prop_x17_tv_scrn_02", new List<string> {
                "xm_prop_x17_tv_scrn_02"
            });
            targets.Add("prop_x17_tv_scrn_03", new List<string> {
                "xm_prop_x17_tv_scrn_03"
            });
            targets.Add("prop_x17_tv_scrn_04", new List<string> {
                "xm_prop_x17_tv_scrn_04"
            });
            targets.Add("prop_x17_tv_scrn_05", new List<string> {
                "xm_prop_x17_tv_scrn_05"
            });
            targets.Add("prop_x17_tv_scrn_06", new List<string> {
                "xm_prop_x17_tv_scrn_06"
            });
            targets.Add("prop_x17_tv_scrn_07", new List<string> {
                "xm_prop_x17_tv_scrn_07"
            });
            targets.Add("prop_x17_tv_scrn_08", new List<string> {
                "xm_prop_x17_tv_scrn_08"
            });
            targets.Add("prop_x17_tv_scrn_09", new List<string> {
                "xm_prop_x17_tv_scrn_09"
            });
            targets.Add("prop_x17_tv_scrn_10", new List<string> {
                "xm_prop_x17_tv_scrn_10"
            });
            targets.Add("prop_x17_tv_scrn_11", new List<string> {
                "xm_prop_x17_tv_scrn_11"
            });
            targets.Add("prop_x17_tv_scrn_12", new List<string> {
                "xm_prop_x17_tv_scrn_12"
            });
            targets.Add("prop_x17_tv_scrn_13", new List<string> {
                "xm_prop_x17_tv_scrn_13"
            });
            targets.Add("prop_x17_tv_scrn_14", new List<string> {
                "xm_prop_x17_tv_scrn_14"
            });
            targets.Add("prop_x17_tv_scrn_15", new List<string> {
                "xm_prop_x17_tv_scrn_15"
            });
            targets.Add("prop_x17_tv_scrn_16", new List<string> {
                "xm_prop_x17_tv_scrn_16"
            });
            targets.Add("prop_x17_tv_scrn_17", new List<string> {
                "xm_prop_x17_tv_scrn_17"
            });
            targets.Add("prop_x17_tv_scrn_18", new List<string> {
                "xm_prop_x17_tv_scrn_18"
            });
            targets.Add("prop_x17_tv_scrn_19", new List<string> {
                "xm_prop_x17_tv_scrn_19"
            });
            targets.Add("prop_x17_tv_ceiling_01", new List<string> {
                "xm_screen_1"
            });
            targets.Add("ex_tvscreen", new List<string> {
                "ex_prop_ex_tv_flat_01"
            });
            targets.Add("prop_ex_computer_screen", new List<string> {
                "ex_prop_monitor_01_ex"
            });
            targets.Add("prop_ex_office_text", new List<string> {
                "ex_prop_ex_office_text"
            });
            targets.Add("gr_bunker_laptop_01a", new List<string> {
                "gr_prop_gr_laptop_01a"
            });
            targets.Add("bunker_laptop_sq_01a", new List<string> {
                "gr_prop_gr_laptop_01b"
            });
            targets.Add("gr_trailer_monitor_01", new List<string> {
                "gr_prop_gr_trailer_monitor_01"
            });
            targets.Add("gr_trailer_monitor_02", new List<string> {
                "gr_prop_gr_trailer_monitor_02"
            });
            targets.Add("gr_trailer_monitor_03", new List<string> {
                "gr_prop_gr_trailer_monitor_03"
            });
            targets.Add("gr_trailertv_01", new List<string> {
                "gr_prop_gr_trailer_tv"
            });
            targets.Add("gr_trailertv_02", new List<string> {
                "gr_prop_gr_trailer_tv_02"
            });
            targets.Add("heist_brd", new List<string> {
                "hei_prop_dlc_heist_board"
            });
            targets.Add("tablet", new List<string> {
                "hei_prop_dlc_tablet"
            });
            targets.Add("hei_mon", new List<string> {
                "hei_prop_hei_monitor_overlay"
            });
            targets.Add("blimp_text", new List<string> {
                "sr_mp_spec_races_blimp_sign"
            });
            targets.Add("id_text", new List<string> {
                "prop_police_id_text"
            });
            targets.Add("id_text_02", new List<string> {
                "prop_police_id_text_02"
            });
            targets.Add("crew_emblem_base", new List<string> {
                "xm_prop_base_crew_emblem"
            });
            targets.Add("orbital_table", new List<string> {
                "xm_prop_orbital_cannon_table"
            });
            targets.Add("prop_impexp_lappy_01a", new List<string> {
                "imp_prop_impexp_lappy_01a"
            });
            targets.Add("port_text", new List<string> {
                "apa_prop_ap_port_text"
            });
            targets.Add("starb_text", new List<string> {
                "apa_prop_ap_starb_text"
            });
            targets.Add("stern_text", new List<string> {
                "apa_prop_ap_stern_text"
            });
            targets.Add("digiscanner", new List<string> {
                "weapon_digiscanner"
            });
        }

    }
}