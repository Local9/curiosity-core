using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class NoClipManager : Manager<NoClipManager>
    {
        private const float MinY = -89f, MaxY = 89f;
        private const float MaxSpeed = 32f;

        public static NoClipManager NoClipInstance;

        private static readonly List<Control> DisabledControls = new List<Control> {
            Control.MoveLeftOnly,
            Control.MoveLeftRight,
            Control.MoveUpDown,
            Control.MoveUpOnly,
            Control.SelectNextWeapon,
            Control.SelectPrevWeapon,
            Control.WeaponWheelLeftRight,
            Control.WeaponWheelUpDown,
            Control.WeaponWheelNext,
            Control.WeaponWheelPrev,
            Control.Duck
        };

        public bool IsEnabled { get; set; }

        public float Speed { get; set; } = 1f;

        public Camera CurrentCamera { get; set; }

        public override void Begin()
        {
            NoClipInstance = this;
        }

        public void Init()
        {
            Instance.AttachTickHandler(OnNoClipControlTick);
            Instance.AttachTickHandler(OnNoClipCheckRotationTick);
        }

        public void Dispose()
        {
            Instance.DetachTickHandler(OnNoClipControlTick);
            Instance.DetachTickHandler(OnNoClipCheckRotationTick);
        }

        private async Task OnNoClipControlTick()
        {
            try
            {

                if (ControlHelper.IsControlJustPressed(Control.SaveReplayClip) && Cache.Player.User.IsDeveloper)
                {
                    IsEnabled = !IsEnabled;
                }

                if (!IsEnabled)
                {
                    if (CurrentCamera != null)
                    {
                        CurrentCamera.Delete();
                        CurrentCamera = null;
                        World.RenderingCamera = null;
                        Cache.PlayerPed.IsPositionFrozen = false;
                        Cache.PlayerPed.IsCollisionEnabled = true;
                        Cache.PlayerPed.CanRagdoll = true;
                        Cache.PlayerPed.IsVisible = true;
                        Cache.PlayerPed.Opacity = 255;
                        Cache.PlayerPed.Task.ClearAllImmediately();
                        await BaseScript.Delay(100);
                    }
                    return;
                }

                // Create camera on toggle
                if (CurrentCamera == null)
                {
                    CurrentCamera = World.CreateCamera(Cache.PlayerPed.Position, GameplayCamera.Rotation, 75f);
                    CurrentCamera.AttachTo(Cache.PlayerPed, Vector3.Zero);
                    World.RenderingCamera = CurrentCamera;
                    Cache.PlayerPed.IsPositionFrozen = true;
                    Cache.PlayerPed.IsCollisionEnabled = false;
                    Cache.PlayerPed.Opacity = 0;
                    Cache.PlayerPed.CanRagdoll = false;
                    Cache.PlayerPed.IsVisible = false;
                    Cache.PlayerPed.Task.ClearAllImmediately();
                }

                // Speed Control
                if (Game.IsControlPressed(2, Control.SelectPrevWeapon))
                {
                    Speed = Math.Min(Speed + 0.1f, MaxSpeed);
                }
                else if (Game.IsControlPressed(2, Control.SelectNextWeapon))
                {
                    Speed = Math.Max(0.1f, Speed - 0.1f);
                }

                var multiplier = 1f;
                if (Game.IsControlPressed(2, Control.FrontendLs))
                {
                    multiplier = 2f;
                }
                else if (Game.IsControlPressed(2, Control.CharacterWheel))
                {
                    multiplier = 4f;
                }
                else if (Game.IsControlPressed(2, Control.Duck))
                {
                    multiplier = 0.25f;
                }

                // Forward
                if (Game.IsControlPressed(2, Control.MoveUpOnly))
                {
                    Cache.PlayerPed.PositionNoOffset = Cache.PlayerPed.Position + CurrentCamera.UpVector * (Speed * multiplier);
                }
                // Backward
                else if (Game.IsControlPressed(2, Control.MoveUpDown))
                {
                    Cache.PlayerPed.PositionNoOffset = Cache.PlayerPed.Position - CurrentCamera.UpVector * (Speed * multiplier);
                }
                // Left
                if (Game.IsControlPressed(2, Control.MoveLeftOnly))
                {
                    var pos = Cache.PlayerPed.GetOffsetPosition(new Vector3(-Speed * multiplier, 0f, 0f));
                    Cache.PlayerPed.PositionNoOffset = new Vector3(pos.X, pos.Y, Cache.PlayerPed.Position.Z);
                }
                // Right
                else if (Game.IsControlPressed(2, Control.MoveLeftRight))
                {
                    var pos = Cache.PlayerPed.GetOffsetPosition(new Vector3(Speed * multiplier, 0f, 0f));
                    Cache.PlayerPed.PositionNoOffset = new Vector3(pos.X, pos.Y, Cache.PlayerPed.Position.Z);
                }

                // Up (E)
                if (Game.IsControlPressed(2, Control.Context))
                {
                    Cache.PlayerPed.PositionNoOffset = Cache.PlayerPed.GetOffsetPosition(new Vector3(0f, 0f, multiplier * Speed / 2));
                }

                // Down (Q)
                if (Game.IsControlPressed(2, Control.ContextSecondary))
                {
                    Cache.PlayerPed.PositionNoOffset = Cache.PlayerPed.GetOffsetPosition(new Vector3(0f, 0f, multiplier * -Speed / 2));
                }


                // Disable controls
                foreach (var ctrl in DisabledControls)
                {
                    Game.DisableControlThisFrame(2, ctrl);
                }

                Cache.PlayerPed.Heading = Math.Max(0f, (360 + CurrentCamera.Rotation.Z) % 360f);
                Cache.PlayerPed.Opacity = 0;
                API.DisablePlayerFiring(Game.Player.Handle, false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async Task OnNoClipCheckRotationTick()
        {
            try
            {
                if (CurrentCamera == null)
                {
                    await BaseScript.Delay(100);
                    return;
                }

                var rightAxisX = Game.GetDisabledControlNormal(0, (Control)220);
                var rightAxisY = Game.GetDisabledControlNormal(0, (Control)221);

                if (!(Math.Abs(rightAxisX) > 0) && !(Math.Abs(rightAxisY) > 0)) return;
                var rotation = CurrentCamera.Rotation;
                rotation.Z += rightAxisX * -10f;

                var yValue = rightAxisY * -5f;
                if (rotation.X + yValue > MinY && rotation.X + yValue < MaxY)
                    rotation.X += yValue;
                CurrentCamera.Rotation = rotation;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
