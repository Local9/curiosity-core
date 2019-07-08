using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net.Extensions;

namespace Curiosity.Client.net
{
    public class CuriosityPlayer : BaseScript
    {
        GlobalEntity.User user;

        long userId = 0;
        int roleId;
        string roleName;
        float posX;
        float posY;
        float posZ;

        bool isLoading = false;
        bool displayInfo = true;
        Text text;
        int screenWidth;
        bool hasSpawned = false;
        bool serverReady = false;
        bool canSaveLocation = false;
        public bool isPlayerSpawned = false;

        Random rnd = new Random();

        public CuriosityPlayer()
        {
            EventHandlers["onClientResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["onClientResourceStop"] += new Action<string>(OnClientResourceStop);

            EventHandlers["curiosity:Client:Player:Setup"] += new Action<string>(OnPlayerSetup);
            EventHandlers["curiosity:Client:Player:Role"] += new Action<string>(UpdatePlayerRole);
            EventHandlers["curiosity:Client:Player:DisplayInfo"] += new Action<bool>(DisplayInfo);

            Tick += UpdatePlayerLocation;
            Tick += PlayerAndServerSettings;
            Tick += DisplayInformation;
            Tick += SpawnTick;
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

        async Task DisplayLoading()
        {
            while (Screen.LoadingPrompt.IsActive)
            {
                Screen.LoadingPrompt.Show("Loading Player");
                await Delay(0);
            }
        }

        async Task PlayerAndServerSettings()
        {
            while(!Client.isSessionActive)
            {
                await Delay(10);
            }

            while (true)
            {
                await Delay(30000);
                TriggerServerEvent("curiosity:Server:Player:GetRole");
            }
        }

        async void UpdatePlayerRole(string role)
        {
            if (text == null) return;

            if (screenWidth != Screen.Resolution.Width)
            {
                float left = (Screen.Width / 2) / 3f;

                if (Screen.Resolution.Width > 1980)
                {
                    left = 1f;
                }

                text.Position = new System.Drawing.PointF { X = left, Y = Screen.Height - 50 };
                screenWidth = Screen.Resolution.Width;
            }

            text.Caption = $"ROLE: {role}\nNAME: {Game.Player.Name}\nLIFEVID: {userId}";

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

        async void OnClientResourceStop(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            await Delay(0);
            SaveLocation();
        }

        void OnPlayerSetup(string jsonUser)
        {
            API.SetNuiFocus(false, false);
            API.SetTransitionTimecycleModifier("DEFAULT", 5.0f);

            user = Newtonsoft.Json.JsonConvert.DeserializeObject<GlobalEntity.User>(jsonUser);

            if (user.Skin == null)
            {
                user.Skin = new GlobalEntity.PlayerCharacter();
            }

            Client.User = user;

            this.userId = user.UserId;
            this.roleId = user.RoleId;
            this.roleName = user.Role;
            this.posX = user.PosX;
            this.posY = user.PosY;
            this.posZ = user.PosZ;
            serverReady = true;
        }

        async Task SpawnTick()
        {
            bool playerPedExists = (Game.PlayerPed.Handle != 0);
            bool playerActive = API.NetworkIsPlayerActive(API.PlayerId());

            if (playerPedExists && playerActive && !hasSpawned && serverReady)
            {
                SpawnPlayer(userId, roleId, roleName, posX, posY, posZ);
                hasSpawned = true;
            }
            await Task.FromResult(0);
        }

        async void SpawnPlayer(long userId, int roleId, string role, float x, float y, float z)
        {

            Screen.Fading.FadeOut(500);

            Debug.WriteLine("OnPlayerSetup() -> STARTING");

            Setup();

            Model defaultModel = user.Skin.Model.ToEnum(PedHash.FreemodeMale01);

            await defaultModel.Request(10000);

            while (!defaultModel.IsLoaded)
            {
                defaultModel.Request();
                await Client.Delay(0);
            }

            await Game.Player.ChangeModel(defaultModel);
            Game.PlayerPed.IsInvincible = true;

            int playerPed = Game.PlayerPed.Handle;

            API.SetPedHeadBlendData(playerPed, user.Skin.FatherAppearance, user.Skin.MotherAppearance, 0, user.Skin.FatherSkin, user.Skin.MotherSkin, 0, user.Skin.FatherMotherAppearanceGene, user.Skin.FatherMotherSkinGene, 0, false);

            API.SetPedEyeColor(playerPed, user.Skin.EyeColor);

            API.SetPedHairColor(playerPed, user.Skin.HairColor, user.Skin.HairSecondaryColor);


            int randomNumber = rnd.Next(10);

            if (user.Skin.Components.Count == 0) // Set some random defaults
            {
                API.SetPedComponentVariation(playerPed, 0, 0, 0, 0); // Face
                API.SetPedComponentVariation(playerPed, 2, randomNumber, 0, 0); // Hair
                API.SetPedComponentVariation(playerPed, 5, 0, 0, 0); // Hands
                API.SetPedComponentVariation(playerPed, 6, randomNumber, 0, 0); // Shoes
            }

            foreach (KeyValuePair<int, Tuple<int, int>> comp in user.Skin.Components)
            {
                API.SetPedComponentVariation(playerPed, comp.Key, comp.Value.Item1, comp.Value.Item2, 0);
            }

            foreach(KeyValuePair<int, int> over in user.Skin.PedHeadOverlay)
            {
                API.SetPedHeadOverlay(playerPed, over.Key, over.Value, 1.0f);
            }

            foreach (KeyValuePair<int, Tuple<int, int>> over in user.Skin.PedHeadOverlayColor)
            {
                API.SetPedHeadOverlayColor(playerPed, over.Key, over.Value.Item1, over.Value.Item2, 0);
            }

            foreach (KeyValuePair<int, Tuple<int, int>> over in user.Skin.Props)
            {
                API.SetPedPropIndex(playerPed, over.Key, over.Value.Item1, over.Value.Item2, false);
            }

            defaultModel.MarkAsNoLongerNeeded();

            // API.RequestCollisionAtCoord(x, y, z);

            //while(!API.HasCollisionLoadedAroundEntity(playerPed))
            //{
            //    Debug.WriteLine("Still requesting collision");
            //    await Delay(0);
            //}

            float groundZ = z;
            API.GetGroundZFor_3dCoord(x, y, z, ref groundZ, false);

            API.SetEntityCoordsNoOffset(playerPed, x, y, groundZ, false, false, false);
            API.NetworkResurrectLocalPlayer(x, y, groundZ, 0.0f, true, false);

            Game.PlayerPed.Position = World.GetSafeCoordForPed(Game.PlayerPed.Position, true, 16);

            API.ShutdownLoadingScreen();
            API.ShutdownLoadingScreenNui();

            Screen.LoadingPrompt.Show("Loading Player");

            await Delay(0);

            if (isLoading)
                return;

            isLoading = !isLoading;

            await Delay(0);

            while (API.GetPlayerSwitchState() != 5)
            {
                await Delay(0);
                ClearScreen();
            }

            ClearScreen();
            await Delay(0);

            while(Screen.Fading.IsFadingOut)
            {
                await Delay(0);
            }

            ClearScreen();
            await Delay(0);

            Screen.Fading.FadeIn(500);

            while(!Screen.Fading.IsFadedIn)
            {
                await Delay(0);
                ClearScreen();
            }

            Game.PlayerPed.Position = new Vector3(x, y, groundZ);

            int gameTimer = API.GetGameTimer();

            ToggleSound(false);

            this.userId = userId;

            await Delay(0);

            float left = (Screen.Width / 2) / 3f;

            if (Screen.Resolution.Width > 1980)
            {
                left = 1f;
            }

            screenWidth = Screen.Resolution.Width;

            text = new Text($"ROLE: {role}\nNAME: {Game.Player.Name}\nLIFEVID: {userId}", new System.Drawing.PointF { X = left, Y = Screen.Height - 50 }, 0.3f, System.Drawing.Color.FromArgb(75, 255, 255, 255), Font.ChaletComprimeCologne, Alignment.Left, false, true);
            text.WrapWidth = 300;

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

            Game.PlayerPed.IsInvincible = false;
            Client.TriggerEvent("playerSpawned");
            canSaveLocation = true;
            isPlayerSpawned = true;

            if (user.Skin.Components.Count == 0)
            {
                Classes.Menus.PlayerCreator.PlayerCreatorMenu.StoreComponents(0, 0, 0);
                Classes.Menus.PlayerCreator.PlayerCreatorMenu.StoreComponents(2, randomNumber, 0);
                Classes.Menus.PlayerCreator.PlayerCreatorMenu.StoreComponents(5, randomNumber, 0);
                Classes.Menus.PlayerCreator.PlayerCreatorMenu.StoreComponents(6, randomNumber, 0);
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
                if (canSaveLocation)
                    SaveLocation();

                await Delay(1000 * 30);
            }
        }

        async void SaveLocation()
        {
            Vector3 playerPosition = Game.PlayerPed.Position;

            if (playerPosition.IsZero)
                return;

            float? posZ = playerPosition.Z;

            if (Game.PlayerPed.IsInAir)
            {
                Vector2 v2 = new Vector2(playerPosition.X, playerPosition.Y);

                float? gpz = await Helpers.WorldProbe.FindGroundZ(v2);

                if (gpz != null)
                    posZ = gpz;
            }

            TriggerServerEvent("curiosity:Server:Player:SaveLocation", playerPosition.X, playerPosition.Y, posZ);
        }
    }
}
