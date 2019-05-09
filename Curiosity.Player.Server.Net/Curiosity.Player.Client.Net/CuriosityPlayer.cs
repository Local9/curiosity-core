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
        Text text;

        public CuriosityPlayer()
        {
            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

            EventHandlers["curiosity:Client:Player:Setup"] += new Action<long, int, string, float, float, float>(OnPlayerSetup);
            EventHandlers["curiosity:Client:Player:Role"] += new Action<string>(UpdatePlayerRole);

            Tick += UpdatePlayerLocation;
            Tick += PlayerSettings;
            Tick += PlayerRole;
        }

        async Task PlayerSettings()
        {
            while (true)
            {
                API.ClearPlayerWantedLevel(Game.Player.Handle);
                API.SetMaxWantedLevel(0);
                API.SetPlayerWantedLevel(Game.Player.Handle, 0, false);
                API.SetPlayerWantedLevelNow(Game.Player.Handle, false);
                API.SetPlayerWantedLevelNoDrop(Game.Player.Handle, 0, false);
                await Delay(0);
            }
        }

        async Task PlayerRole()
        {
            while (true)
            {
                await Delay(10000);
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
