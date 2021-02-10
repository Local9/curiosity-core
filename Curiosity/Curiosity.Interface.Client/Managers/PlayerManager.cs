using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.PDA;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;

namespace Curiosity.Interface.Client.Managers
{
    public class PlayerManager : Manager<PlayerManager>
    {
        DateTime lastTick = DateTime.Now;

        public override void Begin()
        {
            Instance.AttachNuiHandler("PlayerProfile", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser curiosityUser = await EventSystem.Request<CuriosityUser>("character:get:profile");

                if (curiosityUser == null) return null;
                if (curiosityUser.Character == null) return null;

                PlayerProfile pp = new PlayerProfile();
                pp.UserID = curiosityUser.UserId;
                pp.Name = curiosityUser.LatestName;
                pp.Role = curiosityUser.Role.GetStringValue();
                pp.Wallet = curiosityUser.Character.Cash;

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
    }
}
