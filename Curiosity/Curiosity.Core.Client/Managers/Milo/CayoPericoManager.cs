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

        ConfigurationManager configurationManager = ConfigurationManager.GetModule();

        public override void Begin()
        {
            markerLs1 = new NativeUI.Marker(MarkerType.VerticalCylinder, posLs1.AsVector(), 10f, markerColor);
            markerLs2 = new NativeUI.Marker(MarkerType.VerticalCylinder, posLs2.AsVector(), 10f, markerColor);
            markerLs3 = new NativeUI.Marker(MarkerType.VerticalCylinder, posLs3.AsVector(), 10f, markerColor);
            markerCp1 = new NativeUI.Marker(MarkerType.VerticalCylinder, posCp1.AsVector(), 10f, markerColor);
            markerCp2 = new NativeUI.Marker(MarkerType.VerticalCylinder, posCp2.AsVector(), 10f, markerColor);
            markerCp3 = new NativeUI.Marker(MarkerType.VerticalCylinder, posCp3.AsVector(), 10f, markerColor);
        }

        public async Task SetupLosSantos()
        {
            await Init();

            SwitchTrainTrack(3, true); // Enable Metro

            List<string> activeIpls = configurationManager.ActiveIpls();
            List<string> losSantosIpls = configurationManager.LosSantosIpls();
            List<string> cayoIpls = configurationManager.CayoIslandIpls();

            RequestIpls(activeIpls);
            RemoveIpls(losSantosIpls);
            RequestIpls(cayoIpls);

            SetIslandHopperEnabled("HeistIsland", false);
            SetToggleMinimapHeistIsland(false);
            SetAiGlobalPathNodesType(0);
            SetScenarioGroupEnabled("Heist_Island_Peds", false);
            SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Zones", false, false);
            SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Disabled_Zones", true, false);
            SetAudioFlag("PlayerOnDLCHeist4Island", false);
        }

        public async Task SetupCayPerico()
        {
            await Init();

            SwitchTrainTrack(3, false); // Enable Metro

            List<string> activeIpls = configurationManager.ActiveIpls();
            List<string> losSantosIpls = configurationManager.LosSantosIpls();
            List<string> cayoIpls = configurationManager.CayoIslandIpls();

            RemoveIpls(activeIpls);
            RequestIpls(losSantosIpls);
            RemoveIpls(cayoIpls);

            SetIslandHopperEnabled("HeistIsland", true);
            SetToggleMinimapHeistIsland(true);
            SetAiGlobalPathNodesType(1);
            SetScenarioGroupEnabled("Heist_Island_Peds", true);
            SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Zones", true, true);
            SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Disabled_Zones", false, true);
            SetAudioFlag("PlayerOnDLCHeist4Island", true);
        }

        private void RemoveIpls(List<string> ipls)
        {
            foreach(string ipl in ipls)
            {
                if (IsIplActive(ipl)) RemoveIpl(ipl);
            }
        }

        private void RequestIpls(List<string> ipls)
        {
            foreach (string ipl in ipls)
            {
                if (!IsIplActive(ipl)) RequestIpl(ipl);
            }
        }

        private async Task Init()
        {
            await Session.Loading();
            LoadMpDlcMaps();
            EnableMpDlcMaps(true);
        }
    }
}
