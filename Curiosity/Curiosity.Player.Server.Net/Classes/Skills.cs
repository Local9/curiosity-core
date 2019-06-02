using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using GlobalEntity = Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Server.net.Classes
{
    class Skills
    {
        static Dictionary<string, GlobalEntity.Skills> skills = new Dictionary<string, GlobalEntity.Skills>();

        public static void Init()
        {
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Skills:Increase", new Action<Player, string, int>(IncreaseSkillByPlayer));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Skills:Decrease", new Action<Player, string, int>(DecreaseSkillByPlayer));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Skills:Get", new Action<Player>(GetUserSkills));

            Server.GetInstance().RegisterTickHandler(UpdateSkillsDictionary);
        }

        async static void GetUserSkills([FromSource]Player player)
        {
            Dictionary<string, GlobalEntity.Skills> skills = new Dictionary<string, GlobalEntity.Skills>();
            if (!SessionManager.PlayerList.ContainsKey(player.Handle))
            {
                skills.Add("Session Loading...", new GlobalEntity.Skills { Id = 0, TypeId = 0, Description = "Loading", Label = "Loading" });
            }
            else
            {
                skills = await Database.DatabaseUsersSkills.GetSkills(SessionManager.PlayerList[player.Handle].User.CharacterId);
            }
            player.TriggerEvent("curiosity:Player:Skills:Get", Newtonsoft.Json.JsonConvert.SerializeObject(skills));
        }

        async static Task UpdateSkillsDictionary()
        {
            await BaseScript.Delay(0);
            if (Server.isLive)
            {
                while (Server.serverId == 0)
                {
                    await BaseScript.Delay(0);
                }
                skills = await Database.DatabaseUsersSkills.GetSkills();

                if (skills.Count > 0)
                {
                    Log.Success($"{skills.Count} SKILLS CONFIGURED");
                }
                await Task.FromResult(0);
            }
            else
            {
                while (true)
                {
                    while (Server.serverId == 0)
                    {
                        await BaseScript.Delay(0);
                    }
                    skills = await Database.DatabaseUsersSkills.GetSkills();

                    if (skills.Count > 0)
                    {
                        Log.Success($"{skills.Count} SKILLS CONFIGURED");
                    }
                    Log.Verbose("Next skill update in 5 minutes");
                    await BaseScript.Delay((1000 * 60) * 5);
                }
            }
        }

        static void IncreaseSkillByPlayer([FromSource]Player player, string skill, int experience)
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

                Database.DatabaseUsersSkills.IncreaseSkill(characterId, skills[skill].Id, experience);

                if (skills[skill].TypeId == 1)
                    UpdateLifeExperience(session, experience, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseSkill -> {ex.Message}");
            }
        }

        static void DecreaseSkillByPlayer([FromSource]Player player, string skill, int experience)
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
                Database.DatabaseUsersSkills.DecreaseSkill(characterId, skills[skill].Id, experience);

                if (skills[skill].TypeId == 1)
                    UpdateLifeExperience(session, experience, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseSkill -> {ex.Message}");
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
