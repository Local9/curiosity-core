using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.Net
{
    public class CuriosityPlayer : BaseScript
    {
        long userId = 0;
        bool isLoading = false;

        public CuriosityPlayer()
        {
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

            EventHandlers["curiosity:Client:Player:Setup"] += new Action<long, float, float, float>(OnPlayerSetup);

            Tick += UpdatePlayerLocation;
        }

        void ToggleSound(bool state)
        {
            if (state)
            {
                API.StartAudioScene("MP_LEADERBOARD_SCENE");
            }
            else
            {
                API.StopAudioScene("MP_LEADERBOARD_SCENE");
            }
        }

        void Setup()
        {
            API.SetManualShutdownLoadingScreenNui(true);
            ToggleSound(true);

            if (!API.IsPlayerSwitchInProgress())
                API.SwitchOutPlayer(Game.PlayerPed.Handle, 0, 1);
        }

        void ClearScreen()
        {
            API.SetCloudHatOpacity(0.1f);
            API.HideHudAndRadarThisFrame();
            API.SetDrawOrigin(0f, 0f, 0f, 0);
        }

        async void OnResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            Setup();
            await Delay(0);
            TriggerServerEvent("curiosity:Server:Player:Setup");
            await Delay(0);
        }

        async void OnResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            await Delay(0);
            Vector3 playerPosition = Game.PlayerPed.Position;
            TriggerServerEvent("curiosity:Server:Player:SaveLocation", playerPosition.X, playerPosition.Y, playerPosition.Z);
        }

        async void OnPlayerSetup(long userId, float x, float y, float z)
        {
            Setup();
            await Delay(0);

            if (isLoading)
                return;

            isLoading = !isLoading;

            Screen.LoadingPrompt.Show("Loading Player");

            while (API.GetPlayerSwitchState() != 5)
            {
                await Delay(0);
                ClearScreen();
            }

            API.ShutdownLoadingScreen();
            ClearScreen();
            await Delay(0);
            Screen.Fading.FadeOut(1);

            API.ShutdownLoadingScreenNui();

            ClearScreen();
            await Delay(0);
            ClearScreen();

            Screen.Fading.FadeIn(500);

            while(!Screen.Fading.IsFadedIn)
            {
                await Delay(0);
                ClearScreen();
            }

            int gameTimer = API.GetGameTimer();

            ToggleSound(false);

            this.userId = userId;

            await Delay(0);

            float left = (Screen.Width / 2) / 3f;

            Text text = new Text($"NAME: {Game.Player.Name}\nPLAYERID: {userId}", new System.Drawing.PointF { X = left, Y = Screen.Height - 37 }, 0.3f, System.Drawing.Color.FromArgb(75, 255, 255, 255), Font.ChaletComprimeCologne, Alignment.Left, false, true);
            text.WrapWidth = 300;

            await Delay(1000);

            Game.PlayerPed.Position = new Vector3(x, y, z);

            await Delay(0);

            while (true)
            {
                ClearScreen();
                await Delay(0);
                if (API.GetGameTimer() - gameTimer > 5000)
                {
                    API.SwitchInPlayer(Game.PlayerPed.Handle);
                    ClearScreen();
                    while (API.GetPlayerSwitchState() != 12)
                    {
                        await Delay(0);
                        ClearScreen();
                    }
                    break;
                }
            }

            API.ClearDrawOrigin();

            if (Screen.LoadingPrompt.IsActive)
            {
                Screen.LoadingPrompt.Hide();
            }

            while (true)
            {
                text.Draw();
                await Delay(0);
            }
        }

        async Task UpdatePlayerLocation()
        {
            await Delay(10000);
            while (true)
            {
                Vector3 playerPosition = Game.PlayerPed.Position;
                TriggerServerEvent("curiosity:Server:Player:SaveLocation", playerPosition.X, playerPosition.Y, playerPosition.Z);
                await Delay(1000 * 30);
            }
        }
    }
}
