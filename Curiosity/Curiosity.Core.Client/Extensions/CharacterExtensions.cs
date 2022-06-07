using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Events;
using Curiosity.Core.Client.Interface;
using Curiosity.Core.Client.Interface.Menus.Creator;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Extensions
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
            character = Cache.Character;

            Vector3 vPos = Game.PlayerPed.Position;
            float groundZ = vPos.Z;
            if (API.GetGroundZFor_3dCoord(vPos.X, vPos.Y, vPos.Z, ref groundZ, false))
                vPos = new Vector3(vPos.X, vPos.Y, groundZ);

            Position position = new Position(vPos.X, vPos.Y, vPos.Z, Game.PlayerPed.Heading);
            character.LastPosition = position;

            //Logger.Debug($"[LAST POSITION] {character.LastPosition}");

            await BaseScript.Delay(100);

            character.IsDead = Cache.PlayerPed.IsDead;
            character.IsPassive = PlayerOptionsManager.GetModule().IsPassive;
            character.AllowHelmet = Cache.Character.AllowHelmet;

            if (Cache.Player.User.IsSeniorDeveloper)
                Logger.Debug($"{character}");

            bool success = await EventSystem.GetModule().Request<bool>("character:save", character);

            if (success)
            {
                Logger.Debug($"Character Saved");
            }
            else
            {
                Logger.Debug($"Didn't save shit");
            }

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

            int playerPedId = Game.PlayerPed.Handle;

            API.SetPedHeadBlendData(playerPedId, fatherId, motherId, 0, fatherId, motherId, 0, remBlend, skinBlend, 0f, false);

            if (character.CharacterInfo.DrawableVariations.Count == 0)
            {
                API.SetPedComponentVariation(playerPedId, 3, 15, 0, 0);
                API.SetPedComponentVariation(playerPedId, 8, 15, 0, 0);
                API.SetPedComponentVariation(playerPedId, 11, 15, 0, 0);
                API.SetFacialIdleAnimOverride(playerPedId, "mood_Normal_1", null);
            }
            else
            {
                foreach (KeyValuePair<int, KeyValuePair<int, int>> kvp in character.CharacterInfo.DrawableVariations)
                {
                    API.SetPedComponentVariation(playerPedId, kvp.Key, kvp.Value.Key, kvp.Value.Value, 2);
                }
            }

            if (character.CharacterInfo.Props.Count > 0)
            {
                foreach (KeyValuePair<int, KeyValuePair<int, int>> kvp in character.CharacterInfo.Props)
                {
                    API.SetPedPropIndex(playerPedId, kvp.Key, kvp.Value.Key, kvp.Value.Value, true);
                }
            }

            foreach (KeyValuePair<int, float> keyValuePair in character.Features)
            {
                API.SetPedFaceFeature(playerPedId, keyValuePair.Key, keyValuePair.Value);
            }

            API.ClearPedFacialDecorations(playerPedId);

            if (character.Appearance.HairStyle == 0)
            {
                API.SetPedComponentVariation(playerPedId, 2, 0, 0, 0);
            }
            else
            {
                API.SetPedComponentVariation(playerPedId, 2, character.Appearance.HairStyle, 0, 0);
                if (!character.Appearance.HairOverlay.Equals(new KeyValuePair<string, string>()))
                {
                    KeyValuePair<string, string> overlay = character.Appearance.HairOverlay;
                    API.SetPedFacialDecoration(playerPedId, (uint)API.GetHashKey(overlay.Key), (uint)API.GetHashKey(overlay.Value));
                }

                API.SetPedHairColor(playerPedId, character.Appearance.HairPrimaryColor, character.Appearance.HairSecondaryColor);
            }

            API.SetPedHeadOverlay(playerPedId, 1, character.Appearance.FacialHair, character.Appearance.FacialHairOpacity);
            API.SetPedHeadOverlayColor(playerPedId, 1, 1, character.Appearance.FacialHairColor, character.Appearance.FacialHairColor);
            API.SetPedHeadOverlay(playerPedId, 2, character.Appearance.Eyebrow, character.Appearance.EyebrowOpacity);
            API.SetPedHeadOverlayColor(playerPedId, 2, 1, character.Appearance.EyebrowColor, character.Appearance.EyebrowColor);
            API.SetPedHeadOverlay(playerPedId, 4, character.Appearance.EyeMakeup, character.Appearance.EyeMakeupOpacity);
            API.SetPedHeadOverlayColor(playerPedId, 4, 2, character.Appearance.EyeMakeupColor, character.Appearance.EyeMakeupColor);
            API.SetPedHeadOverlay(playerPedId, 5, character.Appearance.Blusher, character.Appearance.BlusherOpacity);
            API.SetPedHeadOverlayColor(playerPedId, 5, 2, character.Appearance.BlusherColor, character.Appearance.BlusherColor);
            API.SetPedHeadOverlay(playerPedId, 8, character.Appearance.Lipstick, character.Appearance.LipstickOpacity);
            API.SetPedHeadOverlayColor(playerPedId, 8, 2, character.Appearance.LipstickColor, character.Appearance.LipstickColor);

            API.SetPedEyeColor(playerPedId, character.Appearance.EyeColor);

            API.SetPedHeadOverlay(playerPedId, 0, character.Appearance.SkinBlemish, character.Appearance.SkinBlemishOpacity);
            API.SetPedHeadOverlay(playerPedId, 3, character.Appearance.SkinAging, character.Appearance.SkinAgingOpacity);
            API.SetPedHeadOverlay(playerPedId, 6, character.Appearance.SkinComplexion, character.Appearance.SkinComplexionOpacity);
            API.SetPedHeadOverlay(playerPedId, 7, character.Appearance.SkinDamage, character.Appearance.SkinDamageOpacity);
            API.SetPedHeadOverlay(playerPedId, 9, character.Appearance.SkinMoles, character.Appearance.SkinMolesOpacity);

            // remove all decorations, and then manually re-add them all. what a retarded way of doing this R*....
            ClearPedDecorations(playerPedId);

            foreach (var tattoo in character.Tattoos.HeadTattoos)
            {
                SetPedDecoration(playerPedId, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.Tattoos.TorsoTattoos)
            {
                SetPedDecoration(playerPedId, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.Tattoos.LeftArmTattoos)
            {
                SetPedDecoration(playerPedId, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.Tattoos.RightArmTattoos)
            {
                SetPedDecoration(playerPedId, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.Tattoos.LeftLegTattoos)
            {
                SetPedDecoration(playerPedId, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.Tattoos.RightLegTattoos)
            {
                SetPedDecoration(playerPedId, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }
            foreach (var tattoo in character.Tattoos.BadgeTattoos)
            {
                SetPedDecoration(playerPedId, (uint)GetHashKey(tattoo.Key), (uint)GetHashKey(tattoo.Value));
            }

            if (!string.IsNullOrEmpty(character.Appearance.HairOverlay.Key) && !string.IsNullOrEmpty(character.Appearance.HairOverlay.Value))
            {
                // reset hair value
                SetPedDecoration(playerPedId, (uint)GetHashKey(character.Appearance.HairOverlay.Key), (uint)GetHashKey(character.Appearance.HairOverlay.Value));
            }

            if (character.IsDead)
            {
                Logger.Debug($"[LOAD] Killed Ped");
                Cache.PlayerPed.Health = 0;
                Cache.PlayerPed.Armor = 0;
                Cache.PlayerPed.Kill();
            }
            else
            {
                Cache.PlayerPed.Health = character.Health;
                Cache.PlayerPed.Armor = character.Armor;
            }

            Logger.Debug($"[LOAD] Character Style Complete");

            character.SetupStats();

            //var voice = VoiceChat.GetModule();
            //voice.Range = VoiceChatRange.Normal;
            //voice.Commit();
        }

        public static async void Revive(this CuriosityCharacter character, Position position)
        {
            await Cache.PlayerPed.FadeOut();

            Cache.PlayerPed.Task.ClearAllImmediately();
            Game.Player.WantedLevel = 0;
            Cache.PlayerPed.IsVisible = true;
            Cache.PlayerPed.Health = Cache.PlayerPed.MaxHealth;
            Cache.PlayerPed.Armor = 0;
            Cache.PlayerPed.ClearBloodDamage();
            Cache.PlayerPed.ClearLastWeaponDamage();

            StopEntityFire(Cache.PlayerPed.Handle);
            SetPedStealthMovement(Cache.PlayerPed.Handle, false, "0");

            Cache.Player.Character.IsDead = false;

            API.NetworkResurrectLocalPlayer(position.X, position.Y, position.Z, position.H, false, false);
            Cache.PlayerPed.IsPositionFrozen = true;
            PlaceObjectOnGroundProperly(Cache.PlayerPed.Handle);

            await Cache.PlayerPed.FadeIn();

            Cache.PlayerPed.IsPositionFrozen = false;
            Cache.Player.EnableHud();
        }

        public static async Task PostLoad(this CuriosityCharacter character)
        {
            CreatorMenus creatorMenu = new CreatorMenus();
            creatorMenu.CreateMenu();

            Screen.LoadingPrompt.Show("Loading Create Character...");

            PluginManager.Instance.DiscordRichPresence.Status = "Creating Character";
            PluginManager.Instance.DiscordRichPresence.Commit();

            Session.CreatingCharacter = true;

            var player = Cache.Player;
            var timestamp = API.GetGameTimer();

            Vector3 endPosition = new Vector3(402.8841f, -996.4642f, -99.00024f);
            Game.PlayerPed.Position = new Vector3(405.9247f, -997.2114f, -99.00401f);
            Game.PlayerPed.Heading = 86.36787f;

            while (timestamp + 5000 > API.GetGameTimer())
            {
                await BaseScript.Delay(100);
            }

            Session.Reload();

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

            DateTime waitTime = DateTime.UtcNow.AddSeconds(3);

            while (true)
            {
                await BaseScript.Delay(1);
                if (DateTime.UtcNow > waitTime) break;
            }

            if (!Cache.PlayerPed.IsInRangeOf(endPosition, 0.1f))
            {
                await Cache.PlayerPed.FadeOut();

                await SafeTeleport.Teleport(player.Entity.Id, new Position(endPosition.X, endPosition.Y, endPosition.Z, 177.9398f));

                //Cache.PlayerPed.Position = endPosition;
                //Cache.PlayerPed.Heading = 177.9398f;

                await Cache.PlayerPed.FadeIn();
            }

            var menuDisplayed = false;

            while (!Cache.Character.MarkedAsRegistered)
            {
                try
                {
                    if (Screen.LoadingPrompt.IsActive)
                    {
                        Screen.LoadingPrompt.Hide();
                    }

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

            /// STUPID HACK for character creation
            var transition = new CharacterManager.LoadTransition();
            await transition.Up(player);
            await transition.Down(player);

            Screen.LoadingPrompt.Show("Loading...");

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

            Cache.Character.MarkedAsRegistered = true;
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

            Notify.Info($"~w~Welcome ~y~{Game.Player.Name}~w~, press ~b~HOME~w~ key to get started.");
            PlayerOptionsManager.GetModule().TogglePlayerPassive(true);

            Session.CreatingCharacter = false;
        }

        public static async void SetupStats(this CuriosityCharacter character)
        {
            LevelManager levelManager = LevelManager.GetModule();
            PlayerStatManager playerStatManager = PlayerStatManager.GetModule();

            List<CharacterStat> characterStats = await EventSystem.GetModule().Request<List<CharacterStat>>("character:get:stats:enhanced", Game.Player.ServerId);

            Logger.Debug($"[LOAD] Setting up stats");

            foreach (CharacterStat stat in characterStats)
            {
                string statStr = "";
                int lvl = 0;

                Logger.Debug($"{stat.Id}:{stat.Label}");

                switch ((Stat)stat.Id)
                {
                    case Stat.STAT_FLYING_ABILITY:
                        statStr = character.MP0_FLYING_ABILITY;
                        break;
                    case Stat.STAT_SWIMMING: // DONE
                        statStr = character.MP0_LUNG_CAPACITY;
                        playerStatManager.TotalSwiming = (int)stat.Value;
                        break;
                    case Stat.STAT_SHOOTING_ABILITY:
                        statStr = character.MP0_SHOOTING_ABILITY;
                        break;
                    case Stat.STAT_SPRINTING: // DONE
                        statStr = character.MP0_STAMINA;
                        playerStatManager.TotalSprinting = (int)stat.Value;
                        break;
                    case Stat.STAT_STEALTH_ABILITY:
                        statStr = character.MP0_STEALTH_ABILITY;
                        break;
                    case Stat.STAT_STRENGTH:
                        statStr = character.MP0_STRENGTH;
                        break;
                    case Stat.STAT_WHEELIE_ABILITY:
                        statStr = character.MP0_WHEELIE_ABILITY;
                        break;
                }

                if (string.IsNullOrEmpty(statStr)) continue;

                int statLevel = levelManager.GetLevelForXP((int)stat.Value, PlayerStatManager.MAX_EXP, PlayerStatManager.MAX_LEVEL);
                uint hash = (uint)API.GetHashKey(statStr);

                Logger.Debug($"{statStr}:{statLevel}");

                playerStatManager.SetStatValue(statLevel, hash);
            }
        }
    }
}