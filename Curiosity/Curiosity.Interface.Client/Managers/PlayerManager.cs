using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.PDA;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client.Managers
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
