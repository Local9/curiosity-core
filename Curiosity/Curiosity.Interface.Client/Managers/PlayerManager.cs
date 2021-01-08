using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
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
                string role = "USER";

                CuriosityUser curiosityUser = await EventSystem.Request<CuriosityUser>("user:getProfile");

                switch (curiosityUser.Role)
                {
                    case Role.DONATOR_LIFE:
                        role = "LifeV Early Supporter";
                        break;
                    case Role.DONATOR_LEVEL_1:
                        role = "LifeV Supporter I";
                        break;
                    case Role.DONATOR_LEVEL_2:
                        role = "LifeV Supporter II";
                        break;
                    case Role.DONATOR_LEVEL_3:
                        role = "LifeV Supporter III";
                        break;
                    default:
                        role = $"{curiosityUser.Role}".ToLowerInvariant();
                        break;
                }

                string jsn = new JsonBuilder().Add("operation", "PLAYER_PROFILE")
                        .Add("name", curiosityUser.LatestName)
                        .Add("userId", curiosityUser.UserId)
                        .Add("role", role)
                        .Add("wallet", curiosityUser.Wallet)
                        .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("PlayerExperience", new AsyncEventCallback(async metadata =>
            {
                List<Skill> skills = await EventSystem.Request<List<Skill>>("user:getSkills");

                string jsn = new JsonBuilder().Add("operation", "PLAYER_SKILLS")
                    .Add("skills", skills)
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("PlayerStats", new AsyncEventCallback(async metadata =>
            {
                List<Skill> stats = await EventSystem.Request<List<Skill>>("user:getStats");

                string jsn = new JsonBuilder().Add("operation", "PLAYER_STATS")
                    .Add("stats", stats)
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));
        }

        [TickHandler(SessionWait = true)]
        private async void OnPlayerTick()
        {
            if (DateTime.Now.Subtract(lastTick).TotalSeconds > 2.5)
            {
                lastTick = DateTime.Now;

                WeaponHash weapon = Game.PlayerPed.Weapons.Current;
                bool result = await EventSystem.Request<bool>("user:license:weapon", weapon);
                
                if (!result)
                {
                    Game.PlayerPed.Weapons.Remove(weapon);
                }
            }
        }
    }
}
