﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Core.Client.Managers
{
    public class LoginManager : Manager<LoginManager>
    {
        public override async void Begin()
        {
            Logger.Info($"--------------------------------------------------");
            Logger.Info($"------------ [LoginManager] Begin ----------------");
            Logger.Info($"--------------------------------------------------");

            API.NetworkSetFriendlyFireOption(false);
            API.SetCanAttackFriendly(API.PlayerPedId(), false, false);
            API.StopPlayerSwitch();

            Cache.UpdatePedId();

            var sound = new SoundSystem();
            var handle = Game.Player.Handle;
            var serverHandle = Game.Player.ServerId;
            var pedHandle = Cache.PlayerPed.Handle;

            sound.Disable();

            Model playerModel = PedHash.FreemodeMale01;
            await playerModel.Request(10000);

            await Game.Player.ChangeModel(playerModel);

            playerModel.MarkAsNoLongerNeeded();

            Cache.PlayerPed.Weapons.RemoveAll();
            Cache.PlayerPed.Task.ClearAllImmediately();
            API.ClearPlayerWantedLevel(handle);
            Cache.PlayerPed.IsVisible = true;
            Cache.PlayerPed.Health = API.GetEntityMaxHealth(pedHandle);
            API.NetworkResurrectLocalPlayer(0, 0, 70f, 0, true, false);
            Cache.PlayerPed.IsPositionFrozen = false;
            Screen.LoadingPrompt.Show("Loading...");

            var transition = new CharacterManager.LoadTransition();
            var user = await EventSystem.Request<CuriosityUser>("user:login");

            Logger.Info($"[User] [{user.DiscordId}] Creating local player...");

            Instance.Local = new CuriosityPlayer(user.DiscordId, new CuriosityEntity(pedHandle))
            {
                Handle = serverHandle,
                Name = user.LatestName,
                User = user
            };

            //var voice = VoiceChat.GetModule();
            //voice.Range = 0f;
            //voice.Commit();

            //Instance.AttachTickHandlers(voice);
            Instance.Local.DisableHud();

            Logger.Info($"[User] [{user.DiscordId}] Logged in with `{user.Role}`");
            transition.Up(Instance.Local);

            await CharacterManager.GetModule().Synchronize();
        }
    }
}
