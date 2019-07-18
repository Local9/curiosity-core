﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.Environment.UI.Mobile.Api;
using System.Collections.Generic;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Client.net.Classes.Environment.UI.Mobile
{
    class ApplicationHandler
    {
        static Client client = Client.GetInstance();

        public static List<Application> Apps = new List<Application>();

        public static Application CurrentApp;
        public static Screen CurrentAppScreen;
        public static List<Screen> PreviousScreens = new List<Screen>();
        public static int SelectedItem;

        public static bool IsInApp = false;
        public static bool IsInKeyboard = false;

        public static void ChangeScreen(dynamic[] dynamics)
        {
            if (dynamics != null)
            {
                PreviousScreens.Add(CurrentAppScreen);
                CurrentAppScreen = dynamics[0];
            }
        }

        public static void Start(Application application)
        {
            if (application.LauncherScreen != null)
            {
                CurrentApp = application;
                CurrentAppScreen = CurrentApp.LauncherScreen;
                PreviousScreens.Clear();
                SelectedItem = 0;
                IsInApp = true;
                API.PlaySoundFrontend(-1, "Menu_Navigate", "Phone_SoundSet_Default", true);
                if (CurrentApp.StartTask != null)
                {
                    CurrentApp.StartTask.Invoke();
                }
                client.RegisterTickHandler(CreateScreen);
            }
        }

        public static void Kill()
        {
            IsInApp = false;

            if (CurrentApp.StopTask != null)
            {
                CurrentApp.StopTask.Invoke();
            }

            CurrentApp = null;
            API.PlaySoundFrontend(-1, "Hang_Up", "Phone_SoundSet_Michael", true);
            
            client.DeregisterTickHandler(CreateScreen);
        }

        public static async Task CreateScreen()
        {
            if (CurrentApp != null && !IsInKeyboard)
            {
                if (CurrentAppScreen == null)
                {
                    if (PreviousScreens == null)
                    {
                        Kill();
                    }
                    else
                    {
                        bool screenFound = false;
                        foreach(Screen screen in PreviousScreens)
                        {
                            if (!screenFound)
                            {
                                CurrentAppScreen = screen;
                                screenFound = true;
                                PreviousScreens.Remove(screen);
                            }
                        }
                        if (!screenFound)
                        {
                            Kill();
                        }
                    }
                }
                else
                {
                    //API.PushScaleformMovieFunction(MobilePhone.MobileScaleform, "SET_DATA_SLOT_EMPTY");
                    //API.PushScaleformMovieFunctionParameterInt(CurrentAppScreen.Type);
                    //API.PopScaleformMovieFunctionVoid();

                    string header = string.Empty;
                    if (!string.IsNullOrEmpty(CurrentAppScreen.Header))
                    {
                        header = CurrentAppScreen.Header;
                    }
                    else
                    {
                        header = CurrentApp.GetName;
                    }

                    API.PushScaleformMovieFunction(MobilePhone.MobileScaleform, "SET_HEADER");
                    API.PushScaleformMovieFunctionParameterString(header);
                    API.PopScaleformMovieFunctionVoid();

                    int i = 0;

                    foreach(Item item in CurrentAppScreen.Items)
                    {
                        API.PushScaleformMovieFunction(MobilePhone.MobileScaleform, "SET_DATA_SLOT");
                        API.PushScaleformMovieFunctionParameterInt(CurrentAppScreen.Type);
                        API.PushScaleformMovieFunctionParameterInt(i);
                        foreach(ItemData itemData in item.Data)
                        {
                            itemData.Push();
                        }
                        API.PopScaleformMovieFunctionVoid();
                        i++;
                    }

                    API.PushScaleformMovieFunction(MobilePhone.MobileScaleform, "DISPLAY_VIEW");
                    API.PushScaleformMovieFunctionParameterInt(CurrentAppScreen.Type);
                    API.PushScaleformMovieFunctionParameterInt(SelectedItem);

                    if (SelectedItem > CurrentAppScreen.Items.Count - 1)
                        SelectedItem = CurrentAppScreen.Items.Count - 1;

                    bool navigated = true;
                    if (ControlHelper.IsControlJustPressed(Control.ReplayFfwd))
                    {
                        SelectedItem = SelectedItem - 1;
                        if (SelectedItem < 0)
                            SelectedItem = CurrentAppScreen.Items.Count - 1;
                    }
                    else if (ControlHelper.IsControlJustPressed(Control.ReplayRewind))
                    {
                        SelectedItem = SelectedItem + 1;
                        if (SelectedItem > CurrentAppScreen.Items.Count - 1)
                            SelectedItem = CurrentAppScreen.Items.Count - 1;
                    }
                    else if (ControlHelper.IsControlJustPressed(Control.CreatorAccept))
                    {
                        Item item = CurrentAppScreen.Items[SelectedItem];
                        item.Select();
                    }
                    else if (ControlHelper.IsControlJustPressed(Control.FrontendCancel))
                    {
                        if (PreviousScreens.Count > 0)
                        {
                            CurrentAppScreen = PreviousScreens[PreviousScreens.Count - 1];
                            SelectedItem = 0;
                            PreviousScreens.Remove(CurrentAppScreen);
                        }
                        else
                        {
                            await BaseScript.Delay(0);
                            Kill();
                            navigated = false;
                        }
                    }
                    else
                    {
                        navigated = false;
                    }

                    if (navigated)
                    {
                        API.PlaySoundFrontend(-1, "Menu_Navigate", "Phone_SoundSet_Default", true);
                    }
                }
            }
            await Task.FromResult(0);
        }

    }
}
