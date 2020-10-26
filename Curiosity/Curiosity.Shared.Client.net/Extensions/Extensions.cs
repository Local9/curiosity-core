using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class Extensions
    {
        public async static Task FadeOut(this Ped ped, bool slow = false)
        {
            await Fade(ped, false, slow);
        }

        public async static Task FadeIn(this Ped ped, bool slow = false)
        {
            await Fade(ped, true, slow);
        }

        public async static Task Fade(this Ped ped, bool fadeIn, bool slow = false)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(ped.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(ped.Handle, false, slow);
            }

            await BaseScript.Delay(3000);
        }
    }
}
