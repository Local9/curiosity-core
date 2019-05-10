using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.Net
{
    public class CuriosityTutorial : BaseScript
    {
        string url = "https://www.youtube.com";
        float scale = 0.1f;
        string sfName = "generic_texture_renderer";

        int width = 1280;
        int height = 720;

        int sfHandle = 0;
        bool txdHasBeenSet = false;
        long duiObj = 0;


        public CuriosityTutorial()
        {
            //EventHandlers["onClientResourceStart"] += new Action<string>(OnClientResourceStart);
            //EventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);
            //Tick += ShowVideo;
        }

        async void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            while (!API.HasScaleformMovieLoaded(sfHandle))
            {
                sfHandle = API.RequestScaleformMovie(sfName);
                await Delay(0);
                if (sfHandle == 0)
                {
                    Screen.ShowNotification("Video Fail to Load");
                }
            }

            long txd = API.CreateRuntimeTxd("meows");
            await Delay(0);
            duiObj = API.CreateDui(url, width, height);
            await Delay(0);
            string dui = API.GetDuiHandle(duiObj);
            await Delay(0);
            Function.Call(Hash.CREATE_RUNTIME_TEXTURE_FROM_DUI_HANDLE, txd, "woof", dui);
            await Delay(0);
        }

        async void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            if (duiObj > 0)
            {
                API.DestroyDui(duiObj);
                await Delay(0);
                API.SetScaleformMovieAsNoLongerNeeded(ref sfHandle);
                await Delay(0);
                txdHasBeenSet = false;
            }

            await Delay(0);
        }

        async Task ShowVideo()
        {
            try
            {
                if (sfHandle > 0 && !txdHasBeenSet)
                {
                    API.BeginScaleformMovieMethod(sfHandle, "SET_TEXTURE");
                    API.PushScaleformMovieMethodParameterString("meows");
                    API.PushScaleformMovieMethodParameterString("woof");
                    API.ScaleformMovieMethodAddParamInt(0);
                    API.ScaleformMovieMethodAddParamInt(0);
                    API.ScaleformMovieMethodAddParamInt(width);
                    API.ScaleformMovieMethodAddParamInt(height);
                    API.EndScaleformMovieMethod();
                    txdHasBeenSet = true;
                }

                Vector3 vector3 = Game.PlayerPed.Position;

                if (sfHandle > 0 && API.HasScaleformMovieLoaded(sfHandle))
                {
                    await Delay(0);

                    API.DrawScaleformMovie_3dNonAdditive(
                            sfHandle,
                            vector3.X - 1, vector3.Y, vector3.Z + 2,
                            0, 0, 0,
                            2, 2, 2,
                            scale * 1, scale * (9 / 16), 1
                            , 2);
                }

                await Delay(0);
            }
            catch (Exception ex)
            {
                Screen.ShowNotification($"{ex.Message}");
            }
        }
    }
}
