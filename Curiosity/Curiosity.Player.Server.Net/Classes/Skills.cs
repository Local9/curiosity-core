using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes
{
    public class Skills
    {
        static Dictionary<string, int> skills = new Dictionary<string, int>();

        const string WORLD_SKILL = "world";

        public static void Init()
        {
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Skills:Increase", new Action<Player, string, int>(IncreaseSkillByPlayer));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Skills:Decrease", new Action<Player, string, int>(DecreaseSkillByPlayer));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Skills:Get", new Action<Player>(GetUserSkills));

            Server.GetInstance().RegisterTickHandler(UpdateSkillsDictionary);
        }

        async static void GetUserSkills([FromSource]Player player)
        {
            Dictionary<string, int> skills = new Dictionary<string, int>();
            if (!SessionManager.PlayerList.ContainsKey(player.Handle))
            {
                skills.Add("Session Loading...", 0);
            }
            else
            {
                skills = await Database.DatabaseUsersSkills.GetSkills(SessionManager.PlayerList[player.Handle].User.CharacterId);
            }
            player.TriggerEvent("curiosity:Player:Skills:Get", Newtonsoft.Json.JsonConvert.SerializeObject(skills));
        }

        async static Task UpdateSkillsDictionary()
        {
            while (skills.Count == 0)
            {
                while (Server.serverId == 0)
                {
                    await BaseScript.Delay(0);
                }
                await BaseScript.Delay(0);
                skills = await Database.DatabaseUsersSkills.GetSkills();

                if (skills.Count > 0)
                {
                    Log.Success($"{skills.Count} SKILLS CONFIGURED");
                }
                await BaseScript.Delay(10000);
            }
        }

        static void IncreaseSkillByPlayer([FromSource]Player player, string skill, int experience)
        {
            try
            {
                long characterId = 0;
                
                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    Log.Error($"IncreaseSkill: Player session missing.");
                    return;
                }

                characterId = SessionManager.PlayerList[player.Handle].User.CharacterId;
                int userId = SessionManager.PlayerList[player.Handle].UserID;

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
                Database.DatabaseUsersSkills.IncreaseSkill(characterId, skills[skill], experience);
                UpdateLifeExperience(userId, experience, false);
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
                long characterId = 0;

                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    Log.Error($"DecreaseSkill: Player session missing.");
                    return;
                }

                characterId = SessionManager.PlayerList[player.Handle].User.CharacterId;
                int userId = SessionManager.PlayerList[player.Handle].UserID;

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
                Database.DatabaseUsersSkills.DecreaseSkill(characterId, skills[skill], experience);
                UpdateLifeExperience(userId, experience, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseSkill -> {ex.Message}");
            }
        }

        static void UpdateLifeExperience(long userId, int experience, bool removeXp)
        {
            string eventToTrigger;
            if (removeXp)
            {
                Database.DatabaseUsers.DecreaseLiveExperience(userId, experience);
                eventToTrigger = "curiosity:Client:Rank:RemovePlayerXP";
            }
            else
            {
                Database.DatabaseUsers.IncreaseLiveExperience(userId, experience);
                eventToTrigger = "curiosity:Client:Rank:AddPlayerXP";
            }
            Player player = SessionManager.GetPlayer(userId);
            player.TriggerEvent(eventToTrigger, experience);
        }
    }
}
