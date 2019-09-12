using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
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

        public static int currentHours = 9;
        public static int currentMinutes = 0;

        private static bool SmoothTimeTransitionsEnabled = true;
        private static bool DontDoTimeSyncRightNow = false;
        private static bool FreezeTime = false;

        private static int minuteTimer = GetGameTimer();
        private static int minuteClockSpeed = 10000;

        private static int currentServerHours = currentHours;
        private static int currentServerMinutes = currentMinutes;

        // app screen
        public static Scaleform MobileScaleform;
        public static int MobileScaleformHandle;

        public static void Init()
        {
            client.RegisterTickHandler(OnMobileCreationTick);
            client.RegisterTickHandler(OnMainAppTick);
            client.RegisterTickHandler(TimeSyncEvent);

            client.RegisterEventHandler("curiosity:Player:World:SetTime", new Action<int, int, bool>(OnTimeSync));
            client.RegisterEventHandler("curiosity:Client:Menu:IsOpened", new Action<bool>(InteractionMenuOpen));
        }

        static async void OnTimeSync(int newHours, int newMinutes, bool freezeTime)
        {
            currentServerHours = newHours;
            currentServerMinutes = newMinutes;

            bool IsTimeDifferenceTooSmall()
            {
                var totalDifference = 0;
                totalDifference += (newHours - currentHours) * 60;
                totalDifference += (newMinutes - currentMinutes);

                if (totalDifference < 15 && totalDifference > -120)
                    return true;

                return false;
            }

            FreezeTime = freezeTime;

            if (SmoothTimeTransitionsEnabled && !IsTimeDifferenceTooSmall())
            {
                if (!DontDoTimeSyncRightNow)
                {
                    bool frozen = freezeTime;
                    DontDoTimeSyncRightNow = true;
                    FreezeTime = freezeTime;

                    var oldSpeed = minuteClockSpeed;

                    while (currentHours != currentServerHours || currentMinutes != currentServerMinutes)
                    {
                        FreezeTime = false;
                        await Client.Delay(0);
                        minuteClockSpeed = 1;
                    }
                    FreezeTime = freezeTime;

                    minuteClockSpeed = oldSpeed;

                    DontDoTimeSyncRightNow = false;
                }
            }
            else
            {
                currentHours = currentServerHours;
                currentMinutes = currentServerMinutes;
            }
        }

        static private async Task TimeSyncEvent()
        {
            // If time is frozen...
            if (FreezeTime)
            {
                // Time is set every tick to make sure it never changes (even with some lag).
                await Client.Delay(0);
            }
            // Otherwise...
            else
            {
                if (minuteClockSpeed > 2000)
                {
                    await Client.Delay(2000);
                }
                else
                {
                    await Client.Delay(minuteClockSpeed);
                }
                // only add a minute if the timer has reached the configured duration (2000ms (2s) by default).
                if (GetGameTimer() - minuteTimer > minuteClockSpeed)
                {
                    currentMinutes++;
                    minuteTimer = GetGameTimer();
                }

                if (currentMinutes > 59)
                {
                    currentMinutes = 0;
                    currentHours++;
                }
                if (currentHours > 23)
                {
                    currentHours = 0;
                }
            }
        }

        static void InteractionMenuOpen(bool isOpen)
        {
            IsMenuOpen = isOpen;
        }

        static void SetSoftKey(SoftKey buttonId, Color color, SoftkeyIcon icon)
        {
            MobileScaleform.CallFunction("SET_SOFT_KEYS", (int)buttonId, 1, (int)icon);

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
                Game.DisableControlThisFrame(0, Control.Sprint);

                if (API.GetFollowPedCamViewMode() == 4)
                    API.SetMobilePhoneScale(0f);
                else
                    API.SetMobilePhoneScale(285f);

                if (!MobilePhone.IsLeaning)
                {
                    API.SetMobilePhonePosition(58.0f, -21.0f - visibleAnimProgress, -60.0f);
                    API.SetMobilePhoneRotation(-90.0f, visibleAnimProgress * 4.0f, 0.0f, 0);
                    if (visibleAnimProgress > 0)
                    {
                        visibleAnimProgress = visibleAnimProgress - 3;
                    }
                }

                MobileScaleform.CallFunction("SET_TITLEBAR_TIME", currentHours, currentMinutes);
                MobileScaleform.CallFunction("SET_SLEEP_MODE", InSleepMode);
                MobileScaleform.CallFunction("SET_THEME", phoneTheme);
                MobileScaleform.CallFunction("SET_BACKGROUND_IMAGE", phoneWallpaper);

                Vector3 pos = Game.PlayerPed.Position;
                MobileScaleform.CallFunction("SET_SIGNAL_STRENGTH", 0);
                // API.PushScaleformMovieFunctionParameterInt(API.GetZoneScumminess(API.GetZoneAtCoords(pos.X, pos.Y, pos.Z)));

                int renderID = 0;
                API.GetMobilePhoneRenderId(ref renderID);
                API.SetTextRenderId(renderID);

                //SetSoftKey(SoftKey.Left, Color.White, SoftkeyIcon.Select);
                //SetSoftKey(MobileScaleform, 2, Color.White, 0);
                //SetSoftKey(MobileScaleform, 3, Color.White, 0);

                API.DrawScaleformMovie(MobileScaleformHandle, 0.0998f, 0.1775f, 0.1983f, 0.364f, 255, 255, 255, 255, 0);
                API.SetTextRenderId(1);
            }
            else if (Game.IsControlJustPressed(1, Control.Phone) && !IsMenuOpen)
            {
                if (Game.PlayerPed.Weapons.Current.Hash != WeaponHash.Unarmed)
                {
                    equipedWeapon = Game.PlayerPed.Weapons.Current.Hash;
                    API.SetCurrentPedWeapon(Game.PlayerPed.Handle, (uint)WeaponHash.Unarmed, true);
                }

                Game.PlaySound("Pull_Out", "Phone_SoundSet_Default");
                MobileScaleform = new Scaleform("CELLPHONE_IFRUIT");

                MobileScaleformHandle = MobileScaleform.Handle;
                if (!MobileScaleform.IsLoaded)
                    await Client.Delay(0);

                API.CreateMobilePhone(0);

                Game.PlayerPed.SetConfigFlag(242, false);
                Game.PlayerPed.SetConfigFlag(243, false);
                Game.PlayerPed.SetConfigFlag(244, true);

                visibleAnimProgress = 21;
                API.N_0x83a169eabcdb10a2(Game.PlayerPed.Handle, 4);

                IsMobilePhoneOpen = true;
                API.SetMobilePhonePosition(58.0f, -21.0f - visibleAnimProgress, -60.0f);

                if (API.GetFollowPedCamViewMode() == 4)
                    API.SetMobilePhoneScale(0f);
                else
                    API.SetMobilePhoneScale(285f);
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
                ApplicationHandler.Kill();
                IsMobilePhoneOpen = false;
                MobileScaleform.CallFunction("SHUTDOWN_MOVIE");
                MobileScaleform.Dispose();
                API.SetScaleformMovieAsNoLongerNeeded(ref MobileScaleformHandle);
                API.DestroyMobilePhone();

                Game.PlayerPed.SetConfigFlag(242, true);
                Game.PlayerPed.SetConfigFlag(243, true);
                Game.PlayerPed.SetConfigFlag(244, false);

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
                        MobileScaleform.CallFunction("SET_DATA_SLOT", 1, a, (int)application.GetIcon);
                        a++;
                    }

                    // FILL
                    for (int i = ApplicationHandler.Apps.Count; i < TOTAL_APPS; i++)
                    {
                        MobileScaleform.CallFunction("SET_DATA_SLOT", 1, i, (int)Api.AppIcons.Empty);
                    }

                    MobileScaleform.CallFunction("DISPLAY_VIEW", 1, selectedItem);
                    MobileScaleform.CallFunction("SET_HEADER", "Life V");

                    bool changeNavigation = true;

                    if (Game.IsControlJustPressed(1, Control.PhoneUp))
                    {
                        API.MoveFinger(1);
                        selectedItem = selectedItem - 3;
                        if (selectedItem < 0)
                            selectedItem = 9 + selectedItem;
                    }
                    else if (Game.IsControlJustPressed(1, Control.PhoneDown))
                    {
                        API.MoveFinger(2);
                        selectedItem = selectedItem + 3;
                        if (selectedItem > 8)
                            selectedItem = selectedItem - 9;
                    }
                    else if (Game.IsControlJustPressed(1, Control.PhoneRight))
                    {
                        API.MoveFinger(4);
                        selectedItem = selectedItem + 1;
                        if (selectedItem > 8)
                            selectedItem = 0;
                    }
                    else if (Game.IsControlJustPressed(1, Control.PhoneLeft))
                    {
                        API.MoveFinger(3);
                        selectedItem = selectedItem - 1;
                        if (selectedItem < 0)
                            selectedItem = 8;
                    }
                    else
                    {
                        if (Game.IsControlJustPressed(1, Control.PhoneSelect))
                        {
                            API.MoveFinger(5);
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
    }
}
