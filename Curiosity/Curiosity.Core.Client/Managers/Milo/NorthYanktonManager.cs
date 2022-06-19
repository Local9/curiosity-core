using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Systems.Library.Models;
using System.Drawing;

namespace Curiosity.Core.Client.Managers.Milo
{
    public class NorthYanktonManager : Manager<NorthYanktonManager>
    {
        private const string NOTIFI_NORTH_YANKTON = "Go to North Yankton";
        private const string NOTIFI_LOS_SANTOS = "Go to Los Santos";

        Position posLosSantos1 = new Position();

        Position posNorthYankton = new Position();

        Color markerColor = Color.FromArgb(255, 135, 206, 235);
        Vector3 markerScale = new Vector3(1.5f, 1.5f, .5f);
        Vector3 markerScaleVehicle = new Vector3(10f, 10f, 1f);

        NUIMarker markerLs1;
        NUIMarker markerNy1;

        ConfigurationManager configurationManager = ConfigurationManager.GetModule();

        public override void Begin()
        {

        }

        private void Init()
        {
            LoadMpDlcMaps();
            EnableMpDlcMaps(true);
        }

        public void SetupLosSantos()
        {
            Logger.Debug($"Run Los Santos Setup");

            Init();

            Dictionary<string, BlipData> blips = BlipManager.GetModule().AllBlips;
            foreach (KeyValuePair<string, BlipData> kvp in blips)
            {
                BlipData blip = kvp.Value;
                foreach (Blip b in blip.Blips)
                {
                    SetBlipHiddenOnLegend(b.Handle, false);
                    SetBlipAlpha(b.Handle, 255);
                }
            }

            SetIslandHopperEnabled("HeistIsland", false);
            SetToggleMinimapHeistIsland(false);
            SetAiGlobalPathNodesType(0);
            SetScenarioGroupEnabled("Heist_Island_Peds", false);
            SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Zones", false, false);
            SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Disabled_Zones", true, false);
            SetAudioFlag("PlayerOnDLCHeist4Island", false);
            ResetDeepOceanScaler();
            RemoveIpl("h4_airstrip_hanger");

            List<string> activeIpls = configurationManager.ActiveIpls();
            List<string> losSantosIpls = configurationManager.LosSantosIpls();

            Common.RequestIpls(activeIpls);
            Common.RemoveIpls(losSantosIpls);
        }
    }
}
