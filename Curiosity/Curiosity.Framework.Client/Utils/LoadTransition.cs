using CitizenFX.Core.UI;

namespace Curiosity.Framework.Client.Utils
{
    internal class LoadTransition
    {
        static PluginManager Instance => PluginManager.Instance;

        private static async Task OnCloudHatAsync()
        {
            SetCloudHatOpacity(0.05f);
            HideHudAndRadarThisFrame();
        }

        public static async Task OnUpAsync()
        {
            ScreenInterface.StartLoadingMessage("PM_WAIT");
            var timestamp = GetGameTimer();

            Instance.AttachTickHandler(OnCloudHatAsync);

            Instance.SoundEngine.Disable();

            SwitchOutPlayer(Game.PlayerPed.Handle, 0, 1);

            while (GetPlayerSwitchState() != 5 && timestamp + 15000 > GetGameTimer())
            {
                await BaseScript.Delay(0);
            }

            Instance.SoundEngine.Enable();

            Instance.DetachTickHandler(OnCloudHatAsync);
        }

        public static async Task OnWaitAsync()
        {
            while (GetPlayerSwitchState() < 3)
            {
                await BaseScript.Delay(100);
            }
        }

        public static async Task OnDownAsync()
        {
            while (GetPlayerSwitchState() != 5)
            {
                await BaseScript.Delay(100);
            }

            GameInterface.Hud.FadeIn(1000);

            Instance.AttachTickHandler(OnCloudHatAsync);

            var timestamp = GetGameTimer();

            while (timestamp + 3000 > GetGameTimer())
            {
                await BaseScript.Delay(10);
            }

            SwitchInPlayer(Game.PlayerPed.Handle);

            while (GetPlayerSwitchState() != 12)
            {
                await BaseScript.Delay(10);
            }

            Instance.DetachTickHandler(OnCloudHatAsync);

            ClearDrawOrigin();

            Screen.LoadingPrompt.Hide();
            ScreenInterface.EnableHud();
        }

        //public async Task DownWait()
        //{
        //    if (Cache.Character.MarkedAsRegistered)
        //    {
        //        while (API.GetPlayerSwitchState() < 10)
        //        {
        //            await BaseScript.Delay(10);
        //        }
        //    }
        //    else
        //    {
        //        while (Cache.Character.MarkedAsRegistered && !API.IsScreenFadedOut())
        //        {
        //            await BaseScript.Delay(10);
        //        }
        //    }
        //}
    }
}
