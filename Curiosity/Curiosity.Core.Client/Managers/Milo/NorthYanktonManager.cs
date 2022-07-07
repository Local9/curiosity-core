using CitizenFX.Core.UI;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Interface;
using Curiosity.Systems.Library.Data;
using Curiosity.Systems.Library.Models;
using System.Drawing;

namespace Curiosity.Core.Client.Managers.Milo
{
    public class NorthYanktonManager : Manager<NorthYanktonManager>
    {
        private const string NOTIFI_NORTH_YANKTON = "Go to North Yankton";
        private const string NOTIFI_LOS_SANTOS = "Go to Los Santos";

        Position posLosSantos1 = new Position(-1027.266f, -2487.949f, 13.93783f, 241.0531f);

        Position posNorthYankton = new Position(3244.602f, -4673.054f, 114.2441f, 177.4711f);

        Color markerColor = Color.FromArgb(255, 135, 206, 235);
        Vector3 markerScale = new Vector3(1.5f, 1.5f, .5f);

        NUIMarker markerLosSantos;
        NUIMarker markerNorthYankton;

        ConfigurationManager configurationManager = ConfigurationManager.GetModule();
        BlipManager blipManager = BlipManager.GetModule();

        public override void Begin()
        {
            markerLosSantos = new NUIMarker(MarkerType.VerticalCylinder, posLosSantos1.AsVector(), markerScale, 10f, markerColor);
            markerLosSantos.TeleportPosition = posNorthYankton;
            markerLosSantos.Data = new { teleportToLosSantos = false, allowVehicle = false };
            markerLosSantos.PlaceOnGround = true;
            markerLosSantos.Add();

            markerNorthYankton = new NUIMarker(MarkerType.VerticalCylinder, posNorthYankton.AsVector(), markerScale, 10f, markerColor);
            markerNorthYankton.TeleportPosition = posLosSantos1;
            markerNorthYankton.Data = new { teleportToLosSantos = false, allowVehicle = false };
            markerNorthYankton.PlaceOnGround = true;
            markerNorthYankton.Add();
        }

        private async Task OnFellOffNorthYankton()
        {
            if (IsEntityInWater(Game.PlayerPed.Handle))
            {
                MovePlayer(posLosSantos1, true, false);
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnTeleportToNorthYankton()
        {
            string message = NOTIFI_LOS_SANTOS;
            NUIMarker activeMarker = null;

            if (Game.PlayerPed.IsDead)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (Game.PlayerPed.Opacity == 0)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (markerLosSantos.IsInRange)
            {
                message = NOTIFI_NORTH_YANKTON;
                activeMarker = markerLosSantos;
            }

            if (markerNorthYankton.IsInRange)
            {
                message = NOTIFI_LOS_SANTOS;
                activeMarker = markerNorthYankton;
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

            while (activeMarker.IsInRange)
            {
                if (activeMarker.IsInMarker)
                {
                    if (Game.PlayerPed.IsInVehicle() && !activeMarker.Data.allowVehicle)
                    {
                        NativeUI.Notifications.ShowHelpNotification($"Cannot use this location with a vehicle");
                    }
                    else
                    {
                        NativeUI.Notifications.ShowHelpNotification($"{message}, press ~INPUT_CONTEXT~");

                        if (Game.IsControlPressed(0, Control.Context) && !Cache.PlayerPed.IsDead)
                        {
                            MovePlayer(activeMarker.TeleportPosition, activeMarker.Data.teleportToLosSantos, activeMarker.Data.allowVehicle);
                            await BaseScript.Delay(10000);
                        }
                    }
                }

                if (!activeMarker.IsInMarker)
                {
                    Vector3 notificationMarker = activeMarker.Position;
                    notificationMarker.Z = notificationMarker.Z + 1f;
                    NativeUI.Notifications.ShowFloatingHelpNotification(message, notificationMarker);
                }

                await BaseScript.Delay(0);
            }
        }

        private async Task MovePlayer(Position pos, bool teleportToLosSantos, bool allowVehicle)
        {
            Init();

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

            SubRegion subRegion = SubRegion.UNKNOWN;

            Logger.Debug($"Teleport to: Los Santos");
            if (teleportToLosSantos)
            {
                Instance.DiscordRichPresence.Status = $"Roaming Los Santos...";
                SetupLosSantos();
                // Change world to City
                EventSystem.Send($"world:routing:city");
                PluginManager.Instance.DetachTickHandler(OnFellOffNorthYankton);
            }

            Logger.Debug($"Teleport to: North Yankton");
            if (!teleportToLosSantos)
            {
                SetupNorthYankton();
                subRegion = SubRegion.PROL;
                Instance.DiscordRichPresence.Status = $"Roaming North Yankton...";
                // Change world to Island
                EventSystem.Send($"world:routing:northYankton");
                PluginManager.Instance.AttachTickHandler(OnFellOffNorthYankton);
            }
            Instance.DiscordRichPresence.Commit();

            Cache.Character.IsOnIsland = !teleportToLosSantos;

            Logger.Debug($"Character.IsOnIsland: {Cache.Character.IsOnIsland}");

            if (!allowVehicle)
            {
                Cache.PlayerPed.Position = pos.AsVector();
                Cache.PlayerPed.Heading = pos.H;
            }

            if (allowVehicle)
            {
                if (Cache.PlayerPed.IsInVehicle())
                {
                    SetPedCoordsKeepVehicle(Cache.PlayerPed.Handle, pos.X, pos.Y, pos.Z);
                    Cache.PlayerPed.CurrentVehicle.Heading = pos.H;
                }
                else
                {
                    Cache.PlayerPed.Position = pos.AsVector();
                    Cache.PlayerPed.Heading = pos.H;
                }
            }

            WorldManager.GetModule().UpdateWeather(true, subRegion);

            await BaseScript.Delay(2000);

            await ScreenInterface.FadeIn(1000);
            Cache.PlayerPed.FadeIn();
        }

        private void Init()
        {
            LoadMpDlcMaps();
            EnableMpDlcMaps(true);
        }

        public void SetupLosSantos()
        {
            Logger.Debug($"Run Los Santos Setup");

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

            SetMinimapInPrologue(false);

            List<string> activeIpls = configurationManager.ActiveIpls();
            List<string> northYanktonIpls = configurationManager.NorthYanktonIpls();

            Common.RequestIpls(activeIpls);
            Common.RemoveIpls(northYanktonIpls);
        }

        public void SetupNorthYankton()
        {
            Logger.Debug($"Run North Yankton Setup");

            Dictionary<string, BlipData> blips = BlipManager.GetModule().AllBlips;
            foreach (KeyValuePair<string, BlipData> kvp in blips)
            {
                BlipData blip = kvp.Value;
                foreach (Blip b in blip.Blips)
                {
                    SetBlipHiddenOnLegend(b.Handle, true);
                    SetBlipAlpha(b.Handle, 0);
                }
            }

            SetMinimapInPrologue(true);

            List<string> activeIpls = configurationManager.ActiveIpls();
            List<string> northYanktonIpls = configurationManager.NorthYanktonIpls();

            Common.RemoveIpls(activeIpls);
            Common.RequestIpls(northYanktonIpls);
        }
    }
}
