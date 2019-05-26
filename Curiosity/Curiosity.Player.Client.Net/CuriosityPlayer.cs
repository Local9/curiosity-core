using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net
{
    public class CuriosityPlayer : BaseScript
    {
        long userId = 0;
        bool isLoading = false;
        bool displayInfo = true;
        Text text;

        public CuriosityPlayer()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

            EventHandlers["curiosity:Client:Player:Setup"] += new Action<long, int, string, float, float, float>(OnPlayerSetup);
            EventHandlers["curiosity:Client:Player:Role"] += new Action<string>(UpdatePlayerRole);
            EventHandlers["curiosity:Client:Player:DisplayInfo"] += new Action<bool>(DisplayInfo);

            Tick += UpdatePlayerLocation;
            Tick += PlayerAndServerSettings;
            Tick += DisplayInformation;
        }

        async void DisplayInfo(bool display)
        {
            displayInfo = display;
            await Delay(0);
        }

        async Task DisplayInformation()
        {
            while (true)
            {
                if (text != null)
                    text.Enabled = displayInfo;

                await Delay(0);
            }
        }

        async Task PlayerAndServerSettings()
        {
            while (true)
            {
                await Delay(30000);
                TriggerServerEvent("curiosity:Server:Player:GetRole");
            }
        }

        async void UpdatePlayerRole(string role)
        {
            if (text == null) return;

            text.Caption = $"ROLE: {role}\nNAME: {Game.Player.Name}\nPLAYERID: {userId}";
            await Delay(0);
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
            SaveLocation();
        }

        async void OnPlayerSetup(long userId, int roleId, string role, float x, float y, float z)
        {
            Screen.LoadingPrompt.Show("Loading Player");

            Setup();
            await Delay(0);

            if (isLoading)
                return;

            isLoading = !isLoading;

            await Delay(0);

            await Delay(0);

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

            if (Screen.Resolution.Width > 1980)
            {
                left = 1f;
            }

            text = new Text($"ROLE: {role}\nNAME: {Game.Player.Name}\nPLAYERID: {userId}", new System.Drawing.PointF { X = left, Y = Screen.Height - 50 }, 0.3f, System.Drawing.Color.FromArgb(75, 255, 255, 255), Font.ChaletComprimeCologne, Alignment.Left, false, true);
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

            await Delay(0);

            API.SetNuiFocus(false, false);
            API.ClearDrawOrigin();

            await Delay(0);

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
                SaveLocation();
                await Delay(1000 * 30);
            }
        }

        void SaveLocation()
        {
            Vector3 playerPosition = Game.PlayerPed.Position;
            TriggerServerEvent("curiosity:Server:Player:SaveLocation", playerPosition.X, playerPosition.Y, playerPosition.Z);
        }
    }
}
