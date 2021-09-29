using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Racing.Client.Diagnostics;
using Curiosity.Racing.Client.Environment.Entities;
using Curiosity.Racing.Client.Environment.Entities.Models;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Racing.Client.Managers
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

            var sound = new SoundSystem();
            var handle = Game.Player.Handle;
            var serverHandle = Game.Player.ServerId;
            var pedHandle = Game.PlayerPed.Handle;

            sound.Disable();

            Model playerModel = PedHash.FreemodeMale01;
            await playerModel.Request(10000);

            await Game.Player.ChangeModel(playerModel);

            playerModel.MarkAsNoLongerNeeded();

            Game.PlayerPed.Weapons.RemoveAll();
            Game.PlayerPed.Task.ClearAllImmediately();
            API.ClearPlayerWantedLevel(handle);
            Game.PlayerPed.IsVisible = true;
            Game.PlayerPed.Health = API.GetEntityMaxHealth(pedHandle);
            API.NetworkResurrectLocalPlayer(0, 0, 70f, 0, true, false);
            Game.PlayerPed.IsPositionFrozen = false;
            Screen.LoadingPrompt.Show("Loading...");

            var transition = new CharacterManager.LoadTransition();
            var user = await EventSystem.Request<CuriosityUser>("user:login");


            Logger.Info($"[User] [{user.DiscordId}] Creating local player...");

            Curiosity.Local = new CuriosityPlayer(user.DiscordId, new CuriosityEntity(pedHandle))
            {
                Handle = serverHandle,
                Name = user.LatestName,
                User = user
            };

            var voice = VoiceChat.GetModule();
            voice.Range = 0f;
            voice.Commit();

            Curiosity.AttachTickHandlers(voice);
            Curiosity.Local.DisableHud();

            Logger.Info($"[User] [{user.DiscordId}] Logged in with `{user.Role}`");
            transition.Up(Curiosity.Local);

            await CharacterManager.GetModule().Synchronize();
        }
    }
}
