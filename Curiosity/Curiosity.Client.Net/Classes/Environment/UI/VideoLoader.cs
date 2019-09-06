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

        static string url = "http://lifev.net";

        static bool IsVideoLoaded = false;
        static Vector3 spawnedVideo = new Vector3(-1373.8600f, -476.79450f, 72.13f);
        static DuiContainer container;
        static float distance = 15.0f;

        public static void Init()
        {
            client.RegisterTickHandler(ShowVideo);
            client.RegisterEventHandler("onClientResourceStop", new Action<string>(OnClientResourceStop));
            client.RegisterEventHandler("curiosity:Client:Video:SetUrl", new Action<string>(SetUrl));
        }

        static async void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;
            await DuiHandler.DestroyAllDui();
        }

        static async void SetUrl(string urlFromServer)
        {
            if (IsVideoLoaded)
            {
                if (url != urlFromServer)
                {
                    url = urlFromServer;
                    Debug.WriteLine($"New URL: {url}");
                    await Client.Delay(100);
                    await DuiHandler.DestroyAllDui();
                    await Client.Delay(1000);
                    container = await DuiHandler.AddDuiAtPosition("ex_tvscreen", "ex_prop_ex_tv_flat_01", url, spawnedVideo, 97.99994f);
                }
            }
        }

        static async Task ShowVideo()
        {
            try
            {
                if (!IsVideoLoaded)
                {
                    if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, spawnedVideo, true) < distance)
                    {
                        await Client.Delay(2000);
                        container = await DuiHandler.AddDuiAtPosition("ex_tvscreen", "ex_prop_ex_tv_flat_01", url, spawnedVideo, 97.99994f);
                        IsVideoLoaded = true;
                        if (url != "http://lifev.net")
                            Notifications.LifeV(1, "Lounge", "Welcome to the Lounge", "Type /watch to watch the movie", 2);
                    }
                }

                if (IsVideoLoaded)
                {
                    if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, spawnedVideo, true) > distance)
                    {
                        await DuiHandler.DestroyAllDui();
                        IsVideoLoaded = false;
                        url = "http://lifev.net";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            await Task.FromResult(0);
        }
    }
}
