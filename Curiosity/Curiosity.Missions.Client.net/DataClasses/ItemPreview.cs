using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Classes.Environment;

namespace Curiosity.Missions.Client.net.DataClasses
{
    class ItemPreview
    {
        static Client client = Client.GetInstance();

        private static Vector3 _currentOffset;

        private static Prop _currentPreview;

        private static Prop _resultProp;

        private static bool _preview;

        private static bool _isDoor;

        private static string _currnetPropHash;

        public static bool PreviewComplete
        {
            get;
            private set;
        }

        public static void Abort()
        {
            Prop prop = _currentPreview;
            if (prop != null)
            {
                prop.Delete();
            }
            else
            {
            }
        }

        private static async void CreateItemPreview()
        {
            if (_currentPreview != null)
            {
                Screen.DisplayHelpTextThisFrame("Press ~INPUT_AIM~ to cancel.\nPress ~INPUT_ATTACK~ to place the item.");

                Game.DisableControlThisFrame(2, Control.Aim);
                Game.DisableControlThisFrame(2, Control.Attack);
                Game.DisableControlThisFrame(2, Control.Attack2);
                Game.DisableControlThisFrame(2, Control.ParachuteBrakeLeft);
                Game.DisableControlThisFrame(2, Control.ParachuteBrakeRight);
                Game.DisableControlThisFrame(2, Control.Cover);
                Game.DisableControlThisFrame(2, Control.Phone);
                Game.DisableControlThisFrame(2, Control.PhoneUp);
                Game.DisableControlThisFrame(2, Control.PhoneDown);
                Game.DisableControlThisFrame(2, Control.Sprint);
                API.HideHudComponentThisFrame(19);
                //API.BlockWeaponWheelThisFrame2();

                if (!Game.IsDisabledControlPressed(2, Control.Aim))
                {
                    Vector3 position = GameplayCamera.Position;
                    
                    Vector3 camRot = API.GetGameplayCamRot(0);

                    Vector3 direction = ScreenToWorld.RotationToDirection(camRot);

                    RaycastResult raycastResult = World.Raycast(position, position + (direction * 15f), IntersectOptions.Everything, Game.PlayerPed);
                    Vector3 hitCoords = raycastResult.HitPosition;
                    if ((hitCoords == Vector3.Zero ? true : hitCoords.DistanceToSquared(Game.PlayerPed.Position) <= 1.5f))
                    {
                        _currentPreview.IsVisible = false;
                    }
                    else
                    {
                        ItemPreview.DrawScaleForms();
                        float single = (Game.IsDisabledControlPressed(2, Control.Sprint) ? 3f : 1f);

                        if (Game.IsDisabledControlPressed(2, Control.ParachuteBrakeLeft))
                        {
                            Vector3 rotation = _currentPreview.Rotation;
                            float z = rotation.Z;
                            z = z + Game.LastFrameTime * 50f * single;
                            rotation.Z = z;
                            _currentPreview.Rotation = rotation;
                        }
                        else if (Game.IsDisabledControlPressed(2, Control.ParachuteBrakeRight))
                        {
                            Vector3 vector3 = _currentPreview.Rotation;
                            float lastFrameTime = vector3.Z;
                            lastFrameTime = lastFrameTime - Game.LastFrameTime * 50f * single;
                            vector3.Z = lastFrameTime;
                            _currentPreview.Rotation = vector3;
                        }

                        if (Game.IsDisabledControlPressed(2, Control.PhoneUp))
                        {
                            float singlePointer = _currentOffset.Z;
                            singlePointer = singlePointer + Game.LastFrameTime * single;
                            _currentOffset.Z = singlePointer;
                        }
                        else if (Game.IsDisabledControlPressed(2, Control.PhoneDown))
                        {
                            float z1 = _currentOffset.Z;
                            z1 = z1 - Game.LastFrameTime * single;
                            _currentOffset.Z = z1;
                        }

                        _currentPreview.Position = (hitCoords + _currentOffset);
                        _currentPreview.IsVisible = true;

                        if (!Game.IsDisabledControlJustPressed(2, Control.Attack))
                        {
                            return;
                        }

                        _currentPreview.ResetOpacity();
                        _resultProp = _currentPreview;
                        _resultProp.IsCollisionEnabled = true;
                        _resultProp.IsPositionFrozen = !_isDoor;
                        _preview = false;
                        _currentPreview = null;
                        _currnetPropHash = string.Empty;
                        PreviewComplete = true;
                        client.DeregisterTickHandler(OnTick);

                        Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
                        Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
                    }
                }
                else
                {
                    _currentPreview.Delete();
                    object obj = null;
                    Prop prop = (Prop)obj;
                    _resultProp = (Prop)obj;
                    _currentPreview = prop;
                    _preview = false;
                    PreviewComplete = true;
                    client.DeregisterTickHandler(OnTick);

                    Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
                    Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
                }
            }
            else
            {
                PreviewComplete = false;
                _currentOffset = Vector3.Zero;
                Model model = _currnetPropHash;
                Vector3 vector31 = new Vector3();
                Vector3 vector32 = vector31;
                vector31 = new Vector3();
                Prop prop1 = await World.CreateProp(model, vector32, vector31, false, false);
                if (prop1 != null)
                {
                    prop1.IsCollisionEnabled = false;
                    _currentPreview = prop1;
                    _currentPreview.Opacity = 150;
                    Game.PlayerPed.Weapons.Select(unchecked((WeaponHash)(-1569615261)), true);
                    _resultProp = null;
                }
                else
                {
                    Screen.DisplayHelpTextThisFrame($"Failed to load prop, even after request.\nProp Name: {_currnetPropHash}");
                    _resultProp = null;
                    _preview = false;
                    PreviewComplete = true;
                }

                if (PreviewComplete)
                {
                    Client.TriggerEvent("curiosity:Client:UI:LocationHide", false);
                    Client.TriggerEvent("curiosity:Client:Menu:IsOpened", false);
                }
            }
        }

        private static void DrawScaleForms()
        {
            Scaleform scaleform = new Scaleform("instructional_buttons");
            scaleform.CallFunction("CLEAR_ALL", new object[0]);
            scaleform.CallFunction("TOGGLE_MOUSE_BUTTONS", new object[] { 0 });
            scaleform.CallFunction("CREATE_CONTAINER", new object[0]);
            object[] empty = new object[] { 0, null, null };
            empty[1] = Function.Call<string>((Hash)331533201183454215L, new InputArgument[] { 2, 152, 0 });
            empty[2] = string.Empty;
            scaleform.CallFunction("SET_DATA_SLOT", empty);
            object[] objArray = new object[] { 1, null, null };
            objArray[1] = Function.Call<string>((Hash)331533201183454215L, new InputArgument[] { 2, 153, 0 });
            objArray[2] = "Rotate";
            scaleform.CallFunction("SET_DATA_SLOT", objArray);
            object[] empty1 = new object[] { 2, null, null };
            empty1[1] = Function.Call<string>((Hash)331533201183454215L, new InputArgument[] { 2, 172, 0 });
            empty1[2] = string.Empty;
            scaleform.CallFunction("SET_DATA_SLOT", empty1);
            object[] objArray1 = new object[] { 3, null, null };
            objArray1[1] = Function.Call<string>((Hash)331533201183454215L, new InputArgument[] { 2, 173, 0 });
            objArray1[2] = "Lift/Lower";
            scaleform.CallFunction("SET_DATA_SLOT", objArray1);
            object[] objArray2 = new object[] { 4, null, null };
            objArray2[1] = Function.Call<string>((Hash)331533201183454215L, new InputArgument[] { 2, 21, 0 });
            objArray2[2] = "Accelerate";
            scaleform.CallFunction("SET_DATA_SLOT", objArray2);
            scaleform.CallFunction("DRAW_INSTRUCTIONAL_BUTTONS", new object[] { -1 });
            scaleform.Render2D();
        }

        public static Prop GetResult()
        {
            return _resultProp;
        }

        public static async Task OnTick()
        {
            if (_preview)
            {
                CreateItemPreview();
            }
            await Task.FromResult(0);
        }

        public static void StartPreview(string propHash, Vector3 offset, bool isDoor)
        {
            if (!_preview)
            {
                _preview = true;
                _currnetPropHash = propHash;
                _isDoor = isDoor;

                client.RegisterTickHandler(OnTick);

                Client.TriggerEvent("curiosity:Client:UI:LocationHide", true);
                Client.TriggerEvent("curiosity:Client:Menu:IsOpened", true);
            }
        }
    }
}
