using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Police.Client.Environment.Entities;
using Curiosity.Police.Client.Environment.Entities.Models;
using Curiosity.Police.Client.Events;
using Curiosity.Police.Client.Interface;
using Curiosity.Police.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Police.Client.Commands.Impl
{
    public class DeveloperTools : CommandContext
    {
        public override string[] Aliases { get; set; } = { "pd" };
        public override string Title { get; set; } = "Police Development";
        public override Color Color { get; set; } = Color.FromArgb(0, 255, 0);
        public override bool IsRestricted { get; set; } = true;
        public override List<Role> RequiredRoles { get; set; } = new List<Role>() { Role.DEVELOPER, Role.PROJECT_MANAGER };

        [CommandInfo(new[] { "cameras" })]
        public class SpeedCameraCommand : ICommand
        {
            List<PoliceCamera> _currentSpeedCameras = new();
            List<PoliceCamera> _newSpeedCameras = new();
            SpeedCameraMetadata speedCameraMetadata = new();
            ConfigurationManager configurationManager => ConfigurationManager.GetModule();
            bool _enabled;

            public async void On(CuriosityPlayer player, List<string> arguments)
            {
                if (arguments.Count > 0)
                {
                    if (arguments[0] == "save")
                    {
                        foreach (PoliceCamera policeCamera in _newSpeedCameras)
                        {
                            policeCamera.Saved = true;
                            speedCameraMetadata.cameras.Add(policeCamera);
                            _currentSpeedCameras.Add(policeCamera);
                        }

                        bool res = await EventSystem.GetModule().Request<bool>("debug:camera:save", speedCameraMetadata);
                        if (res)
                            Screen.ShowNotification($"~g~Saved Cameras");
                        if (!res)
                            Screen.ShowNotification($"~r~Failed saving Cameras");

                        speedCameraMetadata.cameras.Clear();
                        _newSpeedCameras.Clear();
                    }

                    return;
                }

                // Enable way to add a camera to visualise its position
                // save out the data in a file so the information can be easily copied
                _enabled = !_enabled;

                if (_enabled)
                {
                    _currentSpeedCameras = configurationManager.SpeedCameras;

                    PluginManager.Instance.AttachTickHandler(OnControl);
                    PluginManager.Instance.AttachTickHandler(OnShowNewCameras);
                    PluginManager.Instance.AttachTickHandler(OnShowCurrentCameras);
                }

                if (!_enabled)
                {
                    PluginManager.Instance.DetachTickHandler(OnControl);
                    PluginManager.Instance.DetachTickHandler(OnShowNewCameras);
                    PluginManager.Instance.DetachTickHandler(OnShowCurrentCameras);
                }
                string msg = _enabled ? "~g~On" : "~r~Off";
                Screen.ShowNotification($"Debug: { msg }");
            }

            private async Task OnControl()
            {
                if (Game.IsControlJustPressed(0, Control.SelectWeaponUnarmed))
                {
                    AddSpeedCamera();
                }

                if (Game.IsControlJustPressed(0, Control.SelectWeaponMelee))
                {
                    AddSpeedCamera(true);
                }

                if (Game.IsControlJustPressed(0, Control.Jump))
                {
                    foreach(var speedCam in _newSpeedCameras.ToArray())
                    {
                        if (Game.PlayerPed.IsInRangeOf(speedCam.Position, 3f))
                            _newSpeedCameras.Remove(speedCam);
                    }
                }
            }

            private void AddSpeedCamera(bool overrideLimit = false)
            {
                Vector3 pos = Game.PlayerPed.Position;
                string dir = Cache.CuriosityPlayer.GetHeadingDirection();

                uint streetHash = 0;
                uint crossingRoad = 0;
                GetStreetNameAtCoord(pos.X, pos.Y, pos.Z, ref streetHash, ref crossingRoad);
                string street = GetStreetNameFromHashKey(streetHash);

                PoliceCamera speedCamera = new PoliceCamera() { Street = street, Direction = dir, X = pos.X, Y = pos.Y, Z = pos.Z, Limit = overrideLimit ? 35 : null };
                _newSpeedCameras.Add(speedCamera);
            }

            private async Task OnShowCurrentCameras()
            {
                foreach(PoliceCamera speedCamera in _currentSpeedCameras)
                {
                    Color color = Color.FromArgb(120, speedCamera.Limit is null ? 255 : 0, speedCamera.Limit is not null ? 255 : 0, 0);

                    if (speedCamera.Active)
                        color = Color.FromArgb(120, 0, 0, 255);

                    World.DrawMarker(MarkerType.DebugSphere, speedCamera.Position, Vector3.Zero, Vector3.Zero, new Vector3(configurationManager.SpeedCameraDistance), color);
                    ScreenInterface.Draw3DText(speedCamera.Position, $"{speedCamera.Street}~n~{speedCamera}");
                }
            }

            private async Task OnShowNewCameras()
            {
                foreach (PoliceCamera speedCamera in _newSpeedCameras)
                {
                    Color color = Color.FromArgb(120, speedCamera.Limit is null ? 255 : 0, speedCamera.Limit is not null ? 255 : 0, 0);

                    if (!speedCamera.Saved && speedCamera.Limit is null)
                        color = Color.FromArgb(120, 255, 255, 0);

                    if (!speedCamera.Saved && speedCamera.Limit is not null)
                        color = Color.FromArgb(120, 0, 255, 255);

                    World.DrawMarker(MarkerType.DebugSphere, speedCamera.Position, Vector3.Zero, Vector3.Zero, new Vector3(configurationManager.SpeedCameraDistance), color);
                    ScreenInterface.Draw3DText(speedCamera.Position, $"{speedCamera.Street}~n~{speedCamera}");
                }
            }
        }
    }
}
