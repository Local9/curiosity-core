using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class PlayerManager : Manager<PlayerManager>
    {
        string CurrentPedHeadshot;

        List<string> MutedPlayers = new List<string>();

        public override void Begin()
        {
            Instance.AttachNuiHandler("GetProfile", new AsyncEventCallback(async metadata =>
            {
                await CreatePlayerHeadshot();
                await Session.Loading();

                CuriosityUser curiosityUser = await EventSystem.Request<CuriosityUser>("character:get:profile");

                if (curiosityUser == null) return null;
                if (curiosityUser.Character == null) return null;

                // TODO: Add world information
                // TODO: Smaller data sets for the request

                var profile = new {
                    userId = curiosityUser.UserId,
                    name = curiosityUser.LatestName,
                    role = curiosityUser.Role.GetStringValue(),
                    wallet = curiosityUser.Character.Cash,
                    isAdmin = curiosityUser.IsAdmin,
                    isStaff = curiosityUser.IsStaff,
                    headshot = $"https://nui-img/{CurrentPedHeadshot}/{CurrentPedHeadshot}",
                };

                return profile;
            }));

            Instance.AttachNuiHandler("GetEnhancedProfile", new AsyncEventCallback(async metadata =>
            {
                List<CharacterSkill> skills = new();
                List<CharacterStat> stats = new();

                int serverHandle = 0;

                int.TryParse(metadata.Find<string>(0), out serverHandle);

                if (serverHandle == 0)
                {
                    serverHandle = Game.Player.ServerId;
                }

                CuriosityUser curiosityUser = await EventSystem.Request<CuriosityUser>("character:get:profile:enhanced", serverHandle);

                if (serverHandle == Game.Player.ServerId || Cache.Player.User.IsAdmin)
                {
                    skills = await EventSystem.Request<List<CharacterSkill>>("character:get:skills:enhanced", serverHandle);
                    stats = await EventSystem.Request<List<CharacterStat>>("character:get:stats:enhanced", serverHandle);
                }

                if (curiosityUser == null) return null;

                var profile = new
                {
                    handle = curiosityUser.Handle,
                    userId = curiosityUser.UserId,
                    name = curiosityUser.LatestName,
                    role = curiosityUser.Role.GetStringValue(),
                    wallet = curiosityUser.Character.Cash,
                    isAdmin = curiosityUser.IsAdmin,
                    isStaff = curiosityUser.IsStaff,
                    headshot = $"https://nui-img/{CurrentPedHeadshot}/{CurrentPedHeadshot}",
                    skills = new List<dynamic>(),
                    stats = new List<dynamic>(),
                };

                if (skills is not null)
                {
                    foreach (CharacterSkill skill in skills)
                    {
                        var s = new
                        {
                            label = skill.Label,
                            description = skill.Description,
                            value = skill.Value
                        };
                        profile.skills.Add(s);
                    }
                }

                if (stats is not null)
                {
                    foreach (CharacterStat stat in stats)
                    {
                        var s = new
                        {
                            label = stat.Label,
                            value = stat.Value
                        };
                        profile.stats.Add(s);
                    }
                }

                return profile;
            }));

            Instance.AttachNuiHandler("PlayerList", new AsyncEventCallback(async metadata =>
            {
                List<CuriosityPlayerListItem> playerList = await EventSystem.Request<List<CuriosityPlayerListItem>>("user:get:playerlist");

                if (playerList == null) return null;

                var pl = new List<dynamic>();

                foreach(CuriosityPlayerListItem p in playerList)
                {
                    var player = new
                    {
                        id = p.UserId,
                        name = p.Name,
                        handle = p.ServerHandle,
                        job = p.Job,
                        role = p.Role,
                        ping = p.Ping,
                        routingBucket = p.RoutingBucket,
                        isMuted = MutedPlayers.Contains($"{p.ServerHandle}"),
                        discordUrl = p.DiscordAvatar
                    };

                    pl.Add(player);
                }

                return pl;
            }));

            Instance.AttachNuiHandler("ReportList", new AsyncEventCallback(async metadata =>
            {
                List<LogItem> lst = await EventSystem.Request<List<LogItem>>("user:report:list");

                var reasons = new List<dynamic>();

                foreach (LogItem item in lst)
                {
                    var r = new
                    {
                        logTypeId = item.LogTypeId,
                        group = item.Group,
                        description = item.Description,
                        playerHandle = item.PlayerHandle
                    };
                    reasons.Add(r);
                }

                return reasons;
            }));

            Instance.AttachNuiHandler("ToggleMute", new EventCallback(metadata =>
            {
                string playerId = metadata.Find<string>(0);
                int pId = 0;

                if (int.TryParse(playerId, out pId))
                {
                    Instance.ExportDictionary["pma-voice"].toggleMutePlayer(pId);

                    if (MutedPlayers.Contains(playerId))
                    {
                        MutedPlayers.Remove(playerId);
                        return new { success = true };
                    }

                    if (!MutedPlayers.Contains(playerId))
                    {
                        MutedPlayers.Add(playerId);
                        return new { success = true };
                    }
                }

                return new { success = true };
            }));

            Instance.AttachNuiHandler("ReportPlayer", new AsyncEventCallback(async metadata =>
            {
                int playerBeingReportedHandle = metadata.Find<int>(0);
                string playerBeingReported = metadata.Find<string>(1);
                string reason = metadata.Find<string>(2);

                bool reportSuccess = await EventSystem.Request<bool>("user:report:submit", playerBeingReportedHandle, playerBeingReported, reason);

                NotificationManager notification = NotificationManager.GetModule();

                if (reportSuccess)
                {
                    notification.SendNui(Systems.Library.Enums.Notification.NOTIFICATION_SUCCESS, "Report received.");
                }
                else
                {
                    notification.SendNui(Systems.Library.Enums.Notification.NOTIFICATION_ERROR, "Issue when creating report, please inform a member of staff.");
                }

                return null;
            }));

            EventSystem.Attach("user:screen:fadeOut", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    Screen.Fading.FadeOut(metadata.Find<int>(0));

                    while (Screen.Fading.IsFadingOut)
                    {
                        await BaseScript.Delay(100);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }));

            EventSystem.Attach("user:screen:fadeIn", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    Screen.Fading.FadeIn(metadata.Find<int>(0));

                    while (Screen.Fading.IsFadingIn)
                    {
                        await BaseScript.Delay(100);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }));

            EventSystem.Attach("user:position:move", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    Screen.Fading.FadeOut(200);

                    while (Screen.Fading.IsFadingOut)
                    {
                        await BaseScript.Delay(100);
                    }

                    float x = metadata.Find<float>(0);
                    float y = metadata.Find<float>(1);
                    float z = metadata.Find<float>(2);

                    Screen.Fading.FadeIn(1000);

                    while (Screen.Fading.IsFadingIn)
                    {
                        await BaseScript.Delay(100);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }));
        }

        private async Task CreatePlayerHeadshot()
        {
            if (!string.IsNullOrEmpty(CurrentPedHeadshot)) return;

            int handle = API.RegisterPedheadshot(Game.PlayerPed.Handle);
            int failCount = 0;
            while (!API.IsPedheadshotReady(handle) || !API.IsPedheadshotValid(handle))
            {
                await BaseScript.Delay(100);
                failCount++;

                if (failCount >= 10)
                    break;
            }
            CurrentPedHeadshot = API.GetPedheadshotTxdString(handle);
        }
    }
}
