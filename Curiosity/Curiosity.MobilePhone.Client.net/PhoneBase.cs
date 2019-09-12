using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.MobilePhone.Client.net.Models;
using Curiosity.MobilePhone.Client.net.Apps;
using Curiosity.MobilePhone.Client.net.ClientData;
using Newtonsoft.Json;

namespace Curiosity.MobilePhone.Client.net
{
    public class PhoneBase : BaseScript
    {
        public string Data;
        private enum ModelPhone : int
        {
            Micheal = 0,
            Franklin = 1,
            Trevor = 2,
            Prologue = 4
        }

        private static bool wasBackOverridenByApp = false;
        private Phone phone;
        private List<App> apps;
        private App mainApp;
        private App currentApp = null;

        public PhoneBase()
        {
            EventHandlers.Add("lprp:phone_start", new Action<string>(StartApp));
            EventHandlers.Add("lprp:setupPhoneClientUser", new Action<string>(Setup));
            //			EventHandlers.Add("phone:receiveMessage", new Action<string, string>(ReceiveMessage));

            Tick += OnTick;

            // Debug
            Game.PlayerPed.Task.ClearAllImmediately();
            var e = Game.PlayerPed.GetEntityAttachedTo();
            if (e != null)
            {
                e.Delete();
            }
        }

        //		public int messageCount = 0;
        //		public Dictionary<string, string> pedHeadshots = new Dictionary<string, string>();


        /*		public async void ReceiveMessage(string sender, string message)
                {
                    string txdString;
                    if (!pedHeadshots.ContainsKey(sender))
                    {
                        int handle = RegisterPedheadshot(GetPlayerFromName(sender));
                        if (IsPedheadshotValid(handle))
                        {
                            while (!IsPedheadshotReady(handle)) await Delay(50);
                            txdString = GetPedheadshotTxdString(handle);
                        }
                        else
                            txdString = "CHAR_DEFAULT";
                    }
                    else
                        txdString = pedHeadshots[sender];
                    Func.ShowAdvancedNotification(sender, "Messaggio Privato", message, txdString, 2);
                    AddMessage(phone.Scaleform, messageCount, sender, message, false);
                    messageCount += 1;
                }

                public void AddMessage(Scaleform scaleform, int index, string email, string messageTopic, bool sending)
                {
                    Function.Call(Hash._BEGIN_TEXT_COMPONENT, "STRING");
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, messageTopic, -1);
                    Function.Call(Hash._END_TEXT_COMPONENT);
                    Function.Call(Hash._BEGIN_TEXT_COMPONENT, "STRING");
                    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_PLAYER_NAME, email, -1);
                    Function.Call(Hash._END_TEXT_COMPONENT);
                    if (!sending)
                        scaleform.CallFunction("SET_DATA_SLOT", 8, index, 0, 0);
                    else
                        scaleform.CallFunction("SET_DATA_SLOT", 8, index, 4, 0);
                }

        */
        public int GetPlayerFromName(string namePart)
        {
            foreach (Player p in Players)
                if (p.Name.Contains(namePart)) return p.Handle;
            return 0;
        }

        public async void Setup(string data)
        {
            Data = data;
        }

        private async void StartApp(string app)
        {
            if (app == "Main")
            {
                await KillApp();
                if (currentApp != null)
                {
                    currentApp.Kill();
                    Tick -= currentApp.Tick;
                }

                currentApp = mainApp;
            }
            else if (apps.Exists(x => x.Name == app))
            {
                Tick -= mainApp.Tick;
                currentApp = apps.FirstOrDefault(x => x.Name == app);
            }

            currentApp.Initialize(phone);
            Tick += currentApp.Tick;

            Debug.WriteLine($"CurrentApp = {currentApp.Name}");
        }

        private async Task KillApp()
        {
            if (currentApp != null)
            {
                Debug.WriteLine($"Killing App {currentApp.Name}");
                Tick -= currentApp.Tick;
                currentApp.Kill();

                var lastApp = currentApp;
                currentApp = null;

                if (lastApp.Name == "Main")
                {
                    foreach (var app in apps)
                    {
                        app.Kill();
                        Tick -= app.Tick;
                    }

                    phone.Kill();
                }
                else
                {
                    Game.PlaySound("Menu_Navigate", "Phone_SoundSet_Default");
                    StartApp("Main");
                }
            }

            await Task.FromResult(0);
        }

        private async Task OnTick()
        {
            try
            {
                if (Game.PlayerPed.IsAlive)
                {
                    if (phone == null)
                    {
                        if (Data != "{\"phone_data\":[]}")
                        {
                            phone = new Phone(JsonConvert.DeserializeObject<Phone>(Data)) { Scaleform = new Scaleform("CELLPHONE_IFRUIT") };
                            apps = new List<App> { new Messages(phone), new Contacts(phone), new QuickSave(phone), new Settings(phone) };
                            mainApp = new PhoneMain(phone, apps);
                            return;
                        }
                        else
                        {
                            phone = new Phone() { Scaleform = new Scaleform("CELLPHONE_IFRUIT") };
                            apps = new List<App> { new Messages(phone), new Contacts(phone), new QuickSave(phone), new Settings(phone) };
                            mainApp = new PhoneMain(phone, apps);
                            return;
                        }
                    }

                    if (!phone.Visible)
                    {
                        if (Game.IsControlJustPressed(1, Control.Phone) && !IsPedRunningMobilePhoneTask(Game.PlayerPed.Handle))
                        {
                            Game.PlaySound("Pull_Out", "Phone_SoundSet_Default");

                            phone.Scaleform = new Scaleform("CELLPHONE_IFRUIT");

                            phone.AnimationIn();

                            while (!phone.Scaleform.IsLoaded) { await Delay(0); }
                            CreateMobilePhone((int)ModelPhone.Micheal);
                            StartApp("Main");
                            Game.PlayerPed.SetConfigFlag(242, false);
                            Game.PlayerPed.SetConfigFlag(243, false);
                            Game.PlayerPed.SetConfigFlag(244, true);
                            phone.VisibleAnimProgress = 21;
                            N_0x83a169eabcdb10a2(Game.PlayerPed.Handle, 4);
                            await Delay(20);
                            if (GetFollowPedCamViewMode() == 4)
                                SetMobilePhoneScale(0f);
                            else
                                SetMobilePhoneScale(285f);
                            //phone.AnimationIdle();
                            return;
                        }
                    }
                    else
                    {
                        if (currentApp == null) { return; }
                        if (IsPedRunningMobilePhoneTask(Game.PlayerPed.Handle))
                        {
                            Game.DisableControlThisFrame(0, Control.Sprint);

                            SetMobilePhonePosition(60f, -21f - phone.VisibleAnimProgress, -60f);
                            SetMobilePhoneRotation(-90f, phone.VisibleAnimProgress * 2f, 0f, 0);

                            if (phone.VisibleAnimProgress > 0)
                                phone.VisibleAnimProgress -= 3;

                            var time = World.CurrentDayTime;
                            phone.Scaleform.CallFunction("SET_TITLEBAR_TIME", time.Hours, time.Minutes);

                            phone.Scaleform.CallFunction("SET_SLEEP_MODE", phone.SleepMode);
                            phone.Scaleform.CallFunction("SET_THEME", phone.getCurrentCharPhone().Theme);
                            phone.Scaleform.CallFunction("SET_BACKGROUND_IMAGE", phone.getCurrentCharPhone().Wallpaper);

                            var playerPos = Game.PlayerPed.Position;
                            phone.Scaleform.CallFunction("SET_SIGNAL_STRENGTH", GetZoneScumminess(GetZoneAtCoords(playerPos.X, playerPos.Y, playerPos.Z)));
                            float scale = 0;
                            if (GetFollowPedCamViewMode() == 4)
                                scale = 0f;
                            else
                                scale = 285f;
                            SetMobilePhoneScale(scale);
                            int renderId = 0;
                            GetMobilePhoneRenderId(ref renderId);
                            SetTextRenderId(renderId);
                            DrawScaleformMovie(phone.Scaleform.Handle, 0.0998f, 0.1775f, 0.1983f, 0.364f, 255, 255, 255, 255, 0);
                            SetTextRenderId(1);


                            if (currentApp.OverrideBack)
                            {
                                wasBackOverridenByApp = true;
                            }
                            else
                            {
                                wasBackOverridenByApp = false;
                            }

                            if (Game.IsControlJustPressed(1, Control.PhoneCancel))
                            {
                                if (wasBackOverridenByApp)
                                {
                                    wasBackOverridenByApp = false;
                                }
                                else
                                {
                                    await KillApp();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.Message} : Exception thrown on PhoneBase.OnTick()");
            }

            await Task.FromResult(0);
        }
    }

    public enum SoftKeyIcon
    {
        Blank = 1,
        Select = 2,
        Pages = 3,
        Back = 4,
        Call = 5,
        Hangup = 6,
        HangupHuman = 7,
        Week = 8,
        Keypad = 9,
        Open = 10,
        Reply = 11,
        Delete = 12,
        Yes = 13,
        No = 14,
        Sort = 15,
        Website = 16,
        Police = 17,
        Ambulance = 18,
        Fire = 19,
        Pages2 = 20
    }
    public sealed class Wallpapers
    {
        private Wallpapers() { }

        public static readonly string iFruitDefault = "Phone_Wallpaper_ifruitdefault";
        public static readonly string BadgerDefault = "Phone_Wallpaper_badgerdefault";
        public static readonly string Bittersweet = "Phone_Wallpaper_bittersweet_b";
        public static readonly string PurpleGlow = "Phone_Wallpaper_purpleglow";
        public static readonly string GreenSquares = "Phone_Wallpaper_greensquares";
        public static readonly string OrangeHerringBone = "Phone_Wallpaper_orangeherringbone";
        public static readonly string OrangeHalftone = "Phone_Wallpaper_orangehalftone";
        public static readonly string GreenTriangles = "Phone_Wallpaper_greentriangles";
        public static readonly string GreenShards = "Phone_Wallpaper_greenshards";
        public static readonly string BlueAngles = "Phone_Wallpaper_blueangles";
        public static readonly string BlueShards = "Phone_Wallpaper_blueshards";
        public static readonly string BlueTriangles = "Phone_Wallpaper_bluetriangles";
        public static readonly string BlueCircles = "Phone_Wallpaper_bluecircles";
        public static readonly string Diamonds = "Phone_Wallpaper_diamonds";
        public static readonly string GreenGlow = "Phone_Wallpaper_greenglow";
        public static readonly string Orange8Bit = "Phone_Wallpaper_orange8bit";
        public static readonly string OrangeTriangles = "Phone_Wallpaper_orangetriangles";
        public static readonly string PurpleTartan = "Phone_Wallpaper_purpletartan";
    }

}
