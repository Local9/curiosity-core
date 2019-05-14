using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace Curiosity.Server.net.Classes
{
    public class Skills : BaseScript
    {
        Database.DatabaseSkills databaseSkills = Database.DatabaseSkills.GetInstance();

        Dictionary<string, int> skills = new Dictionary<string, int>();

        const string WORLD_SKILL = "world";

        public Skills()
        {
            EventHandlers["curiosity:Server:Skills:Increase"] += new Action<Player, string, int>(IncreaseSkillByPlayer);
            EventHandlers["curiosity:Server:Skills:Decrease"] += new Action<Player, string, int>(DecreaseSkillByPlayer);
            EventHandlers["curiosity:Server:Skills:IncreaseByUserId"] += new Action<long, string, int>(IncreaseSkill);
            EventHandlers["curiosity:Server:Skills:DecreaseByUserId"] += new Action<long, string, int>(DecreaseSkill);

            EventHandlers["curiosity:Server:Skills:Get"] += new Action<Player>(GetUserSkills);

            Tick += UpdateSkillsDictionary;
        }

        async void GetUserSkills([FromSource]Player player)
        {
            Dictionary<string, int> skills = new Dictionary<string, int>();
            if (!SessionManager.PlayerList.ContainsKey(player.Handle))
            {
                skills.Add("Session Loading...", 0);
            }
            else
            {
                skills = await databaseSkills.GetSkills(SessionManager.PlayerList[player.Handle].UserID);
            }
            player.TriggerEvent("curiosity:Player:Skills:Get", Newtonsoft.Json.JsonConvert.SerializeObject(skills));
        }

        async Task UpdateSkillsDictionary()
        {
            while (skills.Count == 0)
            {
                while (Server.serverId == 0)
                {
                    await Delay(0);
                }
                await Delay(0);
                skills = await databaseSkills.GetSkills();
                Server.WriteConsoleLine($"{skills.Count} SKILLS CONFIGURED", true);
                Console.WriteLine();
            }
        }

        void IncreaseSkill(long userId, string skill, int experience)
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
                databaseSkills.IncreaseSkill(userId, skills[skill], experience);
                UpdateWorldSkill(userId, experience, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseSkill -> {ex.Message}");
            }
        }

        void IncreaseSkillByPlayer([FromSource]Player player, string skill, int experience)
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
                databaseSkills.IncreaseSkill(userId, skills[skill], experience);
                UpdateWorldSkill(userId, experience, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseSkill -> {ex.Message}");
            }
        }

        void DecreaseSkill(long userId, string skill, int experience)
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
                databaseSkills.DecreaseSkill(userId, skills[skill], experience);
                UpdateWorldSkill(userId, experience, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DecreaseSkill -> {ex.Message}");
            }
        }

        void DecreaseSkillByPlayer([FromSource]Player player, string skill, int experience)
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
                databaseSkills.DecreaseSkill(userId, skills[skill], experience);
                UpdateWorldSkill(userId, experience, false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"IncreaseSkill -> {ex.Message}");
            }
        }

        void UpdateWorldSkill(long userId, int experience, bool removeXp)
        {
            string eventToTrigger;
            if (removeXp)
            {
                databaseSkills.DecreaseSkill(userId, skills[WORLD_SKILL], experience);
                eventToTrigger = "curiosity:Client:Rank:RemovePlayerXP";
            }
            else
            {
                databaseSkills.IncreaseSkill(userId, skills[WORLD_SKILL], experience);
                eventToTrigger = "curiosity:Client:Rank:AddPlayerXP";
            }
            Player player = SessionManager.GetPlayer(userId);
            player.TriggerEvent(eventToTrigger, experience);
        }
    }
}
