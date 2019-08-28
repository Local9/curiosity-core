using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Global.Shared.net.Enums.Mobile;
using Curiosity.Mobile.Client.net.Mobile.Api;

namespace Curiosity.Mobile.Client.net.Mobile
{
    class MobilePhone
    {
        static Client client = Client.GetInstance();

        static int TOTAL_APPS = 9;

        static public bool IsMobilePhoneOpen = false;
        static public bool InSleepMode = false;

        static int theme = API.GetResourceKvpInt("vf_phone_theme");
        static int wallpaper = API.GetResourceKvpInt("vf_phone_wallpaper");

        public static int phoneTheme = 5;
        public static int phoneWallpaper = 11;
        static int visibleAnimProgress = 21;
        static int selectedItem = 4;

        static WeaponHash equipedWeapon = WeaponHash.Unarmed;

        public static bool IsJobActive = false;
        public static bool IsLeaning = false;
        public static bool IsMenuOpen = false;

        // app screen
        public static int MobileScaleform;

        public static void Init()
        {
            client.RegisterTickHandler(OnMobileCreationTick);
            client.RegisterTickHandler(OnMainAppTick);

            client.RegisterEventHandler("curiosity:Client:Menu:IsOpened", new Action<bool>(InteractionMenuOpen));
        }

        static void InteractionMenuOpen(bool isOpen)
        {
            IsMenuOpen = isOpen;
        }

        static void SetSoftKey(int scaleForm, SoftKey buttonId, Color color, SoftkeyIcon icon)
        {
            API.PushScaleformMovieFunction(scaleForm, "SET_SOFT_KEYS");
            API.PushScaleformMovieFunctionParameterInt((int)buttonId);
            API.PushScaleformMovieFunctionParameterBool(true);
            API.PushScaleformMovieFunctionParameterInt((int)icon);
            API.PopScaleformMovieFunctionVoid();

            //API.PushScaleformMovieFunction(MobileScaleform, "SET_SOFT_KEYS_COLOUR");
            //API.PushScaleformMovieFunctionParameterInt(buttonId);
            //API.PushScaleformMovieFunctionParameterInt(color.R);
            //API.PushScaleformMovieFunctionParameterInt(color.G);
            //API.PushScaleformMovieFunctionParameterInt(color.B);
            //API.PopScaleformMovieFunctionVoid();
        }

        public static async void LeanPhone(bool lean)
        {
            MobilePhone.IsLeaning = lean;
            API.SetPhoneLean(lean);

            if (lean)
                API.SetMobilePhoneRotation(-90.0f, 0.0f, 180.0f, 0);

            await Client.Delay(0);
        }

        static async Task OnMobileCreationTick()
        {
            if (Game.PlayerPed.IsDead)
            {
                await Task.FromResult(0);
                return;
            }

            if (IsMobilePhoneOpen)
            { 
                float scale = 0;
                if (API.GetFollowPedCamViewMode() == 4)
                    scale = 0f;
                else
                    scale = 285f;
                API.SetMobilePhoneScale(scale);

                if (!MobilePhone.IsLeaning)
                {
                    API.SetMobilePhonePosition(58.0f, -21.0f - visibleAnimProgress, -60.0f);
                    API.SetMobilePhoneRotation(-90.0f, visibleAnimProgress * 4.0f, 0.0f, 0);
                    if (visibleAnimProgress > 0)
                    {
                        visibleAnimProgress = visibleAnimProgress - 3;
                    }
                }

                int hours = 0;
                int minutes = 0;
                int seconds = 0;
                API.NetworkGetServerTime(ref hours, ref minutes, ref seconds);

                API.PushScaleformMovieFunction(MobileScaleform, "SET_TITLEBAR_TIME");
                API.PushScaleformMovieFunctionParameterInt(hours);
                API.PushScaleformMovieFunctionParameterInt(minutes);
                API.PopScaleformMovieFunctionVoid();

                API.PushScaleformMovieFunction(MobileScaleform, "SET_SLEEP_MODE");
                API.PushScaleformMovieFunctionParameterBool(InSleepMode);
                API.PopScaleformMovieFunctionVoid();

                API.PushScaleformMovieFunction(MobileScaleform, "SET_THEME");
                API.PushScaleformMovieFunctionParameterInt(phoneTheme);
                API.PopScaleformMovieFunctionVoid();

                API.PushScaleformMovieFunction(MobileScaleform, "SET_BACKGROUND_IMAGE");
                API.PushScaleformMovieFunctionParameterInt(phoneWallpaper);
                API.PopScaleformMovieFunctionVoid();

                Vector3 pos = Game.PlayerPed.Position;
                API.PushScaleformMovieFunction(MobileScaleform, "SET_SIGNAL_STRENGTH");
                // API.PushScaleformMovieFunctionParameterInt(API.GetZoneScumminess(API.GetZoneAtCoords(pos.X, pos.Y, pos.Z)));
                API.PushScaleformMovieFunctionParameterInt(0);
                API.PopScaleformMovieFunctionVoid();

                int renderID = 0;
                API.GetMobilePhoneRenderId(ref renderID);

                API.SetTextRenderId(renderID);

                //SetSoftKey(MobileScaleform, SoftKey.Left, Color.White, SoftkeyIcon.Select);
                //SetSoftKey(MobileScaleform, 2, Color.White, 0);
                //SetSoftKey(MobileScaleform, 3, Color.White, 0);

                API.DrawScaleformMovie(MobileScaleform, 0.0998f, 0.1775f, 0.1983f, 0.364f, 255, 255, 255, 255, 0);
                API.SetTextRenderId(1);
            }
            else if (ControlHelper.IsControlJustPressed(Control.Phone, false) && !IsMenuOpen)
            {
                if (Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed)
                {
                    equipedWeapon = Game.PlayerPed.Weapons.Current.Hash;
                    API.SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)WeaponHash.Unarmed, true);
                }

                API.PlaySoundFrontend(-1, "Pull_Out", "Phone_SoundSet_Default", true);
                MobileScaleform = API.RequestScaleformMovie("CELLPHONE_IFRUIT");
                if (!API.HasScaleformMovieLoaded(MobileScaleform))
                {
                    await Client.Delay(0);
                }

                Game.PlayerPed.SetConfigFlag(242, false);
                Game.PlayerPed.SetConfigFlag(243, false);
                Game.PlayerPed.SetConfigFlag(244, true);

                visibleAnimProgress = 21;
                IsMobilePhoneOpen = true;
                API.SetMobilePhonePosition(58.0f, -21.0f - visibleAnimProgress, -60.0f);

                float scale = 0;
                if (API.GetFollowPedCamViewMode() == 4)
                    scale = 0f;
                else
                    scale = 285f;
                API.SetMobilePhoneScale(scale);

                API.CreateMobilePhone(0);
                API.N_0x83a169eabcdb10a2(Game.PlayerPed.Handle, 4);
            }

            await Task.FromResult(0);
        }

        public async static void ClosePhone(bool closingApp = false)
        {
            if (closingApp)
            {
                ApplicationHandler.IsInApp = false;
                API.PlaySoundFrontend(-1, "Hang_Up", "Phone_SoundSet_Michael", true);
            }
            else
            {
                //Game.PlayerPed.SetConfigFlag(242, true);
                //Game.PlayerPed.SetConfigFlag(243, true);
                //Game.PlayerPed.SetConfigFlag(244, false);

                ApplicationHandler.Kill();
                IsMobilePhoneOpen = false;
                API.SetScaleformMovieAsNoLongerNeeded(ref MobileScaleform);
                API.DestroyMobilePhone();

                await Client.Delay(1000);

                if (equipedWeapon != WeaponHash.Unarmed)
                {
                    API.SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)equipedWeapon, true);
                }
            }

        }

        static async Task OnMainAppTick()
        {
            try
            {
                if (IsMobilePhoneOpen && !ApplicationHandler.IsInApp)
                {
                    int a = 0;
                    foreach(Application application in ApplicationHandler.Apps.OrderBy(x => x.GetPosition))
                    {
                        API.PushScaleformMovieFunction(MobileScaleform, "SET_DATA_SLOT");
                        API.PushScaleformMovieFunctionParameterInt(1);
                        API.PushScaleformMovieFunctionParameterInt(a);
                        API.PushScaleformMovieFunctionParameterInt((int)application.GetIcon);
                        API.PopScaleformMovieFunctionVoid();
                        a++;
                    }

                    // FILL
                    for (int i = ApplicationHandler.Apps.Count; i < TOTAL_APPS; i++)
                    {
                        API.PushScaleformMovieFunction(MobileScaleform, "SET_DATA_SLOT");
                        API.PushScaleformMovieFunctionParameterInt(1);
                        API.PushScaleformMovieFunctionParameterInt(i);
                        API.PushScaleformMovieFunctionParameterInt((int)Api.AppIcons.Empty);
                        API.PopScaleformMovieFunctionVoid();
                    }

                    API.PushScaleformMovieFunction(MobileScaleform, "DISPLAY_VIEW");
                    API.PushScaleformMovieFunctionParameterInt(1);
                    API.PushScaleformMovieFunctionParameterInt(selectedItem);
                    API.PopScaleformMovieFunctionVoid();

                    for (int i = 0; i < TOTAL_APPS; i++)
                    {
                        API.PushScaleformMovieFunction(MobileScaleform, "SET_HEADER");
                        API.PushScaleformMovieFunctionParameterString("Life V");
                        API.PopScaleformMovieFunctionVoid();
                    }

                    bool changeNavigation = true;

                    if (Game.IsControlJustPressed(0, Control.FrontendUp) ||
                            Game.IsDisabledControlJustPressed(0, Control.FrontendUp) ||
                            Game.IsControlJustPressed(0, Control.PhoneScrollBackward) ||
                            Game.IsDisabledControlJustPressed(0, Control.PhoneScrollBackward))
                    {
                        selectedItem = selectedItem - 3;
                        if (selectedItem < 0)
                            selectedItem = 9 + selectedItem;
                    }
                    else if (Game.IsControlJustPressed(0, Control.FrontendDown) ||
                            Game.IsDisabledControlJustPressed(0, Control.FrontendDown) ||
                            Game.IsControlJustPressed(0, Control.PhoneScrollForward) ||
                            Game.IsDisabledControlJustPressed(0, Control.PhoneScrollForward))
                    { 
                        selectedItem = selectedItem + 3;
                        if (selectedItem > 8)
                            selectedItem = selectedItem - 9;
                    }
                    else if (Game.IsControlJustPressed(0, Control.FrontendRight))
                    {
                        selectedItem = selectedItem + 1;
                        if (selectedItem > 8)
                            selectedItem = 0;
                    }
                    else if (Game.IsControlJustPressed(0, Control.FrontendLeft))
                    {
                        selectedItem = selectedItem - 1;
                        if (selectedItem < 0)
                            selectedItem = 8;
                    }
                    else
                    {
                        if (Game.IsControlJustPressed(0, Control.FrontendAccept))
                        {
                            if (ApplicationHandler.Apps[selectedItem] != null)
                                ApplicationHandler.Start(ApplicationHandler.Apps[selectedItem]);
                        }
                        else if (ControlHelper.IsControlJustPressed(Control.PhoneCancel, false))
                        {
                            ClosePhone();
                        }
                        changeNavigation = false;
                    }

                    if (changeNavigation)
                    {
                        API.PlaySoundFrontend(-1, "Menu_Navigate", "Phone_SoundSet_Default", true);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnMainAppTick -> {ex.ToString()}");
            }
            await Task.FromResult(0);
        }

        static bool IsDownPressed()
        {
            // Return false if the buttons are not currently enabled.
            if (!IsMenuOpen)
            {
                return false;
            }
            // when the player is holding TAB, while not in a vehicle, and when the scrollwheel is being used, return false to prevent interferring with weapon selection.
            if (!Game.PlayerPed.IsInVehicle())
            {
                if (Game.IsControlPressed(0, Control.SelectWeapon))
                {
                    if (Game.IsControlPressed(0, Control.SelectNextWeapon) || Game.IsControlPressed(0, Control.SelectPrevWeapon))
                    {
                        return false;
                    }
                }
            }
            // return true if the scrollwheel down or the arrow down key is being used at this frame.
            if (Game.IsControlPressed(0, Control.FrontendDown) ||
                Game.IsDisabledControlPressed(0, Control.FrontendDown) ||
                Game.IsControlPressed(0, Control.PhoneScrollForward) ||
                Game.IsDisabledControlPressed(0, Control.PhoneScrollForward))
            {
                return true;
            }

            // return false if none of the conditions matched.
            return false;
        }
    }
}
