using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    static class VideoLoader
    {

        static internal Client client = Client.GetInstance();

        static string url = "https://www.youtube.com";
        static float scale = 0.1f;
        static string sfName = "generic_texture_renderer";

        static int width = 1280;
        static int height = 720;

        //static int sfHandle = 0;
        static bool txdHasBeenSet = false;
        static long duiObj = 0;


        static Scaleform scaleform = new Scaleform(sfName);

        public static void Init()
        {
            client.RegisterEventHandler("onClientResourceStart", new Action<string>(OnClientResourceStart));
            // client.RegisterEventHandler("onClientResourceStop", new Action<string>(OnClientResourceStop));
            client.RegisterTickHandler(ShowVideo);
        }

        static async void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            while (!scaleform.IsLoaded)
            {
                await Client.Delay(0);
                scaleform = new Scaleform(sfName);
            }
            await Client.Delay(0);
        }

        static async void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            if (duiObj > 0)
            {
                Function.Call(Hash.DESTROY_DUI, duiObj);
                Function.Call(Hash.SET_SCALEFORM_MOVIE_AS_NO_LONGER_NEEDED, scaleform.Handle);
                txdHasBeenSet = false;
            }
            await Client.Delay(0);
        }

        static async Task ShowVideo()
        {
            CitizenFX.Core.UI.Screen.ShowNotification($"{scaleform.IsLoaded}");

            if (scaleform.IsLoaded)
            {
                Vector3 vector3 = Game.PlayerPed.Position;

                API.DrawScaleformMovie_3dNonAdditive(scaleform.Handle,
                        vector3.X - 1, vector3.Y, vector3.Z + 2,
                        0, 0, 0,
                        2, 2, 2,
                        scale * 1, scale * (9 / 16), 1
                        , 2);
            }

            await Task.FromResult(0);

        //    if (scaleform.Handle > 0 && !txdHasBeenSet)
        //    {
        //        Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION, scaleform.Handle, "SET_TEXTURE");

        //        Function.Call(Hash._PUSH_SCALEFORM_MOVIE_METHOD_PARAMETER_STRING, "meows");
        //        Function.Call(Hash._PUSH_SCALEFORM_MOVIE_METHOD_PARAMETER_STRING, "woof");

        //        Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 0);
        //        Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, 0);
        //        Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, width);
        //        Function.Call(Hash._PUSH_SCALEFORM_MOVIE_FUNCTION_PARAMETER_INT, height);

        //        Function.Call(Hash._POP_SCALEFORM_MOVIE_FUNCTION_VOID);

        //        txdHasBeenSet = true;
        //    }

        //    Vector3 vector3 = Game.PlayerPed.Position;

        //    if (scaleform.Handle > 0 && scaleform.IsLoaded)
        //    {
        //        API.DrawScaleformMovie_3dNonAdditive(scaleform.Handle,
        //                vector3.X - 1, vector3.Y, vector3.Z + 2,
        //                0, 0, 0,
        //                2, 2, 2,
        //                scale * 1, scale * (9 / 16), 1
        //                , 2);
        //    }

        //    await Task.FromResult(0);
        }
    }
}
