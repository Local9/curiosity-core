using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.PDA;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class PlayerManager : Manager<PlayerManager>
    {
        string CurrentPedHeadshot;

        public override void Begin()
        {
            Instance.AttachNuiHandler("GetProfile", new AsyncEventCallback(async metadata =>
            {
                await CreatePlayerHeadshot();
                await Session.Loading();

                CuriosityUser curiosityUser = await EventSystem.Request<CuriosityUser>("character:get:profile");

                if (curiosityUser == null) return null;
                if (curiosityUser.Character == null) return null;

                PlayerProfile pp = new PlayerProfile();
                pp.UserID = curiosityUser.UserId;
                pp.Name = curiosityUser.LatestName;
                pp.Role = curiosityUser.Role.GetStringValue();
                pp.Wallet = curiosityUser.Character.Cash;

                pp.IsAdmin = curiosityUser.IsAdmin;
                pp.IsStaff = curiosityUser.IsStaff;
                pp.Headshot = $"https://nui-img/{CurrentPedHeadshot}/{CurrentPedHeadshot}";

                string jsn = new JsonBuilder().Add("operation", "PLAYER_PROFILE")
                        .Add("profile", pp)
                        .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("GetEnhancedProfile", new AsyncEventCallback(async metadata =>
            {
                int serverHandle = metadata.Find<int>(0);

                if (serverHandle == 0)
                {
                    serverHandle = Game.Player.ServerId;
                }

                CuriosityUser curiosityUser = await EventSystem.Request<CuriosityUser>("character:get:profile:enhanced", serverHandle);
                List<CharacterSkill> skills = await EventSystem.Request<List<CharacterSkill>>("character:get:skills:enhanced", serverHandle);
                List<CharacterStat> stats = await EventSystem.Request<List<CharacterStat>>("character:get:stats:enhanced", serverHandle);

                if (curiosityUser == null) return null;

                PlayerProfile pp = new PlayerProfile();
                pp.UserID = curiosityUser.UserId;
                pp.Name = curiosityUser.LatestName;
                pp.Role = curiosityUser.Role.GetStringValue();
                pp.Wallet = curiosityUser.Character.Cash;
                pp.ServerHandle = curiosityUser.Handle;
                pp.IsAdmin = curiosityUser.IsAdmin;
                pp.IsStaff = curiosityUser.IsStaff;

                pp.WorldName = $"{curiosityUser.RoutingBucket}";

                string jsn = new JsonBuilder().Add("operation", "ENHANCED_PROFILE")
                        .Add("profile", pp)
                        .Add("skills", skills)
                        .Add("stats", stats)
                        .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("PlayerExperience", new AsyncEventCallback(async metadata =>
            {
                List<CharacterSkill> skills = await EventSystem.Request<List<CharacterSkill>>("character:get:skills");

                if (skills == null) return null;

                string jsn = new JsonBuilder().Add("operation", "PLAYER_SKILLS")
                    .Add("skills", skills)
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("PlayerStats", new AsyncEventCallback(async metadata =>
            {
                List<CharacterStat> stats = await EventSystem.Request<List<CharacterStat>>("character:get:stats");

                if (stats == null) return null;

                string jsn = new JsonBuilder().Add("operation", "PLAYER_STATS")
                    .Add("stats", stats)
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("PlayerList", new AsyncEventCallback(async metadata =>
            {
                List<CuriosityPlayerList> playerList = await EventSystem.Request<List<CuriosityPlayerList>>("user:get:playerlist");

                if (playerList == null) return null;

                string jsn = new JsonBuilder().Add("operation", "PLAYER_LIST")
                    .Add("list", playerList)
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("ReportList", new AsyncEventCallback(async metadata =>
            {
                List<LogItem> lst = await EventSystem.Request<List<LogItem>>("user:report:list");

                string jsn = new JsonBuilder().Add("operation", "REPORT_REASONS")
                .Add("list", lst)
                .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("ReportPlayer", new AsyncEventCallback(async metadata =>
            {
                int playerBeingReportedHandle = metadata.Find<int>(0);
                string playerBeingReported = metadata.Find<string>(1);
                string reason = metadata.Find<string>(2);

                bool reportSuccess = await EventSystem.Request<bool>("user:report:submit", playerBeingReportedHandle, playerBeingReported, reason);

                NotificationManger notification = NotificationManger.GetModule();

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
