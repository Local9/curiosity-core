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

        Model droneModel = "ch_prop_casino_drone_02a";
        Prop droneProp;

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
                    API.SetPlayerInvisibleLocally(Game.Player.Handle, IsEnabled);
                }

                Ped ped = Cache.PlayerPed;

                if (!IsEnabled)
                {
                    if (CurrentCamera != null)
                    {
                        CurrentCamera.Delete();
                        CurrentCamera = null;
                        World.RenderingCamera = null;

                        Vector3 pos = Game.PlayerPed.Position;
                        float groundZ = pos.Z;
                        if (API.GetGroundZFor_3dCoord_2(pos.X, pos.Y, pos.Z, ref groundZ, false))
                            pos = new Vector3(pos.X, pos.Y, groundZ);

                        float waterHeight = pos.Z;

                        if (API.TestVerticalProbeAgainstAllWater(pos.X, pos.Y, pos.Z, 0, ref waterHeight))
                        {
                            pos.Z = waterHeight;
                        }

                        Game.PlayerPed.Position = pos;

                        ped.IsPositionFrozen = false;
                        ped.IsCollisionEnabled = true;
                        ped.CanRagdoll = true;
                        ped.IsVisible = true;
                        ped.Task.ClearAllImmediately();

                        if (droneModel is not null)
                        {
                            if (droneModel.IsLoaded)
                                droneModel.MarkAsNoLongerNeeded();
                        }

                        if (droneProp is not null)
                        {
                            if (droneProp.Exists())
                            {
                                droneProp.Detach();
                                droneProp.Delete();
                            }

                            droneProp.MarkAsNoLongerNeeded();
                            droneProp = null;
                        }

                        await BaseScript.Delay(100);
                    }
                    return;
                }

                // Create camera on toggle
                if (CurrentCamera == null)
                {
                    CurrentCamera = World.CreateCamera(ped.Position, GameplayCamera.Rotation, 75f);
                    CurrentCamera.AttachTo(ped, new Vector3(0f, 0f, 0.5f));
                    World.RenderingCamera = CurrentCamera;
                    ped.IsPositionFrozen = true;
                    ped.IsCollisionEnabled = false;
                    ped.CanRagdoll = false;
                    ped.IsVisible = false;
                    ped.Task.ClearAllImmediately();

                    if (droneProp is null)
                    {
                        if (!droneModel.IsLoaded)
                            await droneModel.Request(5000);

                        //droneProp = await World.CreateProp(droneModel, ped.Position, false, false);
                        //droneProp.AttachTo(ped, new Vector3(0f, 0f, 0.5f), new Vector3(0f, 0f, 180f));
                        //droneProp.IsPositionFrozen = true;
                    }
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
                    ped.PositionNoOffset = ped.Position + CurrentCamera.UpVector * (Speed * multiplier);
                }
                // Backward
                else if (Game.IsControlPressed(2, Control.MoveUpDown))
                {
                    ped.PositionNoOffset = ped.Position - CurrentCamera.UpVector * (Speed * multiplier);
                }
                // Left
                if (Game.IsControlPressed(2, Control.MoveLeftOnly))
                {
                    var pos = ped.GetOffsetPosition(new Vector3(-Speed * multiplier, 0f, 0f));
                    ped.PositionNoOffset = new Vector3(pos.X, pos.Y, ped.Position.Z);
                }
                // Right
                else if (Game.IsControlPressed(2, Control.MoveLeftRight))
                {
                    var pos = ped.GetOffsetPosition(new Vector3(Speed * multiplier, 0f, 0f));
                    ped.PositionNoOffset = new Vector3(pos.X, pos.Y, ped.Position.Z);
                }

                // Up (E)
                if (Game.IsControlPressed(2, Control.Context))
                {
                    ped.PositionNoOffset = ped.GetOffsetPosition(new Vector3(0f, 0f, multiplier * Speed / 2));
                }

                // Down (Q)
                if (Game.IsControlPressed(2, Control.ContextSecondary))
                {
                    ped.PositionNoOffset = ped.GetOffsetPosition(new Vector3(0f, 0f, multiplier * -Speed / 2));
                }


                // Disable controls
                foreach (var ctrl in DisabledControls)
                {
                    Game.DisableControlThisFrame(2, ctrl);
                }

                ped.Heading = Math.Max(0f, (360 + CurrentCamera.Rotation.Z) % 360f);

                if (droneProp is not null)
                {
                    if (droneProp.Exists())
                        API.SetEntityLocallyInvisible(droneProp.Handle);
                }
                API.DisablePlayerFiring(Game.Player.Handle, false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public void UpdatePlayerDrone()
        {
            Ped ped = Game.PlayerPed;
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

                if (droneProp is not null)
                    droneProp.Rotation = rotation;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
