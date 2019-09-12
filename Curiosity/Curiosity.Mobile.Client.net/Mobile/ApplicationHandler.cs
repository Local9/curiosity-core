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

        public static void Start(Application application)
        {
            if (application.LauncherScreen != null)
            {
                CurrentApp = application;
                CurrentAppScreen = CurrentApp.LauncherScreen;
                PreviousScreens.Clear();
                SelectedItem = 0;
                IsInApp = true;
                Game.PlaySound("Menu_Navigate", "Phone_SoundSet_Default");
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
            Game.PlaySound("Hang_Up", "Phone_SoundSet_Michael");
            
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
                    MobilePhone.MobileScaleform.CallFunction("SET_DATA_SLOT_EMPTY", (int)CurrentAppScreen.Type);

                    string header = string.Empty;
                    if (!string.IsNullOrEmpty(CurrentAppScreen.Header))
                    {
                        header = CurrentAppScreen.Header;
                    }
                    else
                    {
                        header = CurrentApp.GetName;
                    }

                    MobilePhone.MobileScaleform.CallFunction("SET_HEADER", header);

                    for(var i = 0; i < CurrentAppScreen.Items.Count; i++)
                    {
                        API.PushScaleformMovieFunction(MobilePhone.MobileScaleformHandle, "SET_DATA_SLOT");
                        API.PushScaleformMovieFunctionParameterInt((int)CurrentAppScreen.Type);
                        API.PushScaleformMovieFunctionParameterInt(i);
                        foreach(ItemData itemData in CurrentAppScreen.Items[i].Data)
                        {
                            itemData.Push();
                        }
                        API.PopScaleformMovieFunctionVoid();
                    }

                    MobilePhone.MobileScaleform.CallFunction("DISPLAY_VIEW", (int)CurrentAppScreen.Type, SelectedItem);

                    if (SelectedItem > CurrentAppScreen.Items.Count)
                        SelectedItem = CurrentAppScreen.Items.Count - 1;

                    bool navigated = true;
                    if (Game.IsControlJustPressed(1, Control.PhoneUp))
                    {
                        API.MoveFinger(1);
                        if (SelectedItem > 0)
                        {
                            SelectedItem--;
                        }
                        else
                        {
                            SelectedItem = CurrentAppScreen.Items.Count - 1;
                        }
                    }
                    else if (Game.IsControlJustPressed(1, Control.PhoneDown))
                    {
                        API.MoveFinger(2);
                        if (SelectedItem < CurrentAppScreen.Items.Count - 1)
                        {
                            SelectedItem++;
                        }
                        else
                        {
                            SelectedItem = 0;
                        }
                    }
                    else if (Game.IsControlJustPressed(1, Control.PhoneSelect))
                    {
                        API.MoveFinger(5);
                        Item item = CurrentAppScreen.Items[SelectedItem];
                        item.Select();
                    }
                    else if (Game.IsControlJustPressed(1, Control.PhoneCancel))
                    {
                        API.MoveFinger(5);
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
                        Game.PlaySound("Menu_Navigate", "Phone_SoundSet_Default");
                    }
                }
                await Client.Delay(0);
            }
            await Task.FromResult(0);
        }

    }
}
