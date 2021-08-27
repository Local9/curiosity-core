using CitizenFX.Core;
using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Environment.Entities;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.MissionManager.Client.Managers
{
    public class LoginManager : Manager<LoginManager>
    {
        public override async void Begin()
        {
            Logger.Info($"- [LoginManager] Begin ---------------------------");

            var handle = Game.Player.Handle;
            var serverHandle = Game.Player.ServerId;
            Cache.UpdatePedId();

            var pedHandle = Cache.PlayerPed.Handle;

            var user = await EventSystem.Request<CuriosityUser>("user:login:module");

            Logger.Info($"[User] [{user.DiscordId}] Creating local player...");

            Instance.Local = new CuriosityPlayer(user.DiscordId, new CuriosityEntity(pedHandle))
            {
                Handle = serverHandle,
                Name = user.LatestName,
                User = user
            };

            Instance.DiscordRichPresence.Status = $"Freeroam";
            Instance.DiscordRichPresence.Commit();

            Logger.Info($"[User] [{user.DiscordId}] Logged in with `{user.Role}`");

            Instance.ExportRegistry.Add("RefreshPlayerPed", new Func<bool>(
                () =>
                {
                    Cache.UpdatePedId();
                    return false;
                }));
        }
    }
}
