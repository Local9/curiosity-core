﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Extensions.Native;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Utils;
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
        private const string NOTIFI_CAYO_ISLAND = "Go to Cayo Perico Island";
        private const string NOTIFI_LOS_SANTOS = "Go to Los Santos";

        /*
* Must manage player when connecting
* Must manage moving player
* 
* */
        Position posLosSantos1 = new Position(3857.16f, 4459.48f, 0.85f, 357.31f);
        Position posLosSantos2 = new Position(-1605.7f, 5258.76f, 1.2f, 23.88f);
        Position posLosSantos3 = new Position(-1016.42f, -2468.58f, 12.99f, 233.31f);

        Position posCayo1 = new Position(4929.47f, -5174.01f, 1.5f, 241.13f);
        Position posCayo2 = new Position(5094.14f, -4655.52f, 0.8f, 70.03f);
        Position posCayo3 = new Position(4425.68f, -4487.06f, 3.25f, 200.56f);

        Color markerColor = Color.FromArgb(255, 135, 206, 235);
        Vector3 markerScale = new Vector3(3f, 3f, .5f);

        NUIMarker markerLs1;
        NUIMarker markerLs2;
        NUIMarker markerLs3;

        NUIMarker markerCp1;
        NUIMarker markerCp2;
        NUIMarker markerCp3;

        ConfigurationManager configurationManager = ConfigurationManager.GetModule();

        public override void Begin()
        {
            markerLs1 = new NUIMarker(MarkerType.VerticalCylinder, posLosSantos1.AsVector(true), markerScale, 10f, markerColor);
            markerLs1.TeleportPosition = posCayo1;
            markerLs1.Data = new { teleportToLosSantos = false };
            markerLs2 = new NUIMarker(MarkerType.VerticalCylinder, posLosSantos2.AsVector(true), markerScale, 10f, markerColor);
            markerLs2.TeleportPosition = posCayo2;
            markerLs2.Data = new { teleportToLosSantos = false };
            markerLs3 = new NUIMarker(MarkerType.VerticalCylinder, posLosSantos3.AsVector(true), markerScale, 10f, markerColor);
            markerLs3.TeleportPosition = posCayo3;
            markerLs3.Data = new { teleportToLosSantos = false };

            markerCp1 = new NUIMarker(MarkerType.VerticalCylinder, posCayo1.AsVector(true), markerScale, 10f, markerColor);
            markerCp1.TeleportPosition = posLosSantos1;
            markerCp1.Data = new { teleportToLosSantos = true };
            markerCp2 = new NUIMarker(MarkerType.VerticalCylinder, posCayo2.AsVector(true), markerScale, 10f, markerColor);
            markerCp2.TeleportPosition = posLosSantos2;
            markerCp2.Data = new { teleportToLosSantos = true };
            markerCp3 = new NUIMarker(MarkerType.VerticalCylinder, posCayo3.AsVector(true), markerScale, 10f, markerColor);
            markerCp3.TeleportPosition = posLosSantos3;
            markerCp3.Data = new { teleportToLosSantos = true };

            NativeUI.MarkersHandler.AddMarker(markerLs1);
            NativeUI.MarkersHandler.AddMarker(markerLs2);
            NativeUI.MarkersHandler.AddMarker(markerLs3);
            NativeUI.MarkersHandler.AddMarker(markerCp1);
            NativeUI.MarkersHandler.AddMarker(markerCp2);
            NativeUI.MarkersHandler.AddMarker(markerCp3);
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTeleportToCayoPerico()
        {
            string notificationMessage = string.Empty;
            NUIMarker activeMarker = null;

            if (Cache.PlayerPed.IsDead)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (Cache.PlayerPed.Opacity == 0)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (markerLs1.IsInRange || markerLs2.IsInRange || markerLs3.IsInRange)
            {
                notificationMessage = NOTIFI_CAYO_ISLAND;

                if (markerLs1.IsInRange)
                {
                    activeMarker = markerLs1;
                }
                else if (markerLs2.IsInRange)
                {
                    activeMarker = markerLs2;
                }
                else if (markerLs3.IsInRange)
                {
                    activeMarker = markerLs3;
                }
            }

            if (markerCp1.IsInRange || markerCp2.IsInRange || markerCp3.IsInRange)
            {
                notificationMessage = NOTIFI_LOS_SANTOS;

                if (markerCp1.IsInRange)
                {
                    activeMarker = markerCp1;
                }
                else if (markerCp2.IsInRange)
                {
                    activeMarker = markerCp2;
                }
                else if (markerCp3.IsInRange)
                {
                    activeMarker = markerCp3;
                }
            }

            if (activeMarker is null && !Screen.Fading.IsFadedIn)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (activeMarker is null)
            {
                await BaseScript.Delay(1000);
                return;
            }

            while (activeMarker.IsInRange && !Cache.PlayerPed.IsInVehicle())
            {
                if (activeMarker.IsInMarker)
                {
                    NativeUI.Notifications.ShowHelpNotification($"{notificationMessage}, press ~INPUT_CONTEXT~");

                    if (Game.IsControlPressed(0, Control.Context) && !Cache.PlayerPed.IsDead)
                    {
                        MovePlayer(activeMarker.TeleportPosition, activeMarker.Data.teleportToLosSantos);
                        await BaseScript.Delay(10000);
                    }
                }

                if (!activeMarker.IsInMarker)
                {
                    Vector3 notificationMarker = activeMarker.Position;
                    notificationMarker.Z = notificationMarker.Z + 1f;
                    NativeUI.Notifications.ShowFloatingHelpNotification(notificationMessage, notificationMarker);
                }

                await BaseScript.Delay(0);
            }
        }

        private async Task MovePlayer(Position pos, bool teleportToLosSantos)
        {
            Logger.Debug($"teleportToLosSantos: {teleportToLosSantos} Position: {pos}");

            Cache.PlayerPed.FadeOut();
            await ScreenInterface.FadeOut(1000);

            Logger.Debug($"Screen Faded Out");

            if (pos is null)
            {
                Logger.Debug($"Position is null");
                await ScreenInterface.FadeIn(1000);
                await Cache.PlayerPed.FadeIn();
            }

            Logger.Debug($"Teleport to: Los Santos");
            if (teleportToLosSantos)
            {
                SetupLosSantos();
            }

            Logger.Debug($"Teleport to: Cayo Perico");
            if (!teleportToLosSantos)
            {
                SetupCayoPerico();
            }

            Cache.Character.IsOnIsland = !teleportToLosSantos;

            Logger.Debug($"Character.IsOnIsland: {Cache.Character.IsOnIsland}");

            Cache.PlayerPed.Position = pos.AsVector();
            Cache.PlayerPed.Heading = pos.H;

            await BaseScript.Delay(2000);

            //SetPedCoordsKeepVehicle(Cache.PlayerPed.Handle, pos.X, pos.Y, pos.Z);
            //Cache.PlayerPed.CurrentVehicle.Heading = pos.H;

            await ScreenInterface.FadeIn(1000);
            Cache.PlayerPed.FadeIn();
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

            List<string> activeIpls = configurationManager.ActiveIpls();
            List<string> losSantosIpls = configurationManager.LosSantosIpls();
            List<string> cayoIpls = configurationManager.CayoIslandIpls();

            RequestIpls(activeIpls);
            RemoveIpls(losSantosIpls);
            RequestIpls(cayoIpls);

            SwitchTrainTrack(3, true); // Enable Metro
        }

        public void SetupCayoPerico()
        {
            Logger.Debug($"Run Cayo Perico Setup");

            Init();

            Dictionary<string, BlipData> blips = BlipManager.GetModule().AllBlips;
            foreach(KeyValuePair<string, BlipData> kvp in blips)
            {
                BlipData blip = kvp.Value;
                foreach(Blip b in blip.Blips)
                {
                    SetBlipHiddenOnLegend(b.Handle, true);
                }
            }

            SetIslandHopperEnabled("HeistIsland", true);
            SetToggleMinimapHeistIsland(true);
            SetAiGlobalPathNodesType(1);
            SetScenarioGroupEnabled("Heist_Island_Peds", true);
            SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Zones", true, true);
            SetAmbientZoneListStatePersistent("AZL_DLC_Hei4_Island_Disabled_Zones", false, true);
            SetAudioFlag("PlayerOnDLCHeist4Island", true);
            SetDeepOceanScaler(0.0f);

            List<string> activeIpls = configurationManager.ActiveIpls();
            List<string> losSantosIpls = configurationManager.LosSantosIpls();
            List<string> cayoIpls = configurationManager.CayoIslandIpls();

            RemoveIpls(activeIpls);
            RequestIpls(losSantosIpls);
            RemoveIpls(cayoIpls);

            SwitchTrainTrack(3, false); // Enable Metro
        }

        private void RemoveIpls(List<string> ipls)
        {
            foreach(string ipl in ipls)
            {
                Logger.Debug($"Remove IPL: {ipl}");
                if (IsIplActive(ipl)) RemoveIpl(ipl);
            }
        }

        private void RequestIpls(List<string> ipls)
        {
            foreach (string ipl in ipls)
            {
                Logger.Debug($"Request IPL: {ipl}");
                if (!IsIplActive(ipl)) RequestIpl(ipl);
            }
        }

        private void Init()
        {
            LoadMpDlcMaps();
            EnableMpDlcMaps(true);
        }
    }
}