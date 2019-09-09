using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.net.Entity;
using Curiosity.Shared.Client.net;
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
            Tick += SpawnTick;
        }

        async void DisplayInfo(bool display)
        {
            displayInfo = display;
            await Delay(0);
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
            if (role != roleName)
            {
                Classes.Environment.UI.Notifications.LifeV(1, $"Information", "Privilege Update", $"You have been granted the role of ~y~{role}.", 2);
                roleName = role;
            }

            //if (screenWidth != Screen.Resolution.Width)
            //{
            //    float left = (Screen.Width / 2) / 3f;

            //    if (Screen.Resolution.Width > 1980)
            //    {
            //        left = 1f;
            //    }

            //    text.Position = new System.Drawing.PointF { X = left, Y = Screen.Height - 50 };
            //    screenWidth = Screen.Resolution.Width;
            //}

            //text.Caption = $"ROLE: {role}\nNAME: {Game.Player.Name}\nLIFEVID: {userId}";

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

            API.ClearBrief();
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
            Setup();

            PedHash myPedModelToLoad = PedHash.FreemodeMale01;

            if (user.Skin.Model == "mp_f_freemode_01")
            {
                myPedModelToLoad = PedHash.FreemodeFemale01;
            }

            Model defaultModel = myPedModelToLoad;
            await defaultModel.Request(10000);

            while (!defaultModel.IsLoaded)
            {
                defaultModel.Request();
                await Client.Delay(0);
            }

            await Game.Player.ChangeModel(defaultModel);
            Game.PlayerPed.IsInvincible = true;
            Game.PlayerPed.IsPositionFrozen = true;
            Game.PlayerPed.Position = new Vector3(402.668f, -1003.000f, -98.004f);

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
                await Delay(0);
                API.SetPedComponentVariation(playerPed, comp.Key, comp.Value.Item1, comp.Value.Item2, 0);
            }

            foreach(KeyValuePair<int, int> over in user.Skin.PedHeadOverlay)
            {
                await Delay(0);
                API.SetPedHeadOverlay(playerPed, over.Key, over.Value, over.Value == 0 ? 0f : 1.0f);
            }

            foreach (KeyValuePair<int, Tuple<int, int>> over in user.Skin.PedHeadOverlayColor)
            {
                await Delay(0);
                API.SetPedHeadOverlayColor(playerPed, over.Key, over.Value.Item1, over.Value.Item2, 0);
            }

            foreach (KeyValuePair<int, Tuple<int, int>> over in user.Skin.Props)
            {
                await Delay(0);
                API.SetPedPropIndex(playerPed, over.Key, over.Value.Item1, over.Value.Item2, false);
            }

            defaultModel.MarkAsNoLongerNeeded();

            // API.RequestCollisionAtCoord(x, y, z);

            //while(!API.HasCollisionLoadedAroundEntity(playerPed))
            //{
            //    Debug.WriteLine("Still requesting collision");
            //    await Delay(0);
            //}

            API.NetworkResurrectLocalPlayer(402.668f, -1003.000f, -98.004f, 0.0f, true, false);

            float groundZ = z;
            Vector3 spawnPosition = new Vector3(x, y, groundZ);

            Game.PlayerPed.DropsWeaponsOnDeath = false;
            
            API.GetGroundZFor_3dCoord(x, y, z, ref groundZ, false);
            Vector3 safeCoord = new Vector3(x, y, groundZ);
            if (API.GetSafeCoordForPed(x, y, groundZ, true, ref safeCoord, 16))
            {
                spawnPosition = safeCoord;
            }

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

            if (spawnPosition.IsZero)
            {
                spawnPosition = World.GetNextPositionOnSidewalk(new Vector3(x, y, groundZ));
            }

            Game.PlayerPed.Position = spawnPosition + new Vector3(0f, 0f, -1f);

            int gameTimer = API.GetGameTimer();

            ToggleSound(false);

            this.userId = userId;

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

            Game.PlayerPed.IsPositionFrozen = false;
            Game.PlayerPed.IsInvincible = false;

            await Delay(0);

            API.SetNuiFocus(false, false);
            API.ClearDrawOrigin();

            await Delay(0);

            if (Screen.LoadingPrompt.IsActive)
            {
                Screen.LoadingPrompt.Hide();
            }

            canSaveLocation = true;
            isPlayerSpawned = true;

            Classes.Environment.UI.Notifications.LifeV(1, $"Welcome...", $"~y~{Game.Player.Name}~s~!", $"~b~Life V ID: ~y~{userId}~n~~b~Role: ~y~{role}", 2);

            Curiosity.Shared.Client.net.GameData.SpawnInfo spawnInfo = new Shared.Client.net.GameData.SpawnInfo();
            spawnInfo.z = groundZ;
            spawnInfo.y = y;
            spawnInfo.x = x;
            spawnInfo.heading = Game.PlayerPed.Heading;
            spawnInfo.idx = 0;
            spawnInfo.model = Game.PlayerPed.Model.Hash;

            dynamic spawnInfodyn = spawnInfo;

            await Delay(500);

            try
            {
                TriggerEvent("playerSpawned", spawnInfodyn);
            }
            catch (Exception ex)
            {
                // DO NOTHING
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
