using CitizenFX.Core;
using Curiosity.Core.Client.Extensions;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers.Milo
{
    public class CayoPericoManager : Manager<CayoPericoManager>
    {
        /*
         * Must manage player when connecting
         * Must manage moving player
         * 
         * */
        Position posLs1 = new Position(3857.16f, 4459.48f, 0.85f, 357.31f);
        Position posLs2 = new Position(-1605.7f, 5258.76f, 1.2f, 23.88f);
        Position posLs3 = new Position(-1016.42f, -2468.58f, 12.99f, 233.31f);

        Position posCp1 = new Position(4929.47f, -5174.01f, 1.5f, 241.13f);
        Position posCp2 = new Position(5094.14f, -4655.52f, 0.8f, 70.03f);
        Position posCp3 = new Position(4425.68f, -4487.06f, 3.25f, 200.56f);

        Color markerColor = Color.FromArgb(255, 135, 206, 235);
        Vector3 markerScale = new Vector3(5f, 5f, 0.5f);

        NativeUI.Marker markerLs1;
        NativeUI.Marker markerLs2;
        NativeUI.Marker markerLs3;

        NativeUI.Marker markerCp1;
        NativeUI.Marker markerCp2;
        NativeUI.Marker markerCp3;

        public override void Begin()
        {
            markerLs1 = new NativeUI.Marker(MarkerType.VerticalCylinder, posLs1.AsVector(), 10f, markerColor);
            markerLs2 = new NativeUI.Marker(MarkerType.VerticalCylinder, posLs2.AsVector(), 10f, markerColor);
            markerLs3 = new NativeUI.Marker(MarkerType.VerticalCylinder, posLs3.AsVector(), 10f, markerColor);
            markerCp1 = new NativeUI.Marker(MarkerType.VerticalCylinder, posCp1.AsVector(), 10f, markerColor);
            markerCp2 = new NativeUI.Marker(MarkerType.VerticalCylinder, posCp2.AsVector(), 10f, markerColor);
            markerCp3 = new NativeUI.Marker(MarkerType.VerticalCylinder, posCp3.AsVector(), 10f, markerColor);
        }

        public async void SetupLosSantos()
        {
            await Init();

            SwitchTrainTrack(3, true); // Enable Metro

            // Ferris Wheel
            RequestIpl("ferris_finale_anim");
            RequestIpl("ferris_finale_anim_lod");

            // Tunnel Roof
            RequestIpl("dt1_03_gr_closed");

            // Missing Elevators
            RequestIpl("dt1_21_prop_lift");
            // RequestIpl("dt1_21_prop_lift_on");

            // Fountain Fix
            RequestIpl("DT1_05_HC_REMOVE");

            RequestIpl("cs5_4_trains"); // 2773.61 2835.327 35.1903
            RequestIpl("airfield"); // 1743.682 3286.251 40.0875
            RequestIpl("trailerparkA_grp1"); // Lost trailer
            RequestIpl("dockcrane1"); // 889.3 -2910.9 40
            RequestIpl("chophillskennel"); // 19.0568 536.4818 169.6277

            // FIB WINDOW: 136.1795f, -750.701f, 262.0516f
            RequestIpl("atriumglstatic");

            // Hospital: 330.4596 -584.8196 42.3174
            RemoveIpl("RC12B_HospitalInterior"); // Broken interior
            RemoveIpl("RC12B_Destroyed"); // broken windows
            RequestIpl("RC12B_Default"); // default look
            RemoveIpl("RC12B_Fixed"); // boarded up

            // Trevor: 1985.48132, 3828.76757, 32.5
            // Trash or Tidy.Only choose one.
            RequestIpl("TrevorsTrailer");
            RequestIpl("TrevorsTrailerTidy");

            // rails: 2626.374 2949.869 39.1409
            RequestIpl("ld_rail_01_track");
            RequestIpl("ld_rail_02_track");
            RequestIpl("FBI_repair");

            // golf flags
            RequestIpl("golfflags");

            // Casino Exterior;
            RequestIpl("hei_dlc_casino_aircon");
            RequestIpl("hei_dlc_casino_aircon_lod");
            RequestIpl("hei_dlc_casino_door");
            RequestIpl("hei_dlc_casino_door_lod");
            RequestIpl("hei_dlc_vw_roofdoors_locked");
            RequestIpl("hei_dlc_windows_casino");
            RequestIpl("hei_dlc_windows_casino_lod");
            RequestIpl("vw_ch3_additions");
            RequestIpl("vw_ch3_additions_long_0");
            RequestIpl("vw_ch3_additions_strm_0");
            RequestIpl("vw_dlc_casino_door");
            RequestIpl("vw_dlc_casino_door_lod");

            // IPLs for Cayo Island in the Distance
            RequestIpl("xn_h4_islandx_terrain_01_slod");
            RequestIpl("xn_h4_islandx_terrain_02_slod");
            RequestIpl("xn_h4_islandx_terrain_04_slod");
            RequestIpl("xn_h4_islandx_terrain_05_slod");
            RequestIpl("xn_h4_islandx_terrain_06_slod");

            //RequestIpl("tr_tuner_meetup");
            //RequestIpl("tr_tuner_race_line");
            RequestIpl("tr_tuner_shop_burton");
            RequestIpl("tr_tuner_shop_mesa");
            RequestIpl("tr_tuner_shop_mission");
            RequestIpl("tr_tuner_shop_rancho");
            RequestIpl("tr_tuner_shop_strawberry");
        }

        public async void SetupCayPerico()
        {
            await Init();

            // Ferris Wheel
            RemoveIpl("ferris_finale_anim");
            RemoveIpl("ferris_finale_anim_lod");

            // Tunnel Roof
            RemoveIpl("dt1_03_gr_closed");

            // Missing Elevators
            RemoveIpl("dt1_21_prop_lift");
            // RequestIpl("dt1_21_prop_lift_on");

            // Fountain Fix
            RemoveIpl("DT1_05_HC_REMOVE");

            RemoveIpl("cs5_4_trains"); // 2773.61 2835.327 35.1903
            RemoveIpl("airfield"); // 1743.682 3286.251 40.0875
            RemoveIpl("trailerparkA_grp1"); // Lost trailer
            RemoveIpl("dockcrane1"); // 889.3 -2910.9 40
            RemoveIpl("chophillskennel"); // 19.0568 536.4818 169.6277

            // FIB WINDOW: 136.1795f, -750.701f, 262.0516f
            RemoveIpl("atriumglstatic");

            // Hospital: 330.4596 -584.8196 42.3174
            RemoveIpl("RC12B_HospitalInterior"); // Broken interior
            RemoveIpl("RC12B_Destroyed"); // broken windows
            RemoveIpl("RC12B_Default"); // default look
            RemoveIpl("RC12B_Fixed"); // boarded up

            // Trevor: 1985.48132, 3828.76757, 32.5
            // Trash or Tidy.Only choose one.
            RemoveIpl("TrevorsTrailer");
            RemoveIpl("TrevorsTrailerTidy");

            // rails: 2626.374 2949.869 39.1409
            RemoveIpl("ld_rail_01_track");
            RemoveIpl("ld_rail_02_track");
            RemoveIpl("FBI_repair");

            // golf flags
            RemoveIpl("golfflags");

            // Casino Exterior;
            RemoveIpl("hei_dlc_casino_aircon");
            RemoveIpl("hei_dlc_casino_aircon_lod");
            RemoveIpl("hei_dlc_casino_door");
            RemoveIpl("hei_dlc_casino_door_lod");
            RemoveIpl("hei_dlc_vw_roofdoors_locked");
            RemoveIpl("hei_dlc_windows_casino");
            RemoveIpl("hei_dlc_windows_casino_lod");
            RemoveIpl("vw_ch3_additions");
            RemoveIpl("vw_ch3_additions_long_0");
            RemoveIpl("vw_ch3_additions_strm_0");
            RemoveIpl("vw_dlc_casino_door");
            RemoveIpl("vw_dlc_casino_door_lod");

            //RemoveIpl("tr_tuner_meetup");
            //RemoveIpl("tr_tuner_race_line");
            RemoveIpl("tr_tuner_shop_burton");
            RemoveIpl("tr_tuner_shop_mesa");
            RemoveIpl("tr_tuner_shop_mission");
            RemoveIpl("tr_tuner_shop_rancho");
            RemoveIpl("tr_tuner_shop_strawberry");

            // IPLs for LS in the Distance
            RemoveIpl("xn_h4_islandx_terrain_01_slod");
            RemoveIpl("xn_h4_islandx_terrain_02_slod");
            RemoveIpl("xn_h4_islandx_terrain_04_slod");
            RemoveIpl("xn_h4_islandx_terrain_05_slod");
            RemoveIpl("xn_h4_islandx_terrain_06_slod");

            SwitchTrainTrack(3, false); // Enable Metro
        }

        private async Task Init()
        {
            await Session.Loading();
            LoadMpDlcMaps();
            EnableMpDlcMaps(true);
        }
    }
}
