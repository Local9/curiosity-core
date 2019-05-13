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
            EventHandlers["curiosity:Server:Skills:Increase"] += new Action<long, string, int>(IncreaseSkill);
            EventHandlers["curiosity:Server:Skills:Decrease"] += new Action<long, string, int>(DecreaseSkill);

            Tick += UpdateSkillsDictionary;
        }

        async Task UpdateSkillsDictionary()
        {
            while (skills.Count == 0)
            {
                await Delay(0);
                skills = await databaseSkills.GetSkills();
            }
        }

        void IncreaseSkill(long userId, string skill, int experience)
        {
            try
            {
                if (!skills.ContainsKey(skill))
                {
                    Debug.WriteLine($"IncreaseSkill: Unknown Skill -> {skill}");
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
                if (!skills.ContainsKey(skill))
                {
                    Debug.WriteLine($"DecreaseSkill: Unknown Skill -> {skill}");
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
