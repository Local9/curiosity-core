using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    static class Cash
    {
        static int cash = 1000;
        static int bank = 2000;

        static bool loaded = false;
        const int HUD_SCALEFORM_ID = 4;

        static public void Init()
        {
            BaseScript.Delay(10000);
            TickCash();
        }

        static async void TickCash()
        {
            if (ControlHelper.IsControlJustPressed(Control.MultiplayerInfo))
            {
                if (loaded)
                {
                    return;
                }
                loaded = true;
                cash++;

                if (!HasHudScaleformLoaded(HUD_SCALEFORM_ID))
                {
                    RequestHudScaleform(HUD_SCALEFORM_ID);
                    while (!HasHudScaleformLoaded(HUD_SCALEFORM_ID))
                    {
                        await BaseScript.Delay(1);
                    }
                }

                BeginScaleformMovieMethodHudComponent(HUD_SCALEFORM_ID, "SET_PLAYER_MP_CASH");
                PushScaleformMovieFunctionParameterInt(cash);
                PushScaleformMovieFunctionParameterInt(bank);
                EndScaleformMovieMethodReturn();
            }
        }

        async static void DisposeOfScaleForm(Scaleform scaleform, int duration)
        {
            await BaseScript.Delay(duration);
            scaleform.Dispose();
            loaded = false;
        }
    }
}
