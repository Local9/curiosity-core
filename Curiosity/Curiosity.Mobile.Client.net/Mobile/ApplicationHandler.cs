using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Mobile.Client.net.Mobile.Api;
using System.Collections.Generic;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Mobile.Client.net.Mobile
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
        public static bool IsSendingEvent = false;

        public static void ChangeScreen(dynamic[] dynamics)
        {
            if (dynamics != null)
            {
                PreviousScreens.Add(CurrentAppScreen);
                CurrentAppScreen = dynamics[0];
            }
        }

        public static void PlaySound(string audio, string audioSet)
        {
            API.PlaySoundFrontend(-1, audio, audioSet, true);
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

            if (CurrentApp != null)
            {
                if (CurrentApp.StopTask != null)
                {
                    CurrentApp.StopTask.Invoke();
                }
            }

            API.SetPhoneLean(false);
            MobilePhone.IsLeaning = false;
            CitizenFX.Core.Native.API.SetMobilePhoneRotation(0.0f, 0.0f, 0.0f, 0);

            CurrentApp = null;
            API.PlaySoundFrontend(-1, "Hang_Up", "Phone_SoundSet_Michael", true);
            
            client.DeregisterTickHandler(CreateScreen);
        }

        public static async Task CreateScreen()
        {
            while (CurrentApp != null && !IsInKeyboard)
            {
                Game.DisableControlThisFrame(0, Control.InteractionMenu);
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
                    // CLEARS PREVIOUS ITEMS....
                    API.PushScaleformMovieFunction(MobilePhone.MobileScaleform, "SET_DATA_SLOT_EMPTY");
                    API.PushScaleformMovieFunctionParameterInt(CurrentAppScreen.Type);
                    API.PopScaleformMovieFunctionVoid();

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
                    if (Game.IsControlJustPressed(0, Control.FrontendUp) ||
                            Game.IsDisabledControlJustPressed(0, Control.FrontendUp) ||
                            Game.IsControlJustPressed(0, Control.PhoneScrollBackward) ||
                            Game.IsDisabledControlJustPressed(0, Control.PhoneScrollBackward))
                    {
                        SelectedItem = SelectedItem - 1;
                        if (SelectedItem < 0)
                            SelectedItem = CurrentAppScreen.Items.Count - 1;
                    }
                    else if (Game.IsControlJustPressed(0, Control.FrontendDown) ||
                            Game.IsDisabledControlJustPressed(0, Control.FrontendDown) ||
                            Game.IsControlJustPressed(0, Control.PhoneScrollForward) ||
                            Game.IsDisabledControlJustPressed(0, Control.PhoneScrollForward))
                    {
                        SelectedItem = SelectedItem + 1;
                        if (SelectedItem > CurrentAppScreen.Items.Count - 1)
                            SelectedItem = CurrentAppScreen.Items.Count - 1;
                    }
                    else if (Game.IsControlJustPressed(0, Control.FrontendAccept))
                    {
                        Item item = CurrentAppScreen.Items[SelectedItem];
                        item.Select();
                    }
                    else if (Game.IsControlJustPressed(0, Control.FrontendCancel))
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
                await Client.Delay(0);
            }
            await Task.FromResult(0);
        }

    }
}
