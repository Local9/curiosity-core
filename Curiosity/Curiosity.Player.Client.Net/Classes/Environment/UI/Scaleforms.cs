using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class Scaleforms
    {
        static Client client = Client.GetInstance();

        static bool scaleformActive = false;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Scalefrom:Announce", new Action<string>(Announcement));
        }

        static async void Announcement(string message)
        {
            if (scaleformActive) return;

            Scaleform scaleform = await ScaleformWrapper.Request("mp_big_message_freemode");
            scaleform.CallFunction("SHOW_SHARD_CENTERED_TOP_MP_MESSAGE", "~r~ANNOUNCEMENT", message, 1);
            ShowScaleform(scaleform);
        }

        static async void ShowScaleform(Scaleform scaleform)
        {
            int startTimer = API.GetGameTimer();

            while (scaleform.IsLoaded)
            {
                scaleformActive = true;
                await Client.Delay(0);
                if (API.GetGameTimer() - startTimer >= 10000)
                {
                    scaleformActive = false;
                    scaleform.Dispose();
                }
                scaleform.Render2D();
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
