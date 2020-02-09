using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.System.Client.Environment.Entities.Models;
using Curiosity.Systems.Client.Diagnostics;
using Curiosity.Systems.Client.Environment;
using Curiosity.Systems.Client.Environment.Entities.Models;
using Curiosity.Systems.Client.Events;
using Curiosity.Systems.Client.Interface;
using Curiosity.Systems.Client.Interface.Menus;
using Curiosity.Systems.Client.Managers;
using Curiosity.Systems.Library.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Extensions
{
    public static class CharacterExtensions
    {
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

        //public static async Task Save(this CuriosityCharacter character)
        //{
        //    character.Metadata.LastPosition = Cache.Entity.Position;

        //    await EventSystem.GetModule().Request<object>("characters:save", character);

        //    Logger.Info($"[Characters] Saved `{character.CharacterId}` and it's changes.");
        //}

        public static async Task Load(this CuriosityCharacter character)
        {
            var player = Cache.Player;

            API.SetPedDefaultComponentVariation(player.Entity.Id);

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
            API.NetworkResurrectLocalPlayer(position.X, position.Y, position.Z, position.Heading, false, false);
            API.ClearPedBloodDamage(ped);
        }

        public static async Task PostLoad(this CuriosityCharacter character)
        {
            PlayerAppearance playerAppearance = new PlayerAppearance();
            playerAppearance.CreateMenu();

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

            var registered = false;
            var menuDisplayed = false;
            var view = 0;

            //StyleManager.GetModule().OpenStyleChange(character.Style, "General", 0, async type =>
            //{
            //    if (type == 0)
            //    {
            //        registered = true;
            //    }
            //    else
            //    {
            //        var item = InterfaceManager.GetModule().MenuContext.Selected;

            //        if (item == null) return;

            //        var head = HeadComponents.Select(self => self.ToLower())
            //            .Contains(item.Seed.Replace("style_component_", "").ToLower());

            //        if (head && view == 0)
            //        {
            //            view = 1;

            //            player.CameraQueue.Reset();
                        
            //            await player.CameraQueue.View(new CameraBuilder()
            //                .SkipTask()
            //                .WithMotionBlur(0.5f)
            //                .WithInterpolation(CameraViews[1], CameraViews[2], 300)
            //            );
            //        }
            //        else if (!head && view == 1)
            //        {
            //            view = 0;
      
            //            player.CameraQueue.Reset();

            //            await player.CameraQueue.View(new CameraBuilder()
            //                .SkipTask()
            //                .WithMotionBlur(0.5f)
            //                .WithInterpolation(CameraViews[2], CameraViews[1], 300)
            //            );
            //        }
            //    }
            //}, "CHAR_CREATE", "All");

            while (!registered)
            {
                try
                {
                    if (!menuDisplayed)
                    {
                        menuDisplayed = true;
                        playerAppearance.OpenMenu();
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
                DefaultPosition.Heading, 0f);

            while (DefaultPosition.Distance(player.Entity.Position) > 0.1 ||
                   Math.Abs(DefaultPosition.Heading - player.Entity.Position.Heading) > 1)
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
            API.AddTextComponentString($"Welcome to Life V, {Cache.Player.Name}.");
            API.DrawText(0.5f, 0.5f - API.GetTextScaleHeight(0.45f, 0) / 2f);
            API.HideLoadingOnFadeThisFrame();

            await Task.FromResult(0);
        }
    }
}