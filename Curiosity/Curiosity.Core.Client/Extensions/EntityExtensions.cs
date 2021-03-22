using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Extensions
{
    public static class EntityExtensions
    {
        public async static Task FadeOut(this Ped ped, bool slow = true)
        {
            await Fade(ped, false, slow);
        }

        public async static Task FadeIn(this Ped ped, bool slow = true)
        {
            await Fade(ped, true, slow);
        }

        public async static Task Fade(this Ped ped, bool fadeIn, bool slow = true)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(ped.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(ped.Handle, false, slow);
            }

            while (API.NetworkIsEntityFading(ped.Handle))
            {
                await BaseScript.Delay(10);
            }

            Cache.UpdatePedId();
        }

        public async static Task FadeOut(this Vehicle veh, bool slow = true)
        {
            await Fade(veh, false, slow);
        }

        public async static Task FadeIn(this Vehicle veh, bool slow = true)
        {
            await Fade(veh, true, slow);
        }

        public async static Task Fade(this Vehicle veh, bool fadeIn, bool fadeOutNormal = false, bool slow = true)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(veh.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(veh.Handle, fadeOutNormal, slow);
            }

            while (API.NetworkIsEntityFading(veh.Handle))
            {
                await BaseScript.Delay(10);
            }
        }

        public async static Task FadeOut(this Prop prop, bool slow = true)
        {
            await Fade(prop, false, slow);
        }

        public async static Task FadeIn(this Prop prop, bool slow = true)
        {
            await Fade(prop, true, slow);
        }

        public async static Task Fade(this Prop prop, bool fadeIn, bool fadeOutNormal = false, bool slow = true)
        {
            if (fadeIn)
            {
                API.NetworkFadeInEntity(prop.Handle, slow);
            }
            else
            {
                API.NetworkFadeOutEntity(prop.Handle, fadeOutNormal, slow);
            }

            while (API.NetworkIsEntityFading(prop.Handle))
            {
                await BaseScript.Delay(10);
            }
        }
    }
}
