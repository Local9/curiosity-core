using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Classes.Environment.IPL
{
    class IplLoader
    {
        public static void Init()
        {
            API.LoadMpDlcMaps();
            API.EnableMpDlcMaps(true);

            // Ferris Wheel
            API.RequestIpl("ferris_finale_anim");
            API.RequestIpl("ferris_finale_anim_lod");

            // Tunnel Roof
            API.RequestIpl("dt1_03_gr_closed");

            // Missing Elevators
            // API.RequestIpl("dt1_21_prop_lift");
            API.RequestIpl("dt1_21_prop_lift_on");

            // Fountain Fix
            API.RequestIpl("DT1_05_HC_REMOVE");

            API.RequestIpl("cs5_4_trains"); // 2773.61 2835.327 35.1903
            API.RequestIpl("airfield"); // 1743.682 3286.251 40.0875
            API.RequestIpl("trailerparkA_grp1"); // Lost trailer
            API.RequestIpl("dockcrane1"); // 889.3 -2910.9 40
            API.RequestIpl("chophillskennel"); // 19.0568 536.4818 169.6277

            // FIB WINDOW: 136.1795f, -750.701f, 262.0516f
            API.RequestIpl("atriumglstatic");
            // RemoveIpl("atriumglmission");
            // RemoveIpl("atriumglcut");

            // Hospital: 330.4596 -584.8196 42.3174
            API.RemoveIpl("RC12B_HospitalInterior"); // Broken interior
            API.RemoveIpl("RC12B_Destroyed"); // broken windows
            API.RequestIpl("RC12B_Default"); // default look
            API.RemoveIpl("RC12B_Fixed"); // boarded up

            // Trevor: 1985.48132, 3828.76757, 32.5
            // Trash or Tidy.Only choose one.
            API.RequestIpl("TrevorsTrailer");
            API.RemoveIpl("TrevorsTrailerTrash");
            API.RemoveIpl("TrevorsTrailerTidy");

            // rails: 2626.374 2949.869 39.1409
            API.RequestIpl("ld_rail_01_track");
            API.RequestIpl("ld_rail_02_track");

            // golf flags
            API.RequestIpl("golfflags");

            // API.RequestIpl("xm_mpchristmasadditions");
        }
    }
}
