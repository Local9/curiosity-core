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
    class ScaleformTester
    {
        public static void Init()
        {
            Client.GetInstance().RegisterTickHandler(OnTick);
        }

        static async Task OnTick()
        {
            if (ControlHelper.IsControlJustPressed(Control.MultiplayerInfo))
            {
                Scaleform scaleform = await ScaleformWrapper.Request("mp_big_message_freemode");
                //scaleform.CallFunction("SHOW_SHARD_CENTERED_TOP_MP_MESSAGE", "YOU KNOW ITS SIMPLE", "YOU KNOW ITS SIMPLE2", 5);
                scaleform.CallFunction("SHOW_BIG_MP_MESSAGE_WITH_STRAP", "MISSION PASSED", "RESPECT+");

                //scaleform.CallFunction("SHOW_MP_MESSAGE_TOP", 1, "YOU KNOW ITS SIMPLE", "YOU KNOW ITS SIMPLE2", 5);

                int startTimer = API.GetGameTimer();

                while (scaleform.IsLoaded)
                {
                    await Client.Delay(0);
                    if (API.GetGameTimer() - startTimer >= 10000)
                    {
                        scaleform.Dispose();
                    }
                    scaleform.Draw2D();
                }
            }
            await Task.FromResult(0);
        }
    }
}
