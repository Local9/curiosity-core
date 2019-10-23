using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Helper;
using System.Drawing;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class Scaleforms
    {
        static Client client = Client.GetInstance();

        static bool scaleformActive = false;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Scalefrom:Announce", new Action<string>(Announcement));
            client.RegisterEventHandler("curiosity:Client:Scalefrom:MissionComplete", new Action<string>(MissionComplete));
        }

        public static async void MissionComplete(string message)
        {
            if (scaleformActive) return;

            Scaleform scaleform = await ScaleformWrapper.Request("MISSION_COMPLETE");

            int handle = scaleform.Handle;

            scaleform.CallFunction("SET_MISSION_TITLE", "", "Mission Title Thing");
            scaleform.CallFunction("SET_DATA_SLOT", 0, 0, 5, 0, 1, "Rescued Hostage");

            scaleform.CallFunction("SET_DATA_SLOT", 1, 0, 6, "ESDOLLA", 100, "Paid");
            scaleform.CallFunction("SET_DATA_SLOT", 2, 0, 6, "ESMINDOLLA", 100, "Pay loss");

            scaleform.CallFunction("SET_MISSION_TITLE_COLOUR", 255, 153, 51);
            scaleform.CallFunction("SET_MISSION_SUBTITLE_COLOUR", 0, 0, 0);
            scaleform.CallFunction("SET_MISSION_BG_COLOUR", 255, 153, 51);

            // 3 %
            // 4 string:string
            // 5 fraction
            // 6 dollar
            // 7 blank line

            // 9 Checkbox after
            // 10 Checkbox before

            scaleform.CallFunction("DRAW_MENU_LIST");

            ShowScaleform(scaleform, duration: 10000, scale: true);
        }

        public static async void Announcement(string message)
        {
            if (scaleformActive) return;

            Scaleform scaleform = await ScaleformWrapper.Request("mp_big_message_freemode");
            scaleform.CallFunction("SHOW_SHARD_CENTERED_TOP_MP_MESSAGE", "~r~ANNOUNCEMENT", message, 1);
            ShowScaleform(scaleform);
        }

        public static async void Wasted()
        {
            if (scaleformActive) return;

            Game.PlaySound("Bed", "WastedSounds");
            Screen.Effects.Start(ScreenEffect.DeathFailMpDark, 3000);
            Scaleform scaleform = await ScaleformWrapper.Request("mp_big_message_freemode");
            scaleform.CallFunction("SHOW_SHARD_WASTED_MP_MESSAGE", "~r~WASTED", "", 1);
            ShowScaleform(scaleform, 3000);
        }

        static async void ShowScaleform(Scaleform scaleform, int duration = 10000, bool scale = false, float scaleX = 0.2021f, float scaleY = 0.5111f, float x = 0.5f, float y = 0.5f)
        {
            int startTimer = API.GetGameTimer();
            bool setScale = scale;

            PointF pointScale = new PointF(scaleX, scaleY);
            PointF location = new PointF(x, y);

            while (scaleform.IsLoaded)
            {
                scaleformActive = true;
                await Client.Delay(0);
                if (API.GetGameTimer() - startTimer >= duration)
                {
                    scaleformActive = false;
                    scaleform.Dispose();
                }

                if (setScale)
                {
                    API.DrawScaleformMovie(scaleform.Handle, x, y, scaleX, scaleY, 150, 150, 150, 0, 0);
                    // scaleform.Render2DScreenSpace(location, pointScale);
                }
                else
                {
                    scaleform.Render2D();
                }
            }
        }

        static async Task OnTick()
        {
            if (ControlHelper.IsControlJustPressed(Control.MultiplayerInfo))
            {
                Scaleform scaleform = await ScaleformWrapper.Request("mp_big_message_freemode");
                scaleform.CallFunction("SHOW_SHARD_CENTERED_TOP_MP_MESSAGE", "YOU KNOW ITS SIMPLE", "YOU KNOW ITS SIMPLE2", 5);
                // scaleform.CallFunction("SHOW_BIG_MP_MESSAGE_WITH_STRAP", "MISSION PASSED", "RESPECT+");
                // scaleform.CallFunction("SHOW_TERRITORY_CHANGE_MP_MESSAGE", "MISSION PASSED", "RESPECT+", "RESPECT+++");
                //scaleform.CallFunction("SHOW_MP_MESSAGE_TOP", 1, "YOU KNOW ITS SIMPLE", "YOU KNOW ITS SIMPLE2", 5);

                //Scaleform scaleform = await ScaleformWrapper.Request("MISSION_COMPLETE");
                //scaleform.CallFunction("SET_MISSION_TITLE", 1, "TITLE", 1);
                //scaleform.CallFunction("SET_DATA_SLOT", 1, "SOMTHIN");
                //scaleform.CallFunction("SET_TOTAL", 1, 20, "20 Somthing");
                //scaleform.CallFunction("SET_DATA_SLOT", 1);
                //scaleform.CallFunction("SET_TOTAL", 1, 30, "30 Somthing");
                //scaleform.CallFunction("SET_DATA_SLOT", 2);

                int startTimer = API.GetGameTimer();

                while (scaleform.IsLoaded)
                {
                    await Client.Delay(0);
                    if (API.GetGameTimer() - startTimer >= 10000)
                    {
                        scaleform.Dispose();
                    }
                    scaleform.Render2D();
                }
            }
            await Task.FromResult(0);
        }
    }
}
