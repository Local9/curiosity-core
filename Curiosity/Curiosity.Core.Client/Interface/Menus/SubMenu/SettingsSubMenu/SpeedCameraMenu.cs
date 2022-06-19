using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Environment.Entities.Models.Config;
using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers;
using Curiosity.Core.Client.Scripts.JobPolice;
using Curiosity.Core.Client.Utils;
using NativeUI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Interface.Menus.SubMenu.SettingsSubMenu
{
    public class SpeedCameraMenu
    {
        SpeedCameraManager speedCameraManager => SpeedCameraManager.GetModule();
        NotificationManager NotificationManager => NotificationManager.GetModule();
        NoClipManager noClipManager => NoClipManager.GetModule();

        private UIMenu _menu;
        UIMenuCheckboxItem chkEnableCameraDebugging = new UIMenuCheckboxItem("Enable Debug", false);
        UIMenuCheckboxItem chkEnableSpeedCameraDebugging = new UIMenuCheckboxItem("Enable Debug Speed Cameras", false);

        UIMenuItem miSetCameraStart = new UIMenuItem("Start Position");
        UIMenuItem miSetCameraEnd = new UIMenuItem("End Position");
        UIMenuItem miSetCameraAdd = new UIMenuItem("Add");
        UIMenuItem miClear = new UIMenuItem("Clear Last");
        UIMenuItem miSave = new UIMenuItem("Save Positions");

        UIMenuListItem miAdjustClosedPointZ = new UIMenuListItem("Adjust Closest Z", new List<dynamic> { -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5 }, 5);
        UIMenuListItem miAdjustWidth = new UIMenuListItem("Adjust Width", new List<dynamic> { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 }, 15);

        private List<SpeedCamera> currentPoints = new List<SpeedCamera>();
        private List<SpeedCamera> pointsToSave = new List<SpeedCamera>();
        private List<SpeedCamera> pointsSaved = new List<SpeedCamera>();
        SpeedCamera currentCamera;

        float width = 10;

        public UIMenu Create(UIMenu menu)
        {
            menu.OnMenuStateChanged += Menu_OnMenuStateChanged;
            menu.OnCheckboxChange += Menu_OnCheckboxChange;
            menu.OnItemSelect += Menu_OnItemSelect;
            menu.OnListSelect += Menu_OnListSelect;

            menu.AddItem(chkEnableCameraDebugging);
            menu.AddItem(chkEnableSpeedCameraDebugging);
            menu.AddItem(miSetCameraStart);
            menu.AddItem(miSetCameraEnd);
            menu.AddItem(miAdjustWidth);
            menu.AddItem(miAdjustClosedPointZ);
            menu.AddItem(miSetCameraAdd);

            menu.AddItem(miClear);
            menu.AddItem(miSave);

            _menu = menu;
            return menu;
        }

        private void Menu_OnListSelect(UIMenu sender, UIMenuListItem listItem, int newIndex)
        {
            Vector3 pos = Game.PlayerPed.Position;
            List<SpeedCamera> closestCameras = GetClosestCamera(pos, 50f);

            if (listItem == miAdjustClosedPointZ)
            {
                foreach (SpeedCamera cam in closestCameras)
                {
                    if (cam.Start.Vector3.Distance(pos, true) < 5f)
                    {
                        cam.Start.Z += (int)listItem.Items[newIndex];
                    }
                    else if (cam.End.Vector3.Distance(pos, true) < 5f)
                    {
                        cam.End.Z += (int)listItem.Items[newIndex];
                    }
                }
            }
            else if (listItem == miAdjustWidth)
            {
                width = (int)listItem.Items[newIndex];
                foreach (SpeedCamera cam in closestCameras)
                {
                    if (cam.Start.Vector3.Distance(pos, true) < 5f)
                    {
                        cam.Width = width;
                    }
                    else if (cam.End.Vector3.Distance(pos, true) < 5f)
                    {
                        cam.Width = width;
                    }
                }
            }
        }

        public List<SpeedCamera> GetClosestCamera(Vector3 position, float distance)
        {
            return currentPoints
                    .Where(x => position.Distance(x.Center) < distance)
                    .OrderBy(x => position.Distance(x.Center)).ToList();
        }

        private void Menu_OnItemSelect(UIMenu sender, UIMenuItem selectedItem, int index)
        {
            if (selectedItem == miSetCameraStart)
            {
                AddNewCamera();
            }
            else if (selectedItem == miSetCameraEnd)
            {
                AddEndPointToCamera();
            }
            else if (selectedItem == miSetCameraAdd)
            {
                StoreCamera();
            }
            else if (selectedItem == miClear)
            {
                RemoveLastCamera();
            }
            else if (selectedItem == miSave)
            {
                SaveStoredPositions();
            }
        }

        private void Menu_OnCheckboxChange(UIMenu sender, UIMenuCheckboxItem checkboxItem, bool Checked)
        {
            if (checkboxItem == chkEnableCameraDebugging)
            {
                if (Checked)
                {
                    PluginManager.Instance.AttachTickHandler(OnSpeedCameraDebugDisplay);
                    PluginManager.Instance.AttachTickHandler(OnSpeedCameraCreatedDebugDisplay);
                }

                if (!Checked)
                {
                    PluginManager.Instance.DetachTickHandler(OnSpeedCameraDebugDisplay);
                    PluginManager.Instance.DetachTickHandler(OnSpeedCameraCreatedDebugDisplay);
                }
            }

            if (checkboxItem == chkEnableSpeedCameraDebugging)
            {
                speedCameraManager.isDebugging = Checked;
            }
        }

        private void Menu_OnMenuStateChanged(UIMenu oldMenu, UIMenu newMenu, MenuState state)
        {
            if (state == MenuState.Opened || state == MenuState.ChangeForward)
            {
                chkEnableSpeedCameraDebugging.Checked = speedCameraManager.isDebugging;
            }
        }

        private async Task OnSpeedCameraDebugDisplay()
        {
            RaycastResult raycastResult = WorldProbe.CrossairRenderingRaycastResult;

            if (raycastResult.DitHit && noClipManager.IsEnabled)
            {
                World.DrawMarker(MarkerType.DebugSphere, raycastResult.HitPosition, Vector3.Zero, Vector3.Zero, new Vector3(0.25f), System.Drawing.Color.FromArgb(255, 255, 0, 0));

                if (Game.IsControlJustPressed(0, Control.Jump))
                {
                    if (currentCamera == null)
                    {
                        AddNewCamera(raycastResult.HitPosition);
                    }
                    else if (currentCamera is not null)
                    {
                        AddEndPointToCamera(raycastResult.HitPosition + new Vector3(Vector2.Zero, 3f));
                    }

                    await BaseScript.Delay(500);
                }
            }
        }

        private async Task OnSpeedCameraCreatedDebugDisplay()
        {
            Vector3 playerPos = Game.PlayerPed.Position;
            foreach (PoliceCamera camera in PoliceConfig.SpeedCameras.Where(x => x.Center.Distance(playerPos) < 100f))
            {
                uint streetHash = 0;
                uint crossingRoad = 0;
                GetStreetNameAtCoord(camera.Start.X, camera.Start.Y, camera.Start.Z, ref streetHash, ref crossingRoad);
                string street = GetStreetNameFromHashKey(streetHash);

                string msg = $"[{camera.Direction}] ({camera.Limit ?? PoliceConfig.SpeedLimits[$"{streetHash}"]}) {streetHash} : {street}";

                ScreenInterface.Draw3DText(camera.Center, msg);

                Common.IsEntityInAngledArea(Game.PlayerPed, camera.Start.Vector3, camera.End.Vector3, camera.Width ?? PoliceConfig.SpeedCameraWidth, debug: true);
            }

            foreach (SpeedCamera camera in currentPoints.Where(x => x.Center.Distance(playerPos) < 100f))
            {
                uint streetHash = 0;
                uint crossingRoad = 0;
                GetStreetNameAtCoord(camera.Start.X, camera.Start.Y, camera.Start.Z, ref streetHash, ref crossingRoad);
                string street = GetStreetNameFromHashKey(streetHash);

                string msg = $"[{camera.Direction}] ({camera.Limit ?? PoliceConfig.SpeedLimits[$"{streetHash}"]}) {streetHash} : {street}";

                ScreenInterface.Draw3DText(camera.Center, msg);

                Common.IsEntityInAngledArea(Game.PlayerPed, camera.Start.Vector3, camera.End.Vector3, camera.Width ?? PoliceConfig.SpeedCameraWidth, debug: true);
            }

            foreach (SpeedCamera camera in pointsToSave.Where(x => x.Center.Distance(playerPos) < 100f))
            {
                uint streetHash = 0;
                uint crossingRoad = 0;
                GetStreetNameAtCoord(camera.Start.X, camera.Start.Y, camera.Start.Z, ref streetHash, ref crossingRoad);
                string street = GetStreetNameFromHashKey(streetHash);

                string msg = $"[{camera.Direction}] ({camera.Limit ?? PoliceConfig.SpeedLimits[$"{streetHash}"]}) {streetHash} : {street}";

                ScreenInterface.Draw3DText(camera.Center, msg);

                Common.IsEntityInAngledArea(Game.PlayerPed, camera.Start.Vector3, camera.End.Vector3, camera.Width ?? PoliceConfig.SpeedCameraWidth, debug: true);
            }

            foreach (SpeedCamera camera in pointsSaved.Where(x => x.Center.Distance(playerPos) < 100f))
            {
                uint streetHash = 0;
                uint crossingRoad = 0;
                GetStreetNameAtCoord(camera.Start.X, camera.Start.Y, camera.Start.Z, ref streetHash, ref crossingRoad);
                string street = GetStreetNameFromHashKey(streetHash);

                string msg = $"[{camera.Direction}] ({camera.Limit ?? PoliceConfig.SpeedLimits[$"{streetHash}"]}) {streetHash} : {street}";

                ScreenInterface.Draw3DText(camera.Center, msg);

                Common.IsEntityInAngledArea(Game.PlayerPed, camera.Start.Vector3, camera.End.Vector3, camera.Width ?? PoliceConfig.SpeedCameraWidth, debug: true);
            }

            _menu.Subtitle.Caption = $"No. Cams {pointsSaved.Count + pointsSaved.Count + currentPoints.Count + PoliceConfig.SpeedCameras.Count}";
            _menu.UpdateDescription();
        }

        void AddNewCamera(Vector3? position = null)
        {
            if (UpdateClosestPosition()) return;

            if (currentCamera is not null)
            {
                NotificationManager.Warn($"Still have a camera active");
                return;
            }

            currentCamera = new SpeedCamera();
            currentCamera.AddStart(position ?? Game.PlayerPed.Position);
            currentCamera.Width = width;
            NotificationManager.Success($"Start point added<br />{currentCamera.Start.Vector3}");
        }

        void AddEndPointToCamera(Vector3? position = null)
        {
            if (UpdateClosestPosition()) return;

            if (currentCamera is null)
            {
                UpdateClosestPosition();
                return;
            }

            currentCamera.AddEnd(position ?? Game.PlayerPed.Position);
            currentPoints.Add(currentCamera);
            currentCamera = null;
            NotificationManager.Success($"End point added<br />{currentCamera.End.Vector3}");
        }

        void StoreCamera()
        {
            if (currentPoints.Count == 0)
            {
                NotificationManager.Warn($"Need to have setup cameras");
                return;
            }

            foreach (SpeedCamera speedCamera in currentPoints)
            {
                pointsToSave.Add(speedCamera);
            }
            currentPoints.Clear();

            NotificationManager.Success($"Cameras Added to List");
        }

        void RemoveLastCamera()
        {
            currentPoints.RemoveAt(currentPoints.Count - 1);
            NotificationManager.Success($"Camera cleared");
        }

        bool UpdateClosestPosition()
        {
            Vector3 pos = Game.PlayerPed.Position;

            List<SpeedCamera> closestCameras = GetClosestCamera(pos, 50f);
            foreach (SpeedCamera cam in closestCameras)
            {
                if (cam.Start.Vector3.Distance(pos, true) < 5f)
                {
                    cam.AddStart(pos);
                    return true;
                }
                else if (cam.End.Vector3.Distance(pos, true) < 5f)
                {
                    cam.AddEnd(pos);
                    return true;
                }
            }

            return false;
        }

        void SaveStoredPositions()
        {
            string json = JsonConvert.SerializeObject(pointsToSave);
            Events.EventSystem.GetModule().Send("debug:camera:save", json);

            pointsToSave.ForEach(p => pointsSaved.Add(p));

            pointsToSave.Clear();
        }
    }
}
