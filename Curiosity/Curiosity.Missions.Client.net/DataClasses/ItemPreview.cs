using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Curiosity.Missions.Client.net.DataClasses
{
    class ItemPreview
    {
        static Client client = Client.GetInstance();

        private Vector3 _currentOffset;

        private Prop _currentPreview;

        private Prop _resultProp;

        private bool _preview;

        private bool _isDoor;

        private string _currnetPropHash;

        public bool PreviewComplete
        {
            get;
            private set;
        }

        public ItemPreview()
        {
            client.RegisterTickHandler(this.OnTick);
        }

        public void Abort()
        {
            Prop prop = this._currentPreview;
            if (prop != null)
            {
                prop.Delete();
            }
            else
            {
            }
        }

        private async void CreateItemPreview()
        {
            if (this._currentPreview != null)
            {
                Screen.DisplayHelpTextThisFrame("Press ~INPUT_AIM~ to cancel.\nPress ~INPUT_ATTACK~ to place the item.");

                Game.DisableControlThisFrame(2, (Control)25);
                Game.DisableControlThisFrame(2, (Control)24);
                Game.DisableControlThisFrame(2, (Control)257);
                Game.DisableControlThisFrame(2, (Control)152);
                Game.DisableControlThisFrame(2, (Control)153);
                Game.DisableControlThisFrame(2, (Control)44);
                Game.DisableControlThisFrame(2, (Control)27);
                Game.DisableControlThisFrame(2, (Control)172);
                Game.DisableControlThisFrame(2, (Control)173);
                Game.DisableControlThisFrame(2, (Control)21);
                API.BlockWeaponWheelThisFrame();
                
                if (!Game.IsDisabledControlPressed(2, (Control)25))
                {
                    Vector3 position = GameplayCamera.Position;
                    Vector3 direction = GameplayCamera.Direction;
                    RaycastResult raycastResult = World.Raycast(position, position + (direction * 15f), IntersectOptions.Everything, Game.PlayerPed);
                    Vector3 hitCoords = raycastResult.HitPosition;
                    if ((hitCoords == Vector3.Zero ? true : hitCoords.DistanceToSquared(Game.PlayerPed.Position) <= 1.5f))
                    {
                        this._currentPreview.IsVisible = false;
                    }
                    else
                    {
                        ItemPreview.DrawScaleForms();
                        float single = (Game.IsControlPressed(2, (Control)21) ? 1.5f : 1f);
                        if (Game.IsControlPressed(2, (Control)152))
                        {
                            Vector3 rotation = this._currentPreview.Rotation;
                            float z = rotation.Z;
                            z = z + Game.LastFrameTime * 50f * single;
                            this._currentPreview.Rotation = rotation;
                        }
                        else if (Game.IsControlPressed(2, (Control)153))
                        {
                            Vector3 vector3 = this._currentPreview.Rotation;
                            float lastFrameTime = vector3.Z;
                            lastFrameTime = lastFrameTime - Game.LastFrameTime * 50f * single;
                            this._currentPreview.Rotation = vector3;
                        }
                        if (Game.IsControlPressed(2, (Control)172))
                        {
                            float singlePointer = this._currentOffset.Z;
                            singlePointer = singlePointer + Game.LastFrameTime * single;
                        }
                        else if (Game.IsControlPressed(2, (Control)173))
                        {
                            float z1 = this._currentOffset.Z;
                            z1 = z1 - Game.LastFrameTime * single;
                        }
                        this._currentPreview.Position = (hitCoords + this._currentOffset);
                        this._currentPreview.IsVisible = true;
                        if (!Game.IsDisabledControlJustPressed(2, (Control)24))
                        {
                            return;
                        }
                        this._currentPreview.ResetOpacity();
                        this._resultProp = this._currentPreview;
                        this._resultProp.IsCollisionEnabled = true;
                        this._resultProp.IsPositionFrozen = !this._isDoor;
                        this._preview = false;
                        this._currentPreview = null;
                        this._currnetPropHash = string.Empty;
                        this.PreviewComplete = true;
                        client.DeregisterTickHandler(this.OnTick);
                    }
                }
                else
                {
                    this._currentPreview.Delete();
                    object obj = null;
                    Prop prop = (Prop)obj;
                    this._resultProp = (Prop)obj;
                    this._currentPreview = prop;
                    this._preview = false;
                    this.PreviewComplete = true;
                    client.DeregisterTickHandler(this.OnTick);
                }
            }
            else
            {
                this.PreviewComplete = false;
                this._currentOffset = Vector3.Zero;
                Model model = this._currnetPropHash;
                Vector3 vector31 = new Vector3();
                Vector3 vector32 = vector31;
                vector31 = new Vector3();
                Prop prop1 = await World.CreateProp(model, vector32, vector31, false, false);
                if (prop1 != null)
                {
                    prop1.IsCollisionEnabled = false;
                    this._currentPreview = prop1;
                    this._currentPreview.Opacity = 150;
                    Game.PlayerPed.Weapons.Select(unchecked((WeaponHash)(-1569615261)), true);
                    this._resultProp = null;
                }
                else
                {
                    Screen.DisplayHelpTextThisFrame(string.Format("Failed to load prop, even after request.\nProp Name: {0}", this._currnetPropHash));
                    this._resultProp = null;
                    this._preview = false;
                    this.PreviewComplete = true;
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

        public Prop GetResult()
        {
            return this._resultProp;
        }

        public async Task OnTick()
        {
            if (this._preview)
            {
                this.CreateItemPreview();
            }
            await Task.FromResult(0);
        }

        public void StartPreview(string propHash, Vector3 offset, bool isDoor)
        {
            if (!this._preview)
            {
                this._preview = true;
                this._currnetPropHash = propHash;
                this._isDoor = isDoor;
            }
        }
    }
}
