using System.Collections.Generic;

namespace Curiosity.Client.net.Helpers.Dictionary
{
    class PlayerInteractables
    {

        public static List<string> Interactables = new List<string>();
        public static Dictionary<string, Entity.InteractableSetting> SitableItems = new Dictionary<string, Entity.InteractableSetting>();

        public static bool InteractableContains(string model)
        {
            return Interactables.Contains(model);
        }

        public static Entity.InteractableSetting GetInteractableSetting(string model)
        {
            return SitableItems[model];
        }

        public static void SetupInteractables()
        {
            if (Interactables.Count > 0) return;

            Interactables.Add("prop_bench_01a");
            Interactables.Add("prop_bench_01b");
            Interactables.Add("prop_bench_01c");
            Interactables.Add("prop_bench_02");
            Interactables.Add("prop_bench_03");
            Interactables.Add("prop_bench_04");
            Interactables.Add("prop_bench_05");
            Interactables.Add("prop_bench_06");
            Interactables.Add("prop_bench_07");
            Interactables.Add("prop_bench_08");
            Interactables.Add("prop_bench_09");
            Interactables.Add("prop_bench_10");
            Interactables.Add("prop_bench_11");
            Interactables.Add("prop_fib_3b_bench");
            Interactables.Add("prop_ld_bench01");
            Interactables.Add("prop_wait_bench_01");
            Interactables.Add("hei_prop_heist_off_chair");
            Interactables.Add("hei_prop_hei_skid_chair");
            Interactables.Add("prop_chair_01a");
            Interactables.Add("prop_chair_01b");
            Interactables.Add("prop_chair_02");
            Interactables.Add("prop_chair_03");
            Interactables.Add("prop_chair_04a");
            Interactables.Add("prop_chair_04b");
            Interactables.Add("prop_chair_05");
            Interactables.Add("prop_chair_06");
            Interactables.Add("prop_chair_07");
            Interactables.Add("prop_chair_08");
            Interactables.Add("prop_chair_09");
            Interactables.Add("prop_chair_10");
            Interactables.Add("prop_chateau_chair_01");
            Interactables.Add("prop_clown_chair");
            Interactables.Add("prop_cs_office_chair");
            Interactables.Add("prop_direct_chair_01");
            Interactables.Add("prop_direct_chair_02");
            Interactables.Add("prop_gc_chair02");
            Interactables.Add("prop_off_chair_01");
            Interactables.Add("prop_off_chair_03");
            Interactables.Add("prop_off_chair_04");
            Interactables.Add("prop_off_chair_04b");
            Interactables.Add("prop_off_chair_04_s");
            Interactables.Add("prop_off_chair_05");
            Interactables.Add("prop_old_deck_chair");
            Interactables.Add("prop_old_wood_chair");
            Interactables.Add("prop_rock_chair_01");
            Interactables.Add("prop_skid_chair_01");
            Interactables.Add("prop_skid_chair_02");
            Interactables.Add("prop_skid_chair_03");
            Interactables.Add("prop_sol_chair");
            Interactables.Add("prop_wheelchair_01");
            Interactables.Add("prop_wheelchair_01_s");
            Interactables.Add("p_armchair_01_s");
            Interactables.Add("p_clb_officechair_s");
            Interactables.Add("p_dinechair_01_s");
            Interactables.Add("p_ilev_p_easychair_s");
            Interactables.Add("p_soloffchair_s");
            Interactables.Add("p_yacht_chair_01_s");
            Interactables.Add("v_club_officechair");
            Interactables.Add("v_corp_bk_chair3");
            Interactables.Add("v_corp_cd_chair");
            Interactables.Add("v_corp_offchair");
            Interactables.Add("v_ilev_chair02_ped");
            Interactables.Add("v_ilev_hd_chair");
            Interactables.Add("v_ilev_p_easychair");
            Interactables.Add("v_ret_gc_chair03");
            Interactables.Add("prop_ld_farm_chair01");
            Interactables.Add("prop_table_04_chr");
            Interactables.Add("prop_table_05_chr");
            Interactables.Add("prop_table_06_chr");
            Interactables.Add("v_ilev_leath_chr");
            Interactables.Add("prop_table_01_chr_a");
            Interactables.Add("prop_table_01_chr_b");
            Interactables.Add("prop_table_02_chr");
            Interactables.Add("prop_table_03b_chr");
            Interactables.Add("prop_table_03_chr");
            Interactables.Add("prop_torture_ch_01");
            Interactables.Add("v_ilev_fh_dineeamesa");
            Interactables.Add("v_ilev_tort_stool");
            Interactables.Add("v_ilev_fh_kitchenstool");
            Interactables.Add("hei_prop_yah_seat_01");
            Interactables.Add("hei_prop_yah_seat_02");
            Interactables.Add("hei_prop_yah_seat_03");
            Interactables.Add("prop_waiting_seat_01");
            Interactables.Add("prop_yacht_seat_01");
            Interactables.Add("prop_yacht_seat_02");
            Interactables.Add("prop_yacht_seat_03");
            Interactables.Add("prop_hobo_seat_01");
            Interactables.Add("prop_rub_couch01");
            Interactables.Add("miss_rub_couch_01");
            Interactables.Add("prop_ld_farm_couch01");
            Interactables.Add("prop_ld_farm_couch02");
            Interactables.Add("prop_rub_couch02");
            Interactables.Add("prop_rub_couch03");
            Interactables.Add("prop_rub_couch04");
            Interactables.Add("p_lev_sofa_s");
            Interactables.Add("p_res_sofa_l_s");
            Interactables.Add("p_v_med_p_sofa_s");
            Interactables.Add("p_yacht_sofa_01_s");
            Interactables.Add("v_ilev_m_sofa");
            Interactables.Add("v_res_tre_sofa_s");
            Interactables.Add("v_tre_sofa_mess_a_s");
            Interactables.Add("v_tre_sofa_mess_b_s");
            Interactables.Add("v_tre_sofa_mess_c_s");
            Interactables.Add("prop_roller_car_01");
            Interactables.Add("prop_roller_car_02");
            Interactables.Add("ex_mp_h_off_sofa_01");
            Interactables.Add("ex_office2c_sofa01");
            Interactables.Add("ex_mp_h_stn_chairstrip_05");
        }

        public static void SetupSitableItems()
        {
            if (SitableItems.Count > 0) return;

            SitableItems.Add("prop_bench_01a", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_01b", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_01c", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_03", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_04", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_05", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_06", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_07", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_08", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_09", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_10", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_bench_11", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_fib_3b_bench", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_ld_bench01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_wait_bench_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("hei_prop_heist_off_chair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("hei_prop_hei_skid_chair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_01a", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_01b", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_03", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_04a", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_04b", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_05", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_06", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_07", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_08", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_09", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chair_10", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_chateau_chair_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_clown_chair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_cs_office_chair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_direct_chair_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_direct_chair_02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_gc_chair02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_off_chair_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_off_chair_03", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_off_chair_04", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_off_chair_04b", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_off_chair_04_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_off_chair_05", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_old_deck_chair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_old_wood_chair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_rock_chair_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_skid_chair_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_skid_chair_02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_skid_chair_03", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_sol_chair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_wheelchair_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_wheelchair_01_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_armchair_01_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_clb_officechair_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_dinechair_01_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_ilev_p_easychair_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_soloffchair_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_yacht_chair_01_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_club_officechair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_corp_bk_chair3", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_corp_cd_chair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_corp_offchair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_ilev_chair02_ped", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_ilev_hd_chair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_ilev_p_easychair", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_ret_gc_chair03", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_ld_farm_chair01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_table_04_chr", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_table_05_chr", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_table_06_chr", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_ilev_leath_chr", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_table_01_chr_a", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_table_01_chr_b", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_table_02_chr", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_table_03b_chr", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_table_03_chr", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_torture_ch_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_ilev_fh_dineeamesa", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_ilev_tort_stool", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_ilev_fh_kitchenstool", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("hei_prop_yah_seat_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("hei_prop_yah_seat_02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("hei_prop_yah_seat_03", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_waiting_seat_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_yacht_seat_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_yacht_seat_02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_yacht_seat_03", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_hobo_seat_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.65f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_rub_couch01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("miss_rub_couch_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_ld_farm_couch01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_ld_farm_couch02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_rub_couch02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_rub_couch03", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_rub_couch04", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_lev_sofa_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_res_sofa_l_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_v_med_p_sofa_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("p_yacht_sofa_01_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_ilev_m_sofa", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_res_tre_sofa_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_tre_sofa_mess_a_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_tre_sofa_mess_b_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("v_tre_sofa_mess_c_s", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_roller_car_01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("prop_roller_car_02", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("ex_office2c_sofa01", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_BENCH", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
            SitableItems.Add("ex_mp_h_stn_chairstrip_05", new Entity.InteractableSetting { Scenario = "PROP_HUMAN_SEAT_ARMCHAIR﻿", VerticalOffset = -0.5f, ForwardOffset = 0.0f, LeftOffset = 0.0f });
        }
    }
}
