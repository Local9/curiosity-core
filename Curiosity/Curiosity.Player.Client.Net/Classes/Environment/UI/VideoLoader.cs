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

        static string url = "https://player.twitch.tv/?volume=0.50&channel=twitch";
        static string offlineUrl = "about:blank";

        static bool IsVideoLoaded = false;
        static Vector3 spawnedVideo;

        public static void Init()
        {
            client.RegisterTickHandler(ShowVideo);
        }

        static async Task ShowVideo()
        {
            if (IsVideoLoaded)
            {
                if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, spawnedVideo, true) < 10)
                {
                    
                }
            }

            if (!IsVideoLoaded)
            {
                // DuiHandler.CreateModelForRenderInPosition("", "ex_prop_ex_tv_flat_01", , 0.0f);
            }
        }
    }
}
