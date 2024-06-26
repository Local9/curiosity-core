﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GlobalEntity = Curiosity.Global.Shared.Entity;
using GlobalEnum = Curiosity.Global.Shared.Enums;

namespace Curiosity.Server.net.Classes
{
    class Skills
    {
        static Dictionary<string, GlobalEntity.Skills> skills = new Dictionary<string, GlobalEntity.Skills>();

        static Server server = Server.GetInstance();

        static long skillTicker = API.GetGameTimer();
        static int skillMinuteUpdate = (Server.isLive ? 30 : 5);
        static DateTime lastUpdate = DateTime.Now;

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Skills:Increase:Server", new Action<string, string, int>(IncreaseSkillByPlayerId));
            server.RegisterEventHandler("curiosity:Server:Skills:Increase", new Action<CitizenFX.Core.Player, string, int>(IncreaseSkillByPlayer));
            server.RegisterEventHandler("curiosity:Server:Skills:Decrease", new Action<CitizenFX.Core.Player, string, int>(DecreaseSkillByPlayer));
            server.RegisterEventHandler("curiosity:Server:Skills:Get", new Action<CitizenFX.Core.Player>(GetUserSkills));
            server.RegisterEventHandler("curiosity:Server:Skills:GetListData", new Action<CitizenFX.Core.Player, int>(GetListData));

            server.RegisterTickHandler(UpdateSkillsDictionary);

            server.ExportDictionary.Add("increaseSkill", new Func<string, string, int, bool>(
                (player, skill, xp) =>
                {
                    try
                    {
                        if (!SessionManager.PlayerList.ContainsKey(player))
                        {
                            Log.Error($"No player found with the handle {player}");
                            return false;
                        }

                        IncreaseSkillByPlayerExport(player, skill, xp);

                        if (Server.ShowExportMessages)
                            Log.Success($"[EXPORT] increaseSkill called, {player}, {skill}, {xp}");

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[EXPORT] increaseSkill : {ex.Message}");

                        return false;
                    }
                }
            ));

            server.ExportDictionary.Add("decreaseSkill", new Func<string, string, int, bool>(
                (player, skill, xp) =>
                {
                    try
                    {
                        if (!SessionManager.PlayerList.ContainsKey(player))
                        {
                            Log.Error($"No player found with the handle {player}");
                            return false;
                        }

                        DecreaseSkillByPlayerExport(player, skill, xp);

                        if (Server.ShowExportMessages)
                            Log.Success($"[EXPORT] decreaseSkill called, {player}, {skill}, {xp}");

                        return true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[EXPORT] increaseSkill : {ex.Message}");

                        return false;
                    }
                }
            ));

        }

        public static void DecreaseSkillByPlayerExport(string playerId, string skill, int experience)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(playerId))
                {
                    Log.Error($"DecreaseSkill: Player session missing.");
                    return;
                }

                if (!skills.ContainsKey(skill))
                {
                    Log.Error($"DecreaseSkill: Skill is missing.");
                    Log.Error($"DecreaseSkill: Known Skills -> {String.Join("-", skills.Select(x => x.Key))}");
                    return;
                }

                Session session = SessionManager.PlayerList[playerId];

                GlobalEntity.Skills skillEnt = skills[skill];

                long characterId = session.User.CharacterId;

                if (!(characterId > 0))
                {
                    Log.Error($"DecreaseSkill: characterId Missing");
                    return;
                }

                if (!Server.isLive)
                {
                    Log.Success($"DecreaseSkill {session.Player.Name}: {skill} + {experience}");
                }

                Database.DatabaseUsersSkills.DecreaseSkill(characterId, skills[skill].Id, experience);

                if (skills[skill].TypeId == GlobalEnum.SkillType.Experience)
                    UpdateLifeExperience(session, experience, true);

                session.DecreaseSkill(skill, skills[skill], experience);

                PlayerMethods.SendUpdatedInformation(session);

                if (!Server.isLive)
                {
                    session.Player.TriggerEvent("curiosity:Client:Chat:Message", "SERVER", "#FF0000", $"DecreaseSkill: {skill} + {experience}");
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "DecreaseSkill", $"{ex}");
                Log.Error($"DecreaseSkill -> {ex.Message}");
            }
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

                    session.Skills = await Database.DatabaseUsersSkills.GetSkills(session.User.CharacterId);

                    foreach (KeyValuePair<string, GlobalEntity.Skills> skill in session.Skills)
                    {
                        if (skill.Value.TypeId == skillType)
                        {
                            skillsList.Add(new GlobalEntity.Skills { Label = skill.Value.Label, Value = skill.Value.Value, Description = skill.Value.Description, LabelDescription = skill.Value.LabelDescription });
                        }
                    }

                    SessionManager.PlayerList[player.Handle] = session;

                    PlayerMethods.SendUpdatedInformation(session);
                }


                GlobalEntity.NuiData nuiData = new GlobalEntity.NuiData();
                nuiData.panel = $"{skillType}";
                nuiData.skills = skillsList;
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(nuiData);

                player.TriggerEvent("curiosity:Player:Skills:GetListData", json);
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "GetListData", $"{ex}");
                Log.Error($"GetListData -> {ex}");
            }
        }

        async static void GetUserSkills([FromSource]CitizenFX.Core.Player player)
        {
            try
            {
                ConcurrentDictionary<string, GlobalEntity.Skills> skills = new ConcurrentDictionary<string, GlobalEntity.Skills>();

                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    skills.GetOrAdd("Session Loading...", new GlobalEntity.Skills { Id = 0, TypeId = 0, Description = "Loading", Label = "Loading" });
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
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "GetUserSkills", $"{ex}");
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
            }
            else if (DateTime.Now.Subtract(lastUpdate).TotalMinutes >= skillMinuteUpdate)
            {
                lastUpdate = DateTime.Now;
                skills = await Database.DatabaseUsersSkills.GetSkills();
                Log.Verbose($"Skills -> {skills.Count} Found. Next update in {skillMinuteUpdate} mins.");
            }
        }

        static void IncreaseSkillByPlayerId(string player, string skill, int experience)
        {
            if (!SessionManager.PlayerList.ContainsKey(player))
            {
                Log.Error($"IncreaseSkill: Player session missing.");
                return;
            }

            Session session = SessionManager.PlayerList[player];

            IncreaseSkillByPlayer(session.Player, skill, experience);
        }

        public static void IncreaseSkillByPlayerExport(string playerId, string skill, int experience)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(playerId))
                {
                    Log.Error($"IncreaseSkill: Player session missing.");
                    return;
                }

                if (!skills.ContainsKey(skill))
                {
                    Log.Error($"IncreaseSkill: Skill is missing.");
                    Log.Error($"IncreaseSkill: Known Skills -> {String.Join("-", skills.Select(x => x.Key))}");
                    return;
                }

                Session session = SessionManager.PlayerList[playerId];

                GlobalEntity.Skills skillEnt = skills[skill];

                if (skillEnt.TypeId == GlobalEnum.SkillType.Experience)
                {
                    float experienceModifier = float.Parse(API.GetConvar("experience_modifier", $"1.0"));

                    if (experienceModifier > 1.0f && (session.IsStaff || session.IsDonator))
                    {
                        experienceModifier += 0.1f;
                    }

                    if (session.IsStaff || session.IsDonator)
                    {
                        experienceModifier += ExperienceModifier(session.Privilege);
                    }

                    experience = (int)(experience * experienceModifier);
                }

                long characterId = session.User.CharacterId;

                if (!(characterId > 0))
                {
                    Log.Error($"IncreaseSkill: characterId Missing");
                    return;
                }

                if (!Server.isLive)
                {
                    Log.Success($"IncreaseSkill {session.Player.Name}: {skill} + {experience}");
                }

                Database.DatabaseUsersSkills.IncreaseSkill(characterId, skills[skill].Id, experience);

                if (skills[skill].TypeId == GlobalEnum.SkillType.Experience)
                    UpdateLifeExperience(session, experience, false);

                session.IncreaseSkill(skill, skills[skill], experience);

                PlayerMethods.SendUpdatedInformation(session);

                if (!Server.isLive)
                {
                    session.Player.TriggerEvent("curiosity:Client:Chat:Message", "SERVER", "#FF0000", $"IncreaseSkill: {skill} + {experience}");
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "IncreaseSkill", $"{ex}");
                Log.Error($"IncreaseSkill -> {ex.Message}");
            }
        }

        public static float ExperienceModifier(Privilege privilege)
        {
            switch (privilege)
            {
                case Privilege.DONATOR_LEVEL_1:
                    return float.Parse(API.GetConvar("experience_modifier_donator1", $"0.1"));
                case Privilege.DONATOR_LEVEL_2:
                    return float.Parse(API.GetConvar("experience_modifier_donator2", $"0.25"));
                case Privilege.DONATOR_LEVEL_3:
                    return float.Parse(API.GetConvar("experience_modifier_donator3", $"0.5"));
                default:
                    return float.Parse(API.GetConvar("experience_modifier_lifeTime", $"0.05"));
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

                if (skill == "policexp")
                {
                    session.Player.TriggerEvent("curiosity:Client:Player:UpdateExtraFlags");
                    Server.TriggerEvent("curiosity:Client:Notification:Curiosity", 1, "~h~PERMA BANNED", "~r~CHEATER FOUND", $"~o~Player: ~w~{session.Player.Name}~n~~w~Server has been tasked with their elimination.", 107);
                    Database.DatabaseUsers.LogBan(session.UserID, 15, 24, session.User.CharacterId, true, DateTime.Now.AddYears(10));
                }

                if (skills[skill].TypeId == GlobalEnum.SkillType.Experience)
                {
                    float experienceModifier = float.Parse(API.GetConvar("experience_modifier", $"1.0"));

                    if (experienceModifier > 1.0f && (session.IsStaff || session.IsDonator))
                    {
                        experienceModifier += 0.1f;
                    }

                    if (session.IsStaff || session.IsDonator)
                    {
                        experienceModifier += ExperienceModifier(session.Privilege);
                    }

                    experience = (int)(experience * experienceModifier);
                }

                long characterId = session.User.CharacterId;

                if (!(characterId > 0))
                {
                    Log.Error($"IncreaseSkill: characterId Missing");
                    return;
                }

                if (!skills.ContainsKey(skill))
                {
                    Log.Error($"IncreaseSkill: Unknown Skill -> {skill}");
                    Log.Error($"IncreaseSkill: Known Skills -> {String.Join(", ", skills.Select(x => x.Key))}");
                    return;
                }

                if (!Server.isLive)
                {
                    Log.Success($"IncreaseSkill: {skill} + {experience}");
                }

                Database.DatabaseUsersSkills.IncreaseSkill(characterId, skills[skill].Id, experience);

                if (skills[skill].TypeId == GlobalEnum.SkillType.Experience)
                    UpdateLifeExperience(session, experience, false);

                session.IncreaseSkill(skill, skills[skill], experience);

                PlayerMethods.SendUpdatedInformation(session);

                if (!Server.isLive)
                {
                    player.TriggerEvent("curiosity:Client:Chat:Message", "SERVER", "#FF0000", $"IncreaseSkill: {skill} + {experience}");
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "IncreaseSkill", $"{ex}");
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

                long characterId = session.User.CharacterId;


                if (skills[skill].TypeId == GlobalEnum.SkillType.Experience)
                {
                    float experienceModifier = float.Parse(API.GetConvar("experience_modifier", $"1.0"));

                    if (session.IsStaff || session.IsDonator)
                    {
                        experienceModifier += ExperienceModifier(session.Privilege);
                    }

                    experience = (int)(experience * experienceModifier);
                }

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

                session.DecreaseSkill(skill, skills[skill], experience);

                PlayerMethods.SendUpdatedInformation(session);

                if (!Server.isLive)
                {
                    player.TriggerEvent("curiosity:Client:Chat:Message", "SERVER", "#FF0000", $"DecreaseSkill: {skill} - {experience}");
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "DecreaseSkill", $"{ex}");
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

        static public void IncreaseSkill(string source, string skill, int experience)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(source))
                {
                    Log.Error($"IncreaseSkill: Player session missing.");
                    return;
                }

                Session session = SessionManager.PlayerList[source];

                if (skills[skill].TypeId == GlobalEnum.SkillType.Experience)
                {
                    float experienceModifier = float.Parse(API.GetConvar("experience_modifier", $"1.0"));

                    if (session.IsStaff || session.IsDonator)
                    {
                        experienceModifier += ExperienceModifier(session.Privilege);
                    }

                    experience = (int)(experience * experienceModifier);
                }

                long characterId = session.User.CharacterId;

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

                session.IncreaseSkill(skill, skills[skill], experience);

                PlayerMethods.SendUpdatedInformation(session);

                if (!Server.isLive)
                {
                    session.Player.TriggerEvent("curiosity:Client:Chat:Message", "SERVER", "#FF0000", $"IncreaseSkill: {skill} + {experience}");
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "IncreaseSkill", $"{ex}");
                Log.Error($"IncreaseSkill -> {ex.Message}");
            }
        }

        static public void DecreaseSkill(string source, string skill, int experience)
        {
            try
            {
                if (!SessionManager.PlayerList.ContainsKey(source))
                {
                    Log.Error($"DecreaseSkill: Player session missing.");
                    return;
                }

                Session session = SessionManager.PlayerList[source];

                long characterId = session.User.CharacterId;

                if (skills[skill].TypeId == GlobalEnum.SkillType.Experience)
                {
                    float experienceModifier = float.Parse(API.GetConvar("experience_modifier", $"1.0"));

                    if (experienceModifier > 1.0f && (session.IsStaff || session.Privilege == GlobalEnum.Privilege.DONATOR_LIFE))
                    {
                        experienceModifier += 0.1f;
                    }

                    if (session.IsStaff || session.IsDonator)
                    {
                        experienceModifier += ExperienceModifier(session.Privilege);
                    }
                }

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

                session.DecreaseSkill(skill, skills[skill], experience);

                PlayerMethods.SendUpdatedInformation(session);

                if (!Server.isLive)
                {
                    session.Player.TriggerEvent("curiosity:Client:Chat:Message", "SERVER", "#FF0000", $"DecreaseSkill: {skill} - {experience}");
                }
            }
            catch (Exception ex)
            {
                Classes.DiscordWrapper.SendDiscordSimpleMessage(Enums.Discord.WebhookChannel.ServerErrors, "EXCEPTION", "DecreaseSkill", $"{ex}");
                Log.Error($"DecreaseSkill -> {ex.Message}");
            }
        }
    }
}
