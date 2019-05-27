using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Client.net.Helpers;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    static class VideoLoader
    {
        static internal Client client = Client.GetInstance();

        static string url = "https://www.youtube.com/embed/hsaLDnl_fEs?controls=0&autoplay=1&showinfo=0&rel=0";

        static bool IsVideoLoaded = false;

        public static void Init()
        {
            client.RegisterTickHandler(ShowVideo);
        }

        static async Task ShowVideo()
        {
            if (ControlHelper.IsControlJustPressed(Control.SelectCharacterMichael, true))
            {
                if (!IsVideoLoaded)
                {
                    await DuiHandler.CreateRandomUniqueDuiContainer(url);
                }
                else
                {
                    await DuiHandler.DestroyAllDui();
                }
                IsVideoLoaded = !IsVideoLoaded;
            }
        }
    }
}
