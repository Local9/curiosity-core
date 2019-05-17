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
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Skills:IncreaseByUserId", new Action<long, string, int>(IncreaseSkill));
            Server.GetInstance().RegisterEventHandler("curiosity:Server:Skills:DecreaseByUserId", new Action<long, string, int>(DecreaseSkill));
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
                skills = await Database.DatabaseUsersSkills.GetSkills(SessionManager.PlayerList[player.Handle].UserID);
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
                Server.WriteConsoleLine($"{skills.Count} SKILLS CONFIGURED", true);
                Console.WriteLine();
            }
        }

        static void IncreaseSkill(long userId, string skill, int experience)
        {
            try
            {
                if (!(userId > 0))
                {
                    Debug.WriteLine($"IncreaseSkill: UserID Missing");
                    return;
                }

                if (!skills.ContainsKey(skill))
                {
                    Debug.WriteLine($"IncreaseSkill: Unknown Skill -> {skill}");
                    Debug.WriteLine($"IncreaseSkill: Known Skills -> {String.Join("-", skills.Select(x => x.Key))}");
                    return;
                }
                Database.DatabaseUsersSkills.IncreaseSkill(userId, skills[skill], experience);
                UpdateWorldSkill(userId, experience, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseSkill -> {ex.Message}");
            }
        }

        static void IncreaseSkillByPlayer([FromSource]Player player, string skill, int experience)
        {
            try
            {
                long userId = 0;
                
                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    Debug.WriteLine($"IncreaseSkill: Player session missing.");
                    return;
                }

                userId = SessionManager.PlayerList[player.Handle].UserID;

                if (!(userId > 0))
                {
                    Debug.WriteLine($"IncreaseSkill: UserID Missing");
                    return;
                }

                if (!skills.ContainsKey(skill))
                {
                    Debug.WriteLine($"IncreaseSkill: Unknown Skill -> {skill}");
                    Debug.WriteLine($"IncreaseSkill: Known Skills -> {String.Join("-", skills.Select(x => x.Key))}");
                    return;
                }
                Database.DatabaseUsersSkills.IncreaseSkill(userId, skills[skill], experience);
                UpdateWorldSkill(userId, experience, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseSkill -> {ex.Message}");
            }
        }

        static void DecreaseSkill(long userId, string skill, int experience)
        {
            try
            {
                if (!(userId > 0))
                {
                    Debug.WriteLine($"DecreaseSkill: UserID Missing");
                    return;
                }

                if (!skills.ContainsKey(skill))
                {
                    Debug.WriteLine($"DecreaseSkill: Unknown Skill -> {skill}");
                    Debug.WriteLine($"IncreaseSkill: Known Skills -> {String.Join("-", skills.Select(x => x.Key))}");
                    return;
                }
                Database.DatabaseUsersSkills.DecreaseSkill(userId, skills[skill], experience);
                UpdateWorldSkill(userId, experience, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DecreaseSkill -> {ex.Message}");
            }
        }

        static void DecreaseSkillByPlayer([FromSource]Player player, string skill, int experience)
        {
            try
            {
                long userId = 0;

                if (!SessionManager.PlayerList.ContainsKey(player.Handle))
                {
                    Debug.WriteLine($"DecreaseSkill: Player session missing.");
                    return;
                }

                userId = SessionManager.PlayerList[player.Handle].UserID;

                if (!(userId > 0))
                {
                    Debug.WriteLine($"DecreaseSkill: UserID Missing");
                    return;
                }

                if (!skills.ContainsKey(skill))
                {
                    Debug.WriteLine($"DecreaseSkill: Unknown Skill -> {skill}");
                    Debug.WriteLine($"DecreaseSkill: Known Skills -> {String.Join("-", skills.Select(x => x.Key))}");
                    return;
                }
                Database.DatabaseUsersSkills.DecreaseSkill(userId, skills[skill], experience);
                UpdateWorldSkill(userId, experience, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseSkill -> {ex.Message}");
            }
        }

        static void UpdateWorldSkill(long userId, int experience, bool removeXp)
        {
            string eventToTrigger;
            if (removeXp)
            {
                Database.DatabaseUsersSkills.DecreaseSkill(userId, skills[WORLD_SKILL], experience);
                eventToTrigger = "curiosity:Client:Rank:RemovePlayerXP";
            }
            else
            {
                Database.DatabaseUsersSkills.IncreaseSkill(userId, skills[WORLD_SKILL], experience);
                eventToTrigger = "curiosity:Client:Rank:AddPlayerXP";
            }
            Player player = SessionManager.GetPlayer(userId);
            player.TriggerEvent(eventToTrigger, experience);
        }
    }
}
