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
            API.RequestIpl("dt1_21_prop_lift");
            API.RequestIpl("dt1_21_prop_lift_on");

            // Fountain Fix
            API.RequestIpl("DT1_05_HC_REMOVE");
            
            // API.RequestIpl("xm_mpchristmasadditions");
        }
    }
}
