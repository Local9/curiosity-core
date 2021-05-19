using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Library.Models;
using Curiosity.Template.Client.Diagnostics;
using Curiosity.Template.Client.Environment;
using Curiosity.Template.Client.Environment.Entities.Models;
using Curiosity.Template.Client.Events;
using Curiosity.Template.Client.Interface.Menus.Creator;
using Curiosity.Template.Client.Interface.Menus.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Template.Client.Extensions
{
    public static class CharacterExtensions
    {

        public static Dictionary<int, KeyValuePair<string, string>> HairOverlays = new Dictionary<int, KeyValuePair<string, string>>()
            {
                { 0, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
                { 1, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 2, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 3, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_003_a") },
                { 4, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 5, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 6, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 7, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 8, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_008_a") },
                { 9, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 10, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 11, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 12, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 13, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 14, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
                { 15, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
                { 16, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 17, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
                { 18, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_000_a") },
                { 19, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_001_a") },
                { 20, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_000_a") },
                { 21, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_001_a") },
                { 22, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
            };
        public static Position DefaultPosition { get; } = new Position(-1042.24f, -2745.336f, 21.3594f, 332.7626f);
        public static Position TaxiPosition { get; } = new Position(-1050.467f, -2720.67f, 19.68902f, 240.8357f);

        public static RotatablePosition[] CameraViews { get; } =
        {
            new RotatablePosition(402.8294f, -1002.45f, -98.80403f, 357.6219f, -7f, 0f),
            new RotatablePosition(402.8294f, -998.8467f, -98.80403f, 357.1697f, -7f, 0f),
            new RotatablePosition(402.8294f, -997.967f, -98.35f, 357.1697f, -7f, 0f)
        };

        public static string[] HeadComponents { get; } =
        {
            "Face", "Wrinkles", "Freckles", "EyeColor", "Sunburn", "Complexion", "Hair", "Eyebrows", "Beard", "Blush",
            "Makeup", "Lipstick"
        };

        public static Position RegistrationPosition { get; } =
            new Position(405.9247f, -997.2114f, -99.00401f, 86.36787f);

        //public static void Synchronize(this CuriosityCharacter character)
        //{
        //    var player = Cache.Player;

        //    player.User.Characters.RemoveAll(self => self.CharacterId == character.CharacterId);
        //    player.User.Characters.Add(character);
        //}

        public static async Task Save(this CuriosityCharacter character)
        {
            character.LastPosition = Cache.Entity.Position;

            await EventSystem.GetModule().Request<object>("character:save", character);

            // Logger.Info($"[Characters] Saved `{character.CharacterId}` and it's changes.");
        }

        public static async Task Load(this CuriosityCharacter character)
        {
            var player = Cache.Player;

            API.SetPedDefaultComponentVariation(player.Entity.Id);

            Model playerModel = PedHash.FreemodeMale01;
            if (character.Gender == 1)
            {
                playerModel = PedHash.FreemodeFemale01;
            }
            await playerModel.Request(10000);
            await Game.Player.ChangeModel(playerModel);
            playerModel.MarkAsNoLongerNeeded();

            int fatherId = character.Heritage.FatherId;
            int motherId = character.Heritage.MotherId;
            float remBlend = character.Heritage.BlendApperance;
            float skinBlend = character.Heritage.BlendSkin;

            API.SetPedHeadBlendData(Cache.Entity.Id, fatherId, motherId, 0, fatherId, motherId, 0, remBlend, skinBlend, 0f, false);

            CharacterClothing.SetPedTop(Game.PlayerPed, character.Appearance.Top);
            CharacterClothing.SetPedPants(Game.PlayerPed, character.Appearance.Pants);
            CharacterClothing.SetPedShoes(Game.PlayerPed, character.Appearance.Shoes);
            CharacterClothing.SetPedHat(Game.PlayerPed, character.Appearance.Hat);
            CharacterClothing.SetPedGlasses(Game.PlayerPed, character.Appearance.Glasses);

            foreach (KeyValuePair<int, float> keyValuePair in character.Features)
            {
                API.SetPedFaceFeature(Cache.Entity.Id, keyValuePair.Key, keyValuePair.Value);
            }

            API.ClearPedFacialDecorations(Game.PlayerPed.Handle);

            if (character.Appearance.HairStyle == 0)
            {
                API.SetPedComponentVariation(Game.PlayerPed.Handle, 2, 0, 0, 0);
            }
            else
            {
                API.SetPedComponentVariation(Game.PlayerPed.Handle, 2, character.Appearance.HairStyle, 0, 0);
                if (!character.Appearance.HairOverlay.Equals(new KeyValuePair<string, string>()))
                {
                    KeyValuePair<string, string> overlay = character.Appearance.HairOverlay;
                    API.SetPedFacialDecoration(Game.PlayerPed.Handle, (uint)API.GetHashKey(overlay.Key), (uint)API.GetHashKey(overlay.Value));
                }

                API.SetPedHairColor(Game.PlayerPed.Handle, character.Appearance.HairPrimaryColor, character.Appearance.HairSecondaryColor);
            }

            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 1, character.Appearance.FacialHair, character.Appearance.FacialHairOpacity);
            API.SetPedHeadOverlayColor(Game.PlayerPed.Handle, 1, 1, character.Appearance.FacialHairColor, character.Appearance.FacialHairColor);
            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 2, character.Appearance.Eyebrow, character.Appearance.EyebrowOpacity);
            API.SetPedHeadOverlayColor(Game.PlayerPed.Handle, 2, 1, character.Appearance.EyebrowColor, character.Appearance.EyebrowColor);
            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 4, character.Appearance.EyeMakeup, character.Appearance.EyeMakeupOpacity);
            API.SetPedHeadOverlayColor(Game.PlayerPed.Handle, 4, 2, character.Appearance.EyeMakeupColor, character.Appearance.EyeMakeupColor);
            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 5, character.Appearance.Blusher, character.Appearance.BlusherOpacity);
            API.SetPedHeadOverlayColor(Game.PlayerPed.Handle, 5, 2, character.Appearance.BlusherColor, character.Appearance.BlusherColor);
            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 8, character.Appearance.Lipstick, character.Appearance.LipstickOpacity);
            API.SetPedHeadOverlayColor(Game.PlayerPed.Handle, 8, 2, character.Appearance.LipstickColor, character.Appearance.LipstickColor);

            API.SetPedEyeColor(Game.PlayerPed.Handle, character.Appearance.EyeColor);

            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 0, character.Appearance.SkinBlemish, character.Appearance.SkinBlemishOpacity);
            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 3, character.Appearance.SkinAging, character.Appearance.SkinAgingOpacity);
            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 6, character.Appearance.SkinComplexion, character.Appearance.SkinComplexionOpacity);
            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 7, character.Appearance.SkinDamage, character.Appearance.SkinDamageOpacity);
            API.SetPedHeadOverlay(Game.PlayerPed.Handle, 9, character.Appearance.SkinMoles, character.Appearance.SkinMolesOpacity);

            var voice = VoiceChat.GetModule();
            voice.Range = VoiceChatRange.Normal;
            voice.Commit();
        }

        public static void Revive(this CuriosityCharacter character, Position position)
        {
            var ped = Cache.Entity.Id;

            API.RemoveAllPedWeapons(ped, false);
            API.ClearPedTasksImmediately(ped);
            API.ClearPlayerWantedLevel(API.PlayerId());
            API.SetEntityVisible(ped, true, true);
            API.SetEntityHealth(ped, API.GetEntityMaxHealth(ped));
            API.NetworkResurrectLocalPlayer(position.X, position.Y, position.Z, position.H, false, false);
            API.ClearPedBloodDamage(ped);
        }

        public static async Task PostLoad(this CuriosityCharacter character)
        {
            CreatorMenus creatorMenu = new CreatorMenus();
            creatorMenu.CreateMenu();

            Screen.LoadingPrompt.Show("Loading Create Character...");

            CuriosityPlugin.Instance.AttachTickHandler(Intro);

            CuriosityPlugin.Instance.DiscordRichPresence.Status = "Creating Character";
            CuriosityPlugin.Instance.DiscordRichPresence.Commit();

            Session.CreatingCharacter = true;

            var player = Cache.Player;
            var timestamp = API.GetGameTimer();

            while (timestamp + 5000 > API.GetGameTimer())
            {
                await BaseScript.Delay(100);
            }

            Session.Reload();

            CuriosityPlugin.Instance.DetachTickHandler(Intro);

            var board = new MugshotBoardAttachment();

            player.DisableHud();

#pragma warning disable 4014            
            board.Attach(player);

            player.CameraQueue.View(new CameraBuilder()
                .WithMotionBlur(0.5f)
                .WithInterpolation(CameraViews[0], CameraViews[1], 5000)
            );
#pragma warning restore 4014

            API.DoScreenFadeIn(5000);

            await player.AnimationQueue.PlayDirectInQueue(new AnimationBuilder()
                .Select("mp_character_creation@customise@male_a", "intro")
            );

            player.AnimationQueue.AddToQueue(new AnimationBuilder()
                .Select("mp_character_creation@customise@male_a", "loop")
                .WithFlags(AnimationFlags.Loop)
                .SkipTask()
            ).PlayQueue();

            var menuDisplayed = false;

            while (!Cache.Character.MarkedAsRegistered)
            {
                try
                {
                    if (!menuDisplayed)
                    {
                        menuDisplayed = true;
                        creatorMenu.OpenMenu();
                    }

                    if (!API.IsEntityPlayingAnim(player.Entity.Id, "mp_character_creation@customise@male_a", "loop", 3))
                    {
                        player.AnimationQueue.AddToQueue(new AnimationBuilder()
                            .Select("mp_character_creation@customise@male_a", "loop")
                            .WithFlags(AnimationFlags.Loop)
                            .SkipTask()
                        ).PlayQueue();

                        board.IsAttached = false;

#pragma warning disable 4014
                        board.Attach(player);
#pragma warning restore 4014
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Character Init: {ex.Message}");
                }

                await BaseScript.Delay(100);
            }

            player.AnimationQueue.AddToQueue(new AnimationBuilder()
                .Select("mp_character_creation@customise@male_a", "outro")
                .WithFlags(AnimationFlags.StayInEndFrame)
            ).PlayQueue();

            API.DoScreenFadeOut(3000);

            await BaseScript.Delay(5500);

            Session.Join(1);

            board.IsAttached = false;

            await SafeTeleport.Teleport(player.Entity.Id, new Position(-1045.326f, -2750.793f, 21.36343f, 330.1637f));

#pragma warning disable 4014
            player.CameraQueue.View(new CameraBuilder()
#pragma warning restore 4014
                .WithMotionBlur(0.5f)
                .WithInterpolation(
                    new RotatablePosition(-1032.415f, -2726.847f, 26.48441f, 152.7852f, -7f, 0f),
                    new RotatablePosition(-1032.415f, -2726.847f, 22.23441f, 152.7852f, -7f, 0f),
                    10000)
            );

            API.DoScreenFadeIn(8000);

            API.TaskGoStraightToCoord(player.Entity.Id, DefaultPosition.X, DefaultPosition.Y, DefaultPosition.Z,
                1f,
                -1,
                DefaultPosition.H, 0f);

            while (DefaultPosition.Distance(player.Entity.Position) > 0.1 ||
                   Math.Abs(DefaultPosition.H - player.Entity.Position.H) > 1)
            {
                await BaseScript.Delay(10);
            }

            character.MarkedAsRegistered = true;

            // await character.Save();
            await player.AnimationQueue.PlayDirectInQueue(new AnimationBuilder()
                .Select("move_p_m_one_idles@generic", "fidget_look_around")
            );

            API.TransitionToBlurred(0f);

            player.EnableHud();
            player.CameraQueue.Reset();

            await BaseScript.Delay(10);

            API.TransitionFromBlurred(500f);

            await BaseScript.Delay(1000);

            Session.CreatingCharacter = false;
        }

        private static async Task Intro()
        {
            API.Set_2dLayer(7);
            API.SetTextCentre(true);
            API.SetTextFont(0);
            API.SetTextProportional(true);
            API.SetTextScale(0.45f, 0.45f);
            API.SetTextColour(255, 255, 255, 255);
            API.SetTextDropshadow(0, 0, 0, 0, 255);
            API.SetTextEdge(1, 0, 0, 0, 255);
            API.SetTextDropShadow();
            API.SetTextOutline();
            API.SetTextEntry("STRING");
            API.AddTextComponentString($"Welcome, {Cache.Player.Name}.");
            API.DrawText(0.5f, 0.5f - API.GetTextScaleHeight(0.45f, 0) / 2f);
            API.HideLoadingOnFadeThisFrame();

            await Task.FromResult(0);
        }

        public static void SetupStat(string stat, int value)
        {
            API.StatSetInt((uint)API.GetHashKey(stat), value, true);
        }

        public static void SetupStats(this CuriosityCharacter character)
        {
            foreach (KeyValuePair<string, int> keyValuePair in character.Stats)
            {
                API.StatSetInt((uint)API.GetHashKey(keyValuePair.Key), keyValuePair.Value, true);
            }
        }
    }
}