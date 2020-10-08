using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Client.Diagnostics;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Systems.Client.Managers
{
    public class IplManager : Manager<IplManager>
    {
        public override void Begin()
        {
            Logger.Info("[IPL MANAGER] Init");

            LoadMpDlcMaps();
            EnableMpDlcMaps(true);

            // TEMP
            RequestIpl("xm_mpchristmasadditions"); // trees?

            // KEEP BELOW
            RequestIpl("cs5_4_trains"); // 2773.61 2835.327 35.1903
            RequestIpl("airfield"); // 1743.682 3286.251 40.0875
            RequestIpl("trailerparkA_grp1"); // Lost trailer
            RequestIpl("dockcrane1"); // 889.3 -2910.9 40
            RequestIpl("chophillskennel"); // 19.0568 536.4818 169.6277

            // FIB WINDOW: 136.1795f, -750.701f, 262.0516f
            RequestIpl("atriumglstatic");
            // RemoveIpl("atriumglmission");
            // RemoveIpl("atriumglcut");

            // Hospital: 330.4596 -584.8196 42.3174
            RemoveIpl("RC12B_HospitalInterior"); // Broken interior
            RemoveIpl("RC12B_Destroyed"); // broken windows
            RequestIpl("RC12B_Default"); // default look
            RemoveIpl("RC12B_Fixed"); // boarded up

            // Trevor: 1985.48132, 3828.76757, 32.5
            // Trash or Tidy.Only choose one.
            RequestIpl("TrevorsTrailer");
            RemoveIpl("TrevorsTrailerTrash");
            RemoveIpl("TrevorsTrailerTidy");

            // rails: 2626.374 2949.869 39.1409
            RequestIpl("ld_rail_01_track");
            RequestIpl("ld_rail_02_track");

            // golf flags
            RequestIpl("golfflags");

            // Ferris Wheel: -1654.6622314453, -1121.9139404297, 13.027465820313
            RequestIpl("ferris_finale_anim");
            RequestIpl("ferris_finale_anim_lod");

            // Tunnel Roof: 28.828554153442, -626.26544189453, 30.304103851318
            RequestIpl("dt1_03_gr_closed");

            // Missing Elevators: -156.20492553711, -945.26287841797, 269.13494873047
            // RequestIpl("dt1_21_prop_lift"); // Only need one of them
            RequestIpl("dt1_21_prop_lift_on");

            // Fountain Fix: 184.48793029785, -678.62164306641, 43.14094543457
            RequestIpl("DT1_05_HC_REMOVE");

            // Simeon Car Dealer Shop Window Fix: -62.698680877686, -1099.888671875, 26.297519683838
            RequestIpl("shr_int");
            RequestIpl("shr_int_lod");

            // Heist Jewel: -637.20159 - 239.16250 38.1
            RequestIpl("post_hiest_unload");

            //  Max Renda: -585.8247, -282.72, 35.45475
            RequestIpl("refit_unload");

            //  Heist Union Depository: 2.69689322, -667.0166, 16.1306286
            RequestIpl("FINBANK");

            //  Morgue: 239.75195, -1360.64965, 39.53437
            RequestIpl("Coroner_Int_on");
            RequestIpl("coronertrash");

            //  Cluckin Bell: -146.3837, 6161.5, 30.2062
            RequestIpl("CS1_02_cf_onmission1");
            RequestIpl("CS1_02_cf_onmission2");
            RequestIpl("CS1_02_cf_onmission3");
            RequestIpl("CS1_02_cf_onmission4");

            //  Grapeseed's farm: 2447.9, 4973.4, 47.7
            RequestIpl("farm");
            RequestIpl("farmint");
            RequestIpl("farm_lod");
            RequestIpl("farm_props");
            RequestIpl("des_farmhouse");

            //  FIB lobby: 105.4557, -745.4835, 44.7548
            RequestIpl("FIBlobby");

            //  Billboard: iFruit
            RequestIpl("FruitBB");
            RequestIpl("sc1_01_newbill");
            RequestIpl("hw1_02_newbill");
            RequestIpl("hw1_emissive_newbill");
            RequestIpl("sc1_14_newbill");
            RequestIpl("dt1_17_newbill");

            //  Lester's factory: 716.84, -962.05, 31.59
            RequestIpl("id2_14_during_door");
            RequestIpl("id2_14_during1");

            //  Life Invader lobby: -1047.9, -233.0, 39.0
            RequestIpl("facelobby");

            //  Tunnels
            RequestIpl("v_tunnel_hole");

            //  Carwash: 55.7, -1391.3, 30.5
            RequestIpl("Carwash_with_spinners");

            //  Stadium "Fame or Shame": -248.49159240722656, -2010.509033203125, 34.57429885864258
            RequestIpl("sp1_10_real_interior");
            RequestIpl("sp1_10_real_interior_lod");

            //  House in Banham Canyon: -3086.428, 339.2523, 6.3717
            RequestIpl("ch1_02_open");
            // RequestIpl("ch1_02_closed"); // adds shutters

            //  Garage in La Mesa(autoshop): 970.27453, -1826.56982, 31.11477
            RequestIpl("bkr_bi_id1_23_door");

            //  Hill Valley church - Grave: -282.46380000, 2835.84500000, 55.91446000
            RequestIpl("lr_cs6_08_grave_closed");

            //  Lost's trailer park: 49.49379000, 3744.47200000, 46.38629000
            RequestIpl("methtrailer_grp1");

            //  Lost safehouse: 984.1552, -95.3662, 74.50
            RequestIpl("bkr_bi_hw1_13_int");

            //  Raton Canyon river: 1600.619 4443.457 0.725
            RequestIpl("CanyonRvrShallow");
            RequestIpl("CanyonRvrDeep");

            //  Pillbox hospital: 307.1680, -590.807, 43.280
            RequestIpl("rc12b_default");

            //  Josh's house: -1117.1632080078, 303.090698, 66.52217
            RequestIpl("bh1_47_joshhse_unburnt");
            RequestIpl("bh1_47_joshhse_unburnt_lod");

            //  Bahama Mamas: -1388.0013, -618.41967, 30.819599
            RequestIpl("hei_sm_16_interior_v_bahama_milo_");

            //  Zancudo River(need streamed content): 86.815, 3191.649, 30.463
            RequestIpl("cs3_05_water_grp1");
            RequestIpl("cs3_05_water_grp1_lod");
            RequestIpl("cs3_05_water_grp2");
            RequestIpl("cs3_05_water_grp2_lod");
            RequestIpl("trv1_trail_start");

            //  Cassidy Creek(need streamed content): -425.677, 4433.404, 27.3253
            RequestIpl("canyonriver01");
            RequestIpl("canyonriver01_lod");

            // Josh's house: -1117.1632080078, 303.090698, 66.52217
            RequestIpl("bh1_47_joshhse_unburnt");
            RequestIpl("bh1_47_joshhse_unburnt_lod");

            // Graffitis
            RequestIpl("CH3_RD2_BishopsChickenGraffiti"); // 1861.28, 2402.11, 58.53
            RequestIpl("CS5_04_MazeBillboardGraffiti"); // 2697.32, 3162.18, 58.1
            RequestIpl("CS5_Roads_RonOilGraffiti"); // 2119.12, 3058.21, 53.25

            // Pillbox hill hospital
            RequestIpl("rc12b_hospitalinterior");
            RemoveIpl("rc12b_hospitalinterior_lod");
            RemoveIpl("rc12b_default");
            RemoveIpl("rc12b_fixed");
            RemoveIpl("rc12b_destroyed");

            // Bunker exteriors
            RequestIpl("xm_hatch_01_cutscene"); // 1286.924 2846.06 49.39426
            RequestIpl("xm_hatch_02_cutscene"); // 18.633 2610.834 86.0
            RequestIpl("xm_hatch_03_cutscene"); // 2768.574 3919.924 45.82
            RequestIpl("xm_hatch_04_cutscene"); // 3406.90 5504.77 26.28
            RequestIpl("xm_hatch_06_cutscene"); // 1.90 6832.18 15.82
            RequestIpl("xm_hatch_07_cutscene"); // -2231.53 2418.42 12.18
            RequestIpl("xm_hatch_08_cutscene"); // -6.92 3327.0 41.63
            RequestIpl("xm_hatch_09_cutscene"); // 2073.62 1748.77 104.51
            RequestIpl("xm_hatch_10_cutscene"); // 1874.35 284.34 164.31
            RequestIpl("xm_hatch_closed"); // Closed hatches(all)
            RequestIpl("xm_siloentranceclosed_x17"); // Closed silo: 598.4869 5556.846 716.7615
            RequestIpl("xm_bunkerentrance_door"); // Bunker entrance closed door: 2050.85 2950.0 47.75
            RequestIpl("xm_hatches_terrain"); // Terrain adjustments for facilities(all) + silo
            RequestIpl("xm_hatches_terrain_lod");

            // Bahama Mamas: -1388.0013, -618.41967, 30.819599
            // RequestIpl("hei_sm_16_interior_v_bahama_milo_"); add to interior loader?

            Logger.Info("[IPL MANAGER] Loaded");
        }

    }
}
