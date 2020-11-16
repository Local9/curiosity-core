using CitizenFX.Core;
using CitizenFX.Core.Native;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Extensions
{
    public static class PedExtensions
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

        public static bool IsPlayingAnim(this Ped ped, string animSet, string animName)
        {
            return API.IsEntityPlayingAnim(ped.Handle, animSet, animName, 3);
        }

        public static async void AnimationSearch(this Ped ped)
        {
            string scenario = "PROP_HUMAN_BUM_BIN";
            if (!ped.IsInVehicle())
            {
                API.TaskStartScenarioInPlace(ped.Handle, scenario, 0, true);
                await BaseScript.Delay(5000);
                ped.Task.ClearAll();
            }
        }
        public static async void AnimationClipboard(this Ped ped)
        {
            string scenario = "WORLD_HUMAN_CLIPBOARD";
            if (!ped.IsInVehicle())
            {
                API.TaskStartScenarioInPlace(ped.Handle, scenario, 0, true);
                await BaseScript.Delay(5000);
                ped.Task.ClearAll();
            }
        }

        public static async void AnimationRadio(this Ped ped)
        {
            LoadAnimation("random@arrests");
            ped.Task.PlayAnimation("random@arrests", "generic_radio_enter", 1.5f, 2.0f, -1, (AnimationFlags)50, 2.0f);
            await BaseScript.Delay(6000);
            ped.Task.ClearAll();
        }

        public static async Task<bool> LoadAnimation(string dict)
        {
            while (!API.HasAnimDictLoaded(dict))
            {
                await BaseScript.Delay(100);
                API.RequestAnimDict(dict);
            }
            return true;
        }
    }
}
