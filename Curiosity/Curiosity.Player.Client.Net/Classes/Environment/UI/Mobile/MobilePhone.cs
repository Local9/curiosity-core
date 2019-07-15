﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Client.net.Classes.Environment.UI.Mobile.Api.Entity;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile
{
    class MobilePhone
    {
        static Client client = Client.GetInstance();

        static int TOTAL_APPS = 9;

        static public bool IsMobilePhoneOpen = false;
        static public bool InSleepMode = false;
        static public bool IsInApp = false;

        static int theme = API.GetResourceKvpInt("vf_phone_theme");
        static int wallpaper = API.GetResourceKvpInt("vf_phone_wallpaper");

        static int phoneTheme = 5;
        static int phoneWallpaper = 11;
        static int visibleAnimProgress = 21;
        static int selectedItem = 4;

        public static Dictionary<int, Application> applications = new Dictionary<int, Application>();

        // app screen
        static int mobileScaleform;

        public static void Init()
        {
            client.RegisterTickHandler(OnMobileCreationTick);
            client.RegisterTickHandler(OnMainAppTick);
            client.RegisterTickHandler(Test);

            AddApplication(Api.AppIcons.Empty);
            AddApplication(Api.AppIcons.Empty);
            AddApplication(Api.AppIcons.Contacts);

            AddApplication(Api.AppIcons.Empty);
            AddApplication(Api.AppIcons.Empty);
            AddApplication(Api.AppIcons.Empty);

            AddApplication(Api.AppIcons.Empty);
            AddApplication(Api.AppIcons.Settings);
            AddApplication(Api.AppIcons.Empty);
        }

        static async Task Test()
        {
            if (ControlHelper.IsControlJustPressed(Control.FrontendSocialClub))
            {
                API.DisplayOnscreenKeyboard(6, "FMMC_KEY_TIP8", "", "", "", "", "", 60);
                await BaseScript.Delay(100); // 100ms delay to prevent instant enter.
                while (API.UpdateOnscreenKeyboard() != 1 && API.UpdateOnscreenKeyboard() != 2)
                {
                    API.DisableAllControlActions(0);
                    await BaseScript.Delay(0);
                }
                if (API.UpdateOnscreenKeyboard() == 1)
                {
                    string message = API.GetOnscreenKeyboardResult();
                    API.SetNotificationTextEntry("STRING");
                    if (message.Length <= 0)
                    {
                        API.AddTextComponentString("~r~Message too short!");
                    }
                    else
                    {
                        API.AddTextComponentString("~g~Message sent! " + message);
                    }
                    API.DrawNotification(true, true);
                }
                API.EnableAllControlActions(0);
            }
        }

        static void AddApplication(Api.AppIcons appIcon)
        {
            if (applications.Count == 9) return;

            int id = applications.Count + 1;

            applications.Add(id, new Application() { Id = id, Name = $"App #{id}", Icon = appIcon });
        }

        static async Task OnMobileCreationTick()
        {
            if (IsMobilePhoneOpen)
            {
                API.SetMobilePhonePosition(58.0f, -21.0f - visibleAnimProgress, -60.0f);
                API.SetMobilePhoneRotation(-90.0f, visibleAnimProgress * 4.0f, 0.0f, 0);
                if (visibleAnimProgress > 0)
                {
                    visibleAnimProgress = visibleAnimProgress - 3;
                }

                int hours = 0;
                int minutes = 0;
                int seconds = 0;
                API.NetworkGetServerTime(ref hours, ref minutes, ref seconds);

                API.PushScaleformMovieFunction(mobileScaleform, "SET_TITLEBAR_TIME");
                API.PushScaleformMovieFunctionParameterInt(hours);
                API.PushScaleformMovieFunctionParameterInt(minutes);
                API.PopScaleformMovieFunctionVoid();

                API.PushScaleformMovieFunction(mobileScaleform, "SET_SLEEP_MODE");
                API.PushScaleformMovieFunctionParameterBool(InSleepMode);
                API.PopScaleformMovieFunctionVoid();

                API.PushScaleformMovieFunction(mobileScaleform, "SET_THEME");
                API.PushScaleformMovieFunctionParameterInt(phoneTheme);
                API.PopScaleformMovieFunctionVoid();

                API.PushScaleformMovieFunction(mobileScaleform, "SET_BACKGROUND_IMAGE");
                API.PushScaleformMovieFunctionParameterInt(phoneWallpaper);
                API.PopScaleformMovieFunctionVoid();

                Vector3 pos = Game.PlayerPed.Position;
                API.PushScaleformMovieFunction(mobileScaleform, "SET_SIGNAL_STRENGTH");
                API.PushScaleformMovieFunctionParameterInt(API.GetZoneScumminess(API.GetZoneAtCoords(pos.X, pos.Y, pos.Z)));
                API.PopScaleformMovieFunctionVoid();

                int renderID = 0;
                API.GetMobilePhoneRenderId(ref renderID);

                API.SetTextRenderId(renderID);

                API.DrawScaleformMovie(mobileScaleform, 0.0998f, 0.1775f, 0.1983f, 0.364f, 255, 255, 255, 255, 0);
                API.SetTextRenderId(1);
            }
            else if (ControlHelper.IsControlJustPressed(Control.ReplayFfwd))
            {
                API.PlaySoundFrontend(-1, "Pull_Out", "Phone_SoundSet_Default", true);
                mobileScaleform = API.RequestScaleformMovie("CELLPHONE_IFRUIT");
                if (!API.HasScaleformMovieLoaded(mobileScaleform))
                {
                    await Client.Delay(0);
                }
                visibleAnimProgress = 21;
                IsMobilePhoneOpen = true;
                API.SetMobilePhonePosition(58.0f, -21.0f - visibleAnimProgress, -60.0f);
                API.SetMobilePhoneScale(285.0f);
                API.CreateMobilePhone(0);
            }

            await Task.FromResult(0);
        }

        static void ClosePhone(bool closingApp = false)
        {
            if (closingApp)
            {
                IsInApp = false;
                API.PlaySoundFrontend(-1, "Hang_Up", "Phone_SoundSet_Michael", true);
            }
            else
            {
                IsMobilePhoneOpen = false;
                API.SetScaleformMovieAsNoLongerNeeded(ref mobileScaleform);
                API.DestroyMobilePhone();
            }

        }

        static async Task OnMainAppTick()
        {
            try
            {
                if (IsMobilePhoneOpen && !IsInApp)
                {
                    for (int i = 0; i < TOTAL_APPS; i++)
                    {
                        API.PushScaleformMovieFunction(mobileScaleform, "SET_DATA_SLOT");
                        API.PushScaleformMovieFunctionParameterInt(1);
                        API.PushScaleformMovieFunctionParameterInt(i);
                        // Need to loop over applications

                        if (applications.ContainsKey(i + 1))
                        {
                            API.PushScaleformMovieFunctionParameterInt((int)applications[i + 1].Icon);
                        }
                        else
                        {
                            API.PushScaleformMovieFunctionParameterInt(3);
                        }

                        API.PopScaleformMovieFunctionVoid();
                    }

                    API.PushScaleformMovieFunction(mobileScaleform, "DISPLAY_VIEW");
                    API.PushScaleformMovieFunctionParameterInt(1);
                    API.PushScaleformMovieFunctionParameterInt(selectedItem);
                    API.PopScaleformMovieFunctionVoid();

                    for (int i = 0; i < TOTAL_APPS; i++)
                    {
                        API.PushScaleformMovieFunction(mobileScaleform, "SET_HEADER");
                        API.PushScaleformMovieFunctionParameterString("Life V");
                        API.PopScaleformMovieFunctionVoid();
                    }

                    bool changeNavigation = true;

                    if (ControlHelper.IsControlJustPressed(Control.ReplayFfwd))
                    {
                        selectedItem = selectedItem - 3;
                        if (selectedItem < 0)
                            selectedItem = 9 + selectedItem;
                    }
                    else if (ControlHelper.IsControlJustPressed(Control.ReplayRewind))
                    {
                        selectedItem = selectedItem + 3;
                        if (selectedItem > 8)
                            selectedItem = selectedItem - 9;
                    }
                    else if (ControlHelper.IsControlJustPressed(Control.ReplayAdvance))
                    {
                        selectedItem = selectedItem + 1;
                        if (selectedItem > 8)
                            selectedItem = 0;
                    }
                    else if (ControlHelper.IsControlJustPressed(Control.ReplayBack))
                    {
                        selectedItem = selectedItem - 1;
                        if (selectedItem < 0)
                            selectedItem = 9;
                    }
                    else
                    {
                        if (ControlHelper.IsControlJustPressed(Control.CreatorAccept))
                        {
                                
                        }
                        else if (ControlHelper.IsControlJustPressed(Control.FrontendCancel))
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
                Debug.WriteLine($"{ex.Message}");
            }
            await Task.FromResult(0);
        }
    }
}
