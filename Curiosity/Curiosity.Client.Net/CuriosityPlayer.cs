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

        bool showScaleform = true;

        Scaleform scaleform;

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

            API.RegisterCommand("rules", new Action(ShowScaleformRules), false);
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
            Screen.Effects.Stop(ScreenEffect.DeathFailOut);
            Screen.Fading.FadeOut(500);
            Screen.LoadingPrompt.Show("Loading...");
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

            API.SetEntityCoordsNoOffset(Game.PlayerPed.Handle, 402.668f, -1003.000f, -98.004f, false, false, true);

            API.NetworkResurrectLocalPlayer(402.668f, -1003.000f, -98.004f, 0.0f, true, false);

            API.ClearPedTasksImmediately(Game.PlayerPed.Handle);
            API.RemoveAllPedWeapons(Game.PlayerPed.Handle, false);
            API.ClearPlayerWantedLevel(Game.Player.Handle);
            Vector3 spawnPosition = new Vector3(x, y, z);

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

            Vector3 groundSpawn = await spawnPosition.Ground();
            Vector3 safeSpawn = groundSpawn;

            if (API.GetSafeCoordForPed(groundSpawn.X, groundSpawn.Y, groundSpawn.Z, true, ref safeSpawn, 16))
            {
                groundSpawn = await safeSpawn.Ground();
            }

            Game.PlayerPed.Position = groundSpawn;


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

            while (!API.HasCollisionLoadedAroundEntity(Game.PlayerPed.Handle))
            {
                await Delay(0);
            }

            ShowScaleformRules();

            Game.PlayerPed.IsPositionFrozen = false;
            Game.PlayerPed.IsInvincible = false;
            Game.PlayerPed.IsCollisionEnabled = true;
            Game.PlayerPed.IsVisible = true;

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
            spawnInfo.z = Game.PlayerPed.Position.Z;
            spawnInfo.y = Game.PlayerPed.Position.Y;
            spawnInfo.x = Game.PlayerPed.Position.Z;
            spawnInfo.heading = Game.PlayerPed.Heading;
            spawnInfo.idx = 0;
            spawnInfo.model = Game.PlayerPed.Model.Hash;

            dynamic spawnInfodyn = spawnInfo;

            await Delay(500);

            try
            {
                int deathCheck = API.GetResourceKvpInt("DEATH");

                if (deathCheck > 0)
                {
                    API.SetResourceKvpInt("DEATH", 0);
                    Client.TriggerServerEvent("curiosity:Server:Bank:MedicalFees", true);
                    Classes.Environment.UI.Notifications.LifeV(1, "EMS", "Medical Fees", "Charged for trying to dodge medical fees.", 132);
                }

                Screen.Effects.Stop(ScreenEffect.DeathFailOut);
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
            Vector3 playerPosition = await Game.PlayerPed.Position.Ground();
            TriggerServerEvent("curiosity:Server:Player:SaveLocation", playerPosition.X, playerPosition.Y, posZ);
        }

        async void ShowScaleformRules()
        {
            ScaleformTask();
            await Client.Delay(10000);
            scaleform.Dispose();
        }

        async void LoadDict(string dict)
        {
            API.RequestStreamedTextureDict(dict, false);
            while (!API.HasStreamedTextureDictLoaded(dict))
            {
                await Client.Delay(0);
            }
        }

        async void ScaleformTask()
        {
            scaleform = new Scaleform("GTAV_ONLINE");

            while (!scaleform.IsLoaded)
            {
                await Client.Delay(0);
            }

            string description = "~r~We do not tolerate any form of racism.~s~~n~";
            description += "~r~DO NOT~s~ Try to kill or harass other players.~n~";
            description += "~r~DO NOT~s~ Spam the chat.~n~";
            description += "~r~DO NOT~s~ Force people to RP, it is voluntary.~n~";
            description += "~r~DO NOT~s~ Abuse exploits, report them on the forums.~n~";
            description += "~r~DO NOT~s~ Drive recklessly.~n~";
            description += "~g~RESPECT ALL~s~ Players and Staff members.~n~";
            description += "~g~USE ONLY~s~ English in the chat.~n~";
            description += "~g~PvE ONLY~s~ Do jobs and work together.~n~";
            description += "Finally, keep it friendly and we'll all get along.~n~";
            description += "~o~Press ~b~M~o~ to get started!~w~~n~";
            description += "~b~Forums~s~: forums.lifev.net / ~b~Discord~s~: discord.lifev.net";

            scaleform.CallFunction("SETUP_TABS", 1, false);
            const string dictTexture = "www_arenawar_tv";
            LoadDict(dictTexture);
            scaleform.CallFunction("SETUP_TABS", true);
            scaleform.CallFunction("SET_BIGFEED_INFO", "Hello", description, 0, dictTexture, "bg_top_left", $"~y~SERVER RULES", "deprecated", $"Welcome To ~y~Life V~s~!", 0);
            scaleform.CallFunction("SET_NEWS_CONTEXT", 0);

            while (scaleform.IsLoaded)
            {
                await Client.Delay(0);
                scaleform.Render2D();
            }

            API.SetStreamedTextureDictAsNoLongerNeeded(dictTexture);
        }
    }
}
