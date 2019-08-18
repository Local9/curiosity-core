using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.Shared.Server.net.Helpers;

using GlobalEntity = Curiosity.Global.Shared.net.Entity;
using GlobalEnum = Curiosity.Global.Shared.net.Enums;

namespace Curiosity.Server.net.Classes
{
    class Skills
    {
        static Dictionary<string, GlobalEntity.Skills> skills = new Dictionary<string, GlobalEntity.Skills>();

        static Server server = Server.GetInstance();

        static long skillTicker = API.GetGameTimer();
        static int skillMinuteUpdate = (Server.isLive ? 30 : 5);

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Skills:Increase", new Action<CitizenFX.Core.Player, string, int>(IncreaseSkillByPlayer));
            server.RegisterEventHandler("curiosity:Server:Skills:Decrease", new Action<CitizenFX.Core.Player, string, int>(DecreaseSkillByPlayer));
            server.RegisterEventHandler("curiosity:Server:Skills:Get", new Action<CitizenFX.Core.Player>(GetUserSkills));
            server.RegisterEventHandler("curiosity:Server:Skills:GetListData", new Action<CitizenFX.Core.Player, int>(GetListData));

            server.RegisterTickHandler(UpdateSkillsDictionary);
        }

        async static void GetListData([FromSource]CitizenFX.Core.Player player, int skillTypeId)
        {
            try
            {
                await BaseScript.Delay(0);
                List<GlobalEntity.Skills> skillsList = new List<GlobalEntity.Skills>();

                GlobalEnum.SkillType skillType = (GlobalEnum.SkillType)skillTypeId;

                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    skillsList.Add(new GlobalEntity.Skills { Label = "Please try again", Value = 0 });
                }
                else
                {
                    Session session = SessionManager.PlayerList[player.Handle];
                    foreach (KeyValuePair<string, GlobalEntity.Skills> skill in session.Skills)
                    {
                        if (skill.Value.TypeId == skillType)
                        {
                            skillsList.Add(new GlobalEntity.Skills { Label = skill.Value.Label, Value = skill.Value.Value, Description = skill.Value.Description, LabelDescription = skill.Value.LabelDescription });
                        }
                    }
                }

                GlobalEntity.NuiData nuiData = new GlobalEntity.NuiData();
                nuiData.panel = $"{skillType}";
                nuiData.skills = skillsList;
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(nuiData);

                player.TriggerEvent("curiosity:Player:Skills:GetListData", json);
            }
            catch (Exception ex)
            {
                Log.Error($"GetListData -> {ex}");
            }
        }

        async static void GetUserSkills([FromSource]CitizenFX.Core.Player player)
        {
            try
            {
                Dictionary<string, GlobalEntity.Skills> skills = new Dictionary<string, GlobalEntity.Skills>();

                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    skills.Add("Session Loading...", new GlobalEntity.Skills { Id = 0, TypeId = 0, Description = "Loading", Label = "Loading" });
                }
                else
                {
                    Session session = SessionManager.PlayerList[player.Handle];
                    if (session.Skills.Count > 0)
                    {
                        skills = session.Skills;
                    }
                    else
                    {
                        skills = await Database.DatabaseUsersSkills.GetSkills(session.User.CharacterId);
                        session.Skills = skills;
                    }
                }
                player.TriggerEvent("curiosity:Player:Skills:Get", Newtonsoft.Json.JsonConvert.SerializeObject(skills));
            }
            catch (Exception ex)
            {
                Log.Error($"GetUserSkills -> {ex}");
            }
        }

        async static Task UpdateSkillsDictionary()
        {
            while (!Server.serverActive)
            {
                await BaseScript.Delay(0);
            }

            if (skills.Count == 0 && (API.GetGameTimer() - skillTicker) > 1000)
            {
                skillTicker = API.GetGameTimer();
                skills = await Database.DatabaseUsersSkills.GetSkills();
                Log.Verbose($"Skills -> {skills.Count} Found.");
            } else if ((API.GetGameTimer() - skillTicker) > (1000 * 60) * skillMinuteUpdate)
            {
                skillTicker = API.GetGameTimer();
                skills = await Database.DatabaseUsersSkills.GetSkills();
                Log.Verbose($"Skills -> {skills.Count} Found. Next update in {skillMinuteUpdate} mins.");
            }
        }

        static void IncreaseSkillByPlayer([FromSource]CitizenFX.Core.Player player, string skill, int experience)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    Log.Error($"IncreaseSkill: Player session missing.");
                    return;
                }

                Session session = SessionManager.PlayerList[player.Handle];

                int characterId = session.User.CharacterId;

                if (!(characterId > 0))
                {
                    Log.Error($"IncreaseSkill: characterId Missing");
                    return;
                }

                if (!skills.ContainsKey(skill))
                {
                    Log.Error($"IncreaseSkill: Unknown Skill -> {skill}");
                    Log.Error($"IncreaseSkill: Known Skills -> {String.Join("-", skills.Select(x => x.Key))}");
                    return;
                }

                if (!Server.isLive)
                {
                    Log.Success($"IncreaseSkill: {skill} + {experience}");
                }

                Database.DatabaseUsersSkills.IncreaseSkill(characterId, skills[skill].Id, experience);

                if (skills[skill].TypeId == GlobalEnum.SkillType.Experience)
                    UpdateLifeExperience(session, experience, false);

                if (!session.Skills.ContainsKey(skill))
                {
                    session.Skills.Add(skill, skills[skill]);
                    session.Skills[skill].Value = 0 + experience;
                }

                session.Skills[skill].Value = session.Skills[skill].Value + experience;
                PlayerMethods.SendUpdatedInformation(session);

                if (!Server.isLive)
                {
                    player.TriggerEvent("curiosity:Client:Chat:Message", "SERVER", "#FF0000", $"IncreaseSkill: {skill} + {experience}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"IncreaseSkill -> {ex.Message}");
            }
        }

        static void DecreaseSkillByPlayer([FromSource]CitizenFX.Core.Player player, string skill, int experience)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    Log.Error($"DecreaseSkill: Player session missing.");
                    return;
                }

                Session session = SessionManager.PlayerList[player.Handle];

                int characterId = session.User.CharacterId;

                if (!(characterId > 0))
                {
                    Log.Error($"DecreaseSkill: characterId Missing");
                    return;
                }

                if (!skills.ContainsKey(skill))
                {
                    Log.Error($"DecreaseSkill: Unknown Skill -> {skill}");
                    Log.Error($"DecreaseSkill: Known Skills -> {String.Join("-", skills.Select(x => x.Key))}");
                    return;
                }

                if (!Server.isLive)
                {
                    Log.Success($"DecreaseSkill: {skill} - {experience}");
                }

                Database.DatabaseUsersSkills.DecreaseSkill(characterId, skills[skill].Id, experience);

                if (skills[skill].TypeId == GlobalEnum.SkillType.Experience)
                    UpdateLifeExperience(session, experience, false);

                if (!session.Skills.ContainsKey(skill))
                {
                    session.Skills.Add(skill, skills[skill]);
                    session.Skills[skill].Value = 0 - experience;
                }

                session.Skills[skill].Value = session.Skills[skill].Value - experience;
                PlayerMethods.SendUpdatedInformation(session);

                if (!Server.isLive)
                {
                    player.TriggerEvent("curiosity:Client:Chat:Message", "SERVER", "#FF0000", $"DecreaseSkill: {skill} - {experience}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DecreaseSkill -> {ex.Message}");
            }
        }

        static void UpdateLifeExperience(Session session, int experience, bool removeXp)
        {
            string eventToTrigger;
            if (removeXp)
            {
                Database.DatabaseUsers.DecreaseLiveExperience(session.User.UserId, experience);
                eventToTrigger = "curiosity:Client:Rank:RemovePlayerXP";
            }
            else
            {
                Database.DatabaseUsers.IncreaseLiveExperience(session.User.UserId, experience);
                eventToTrigger = "curiosity:Client:Rank:AddPlayerXP";
            }
            session.Player.TriggerEvent(eventToTrigger, experience);
        }
    }
}
