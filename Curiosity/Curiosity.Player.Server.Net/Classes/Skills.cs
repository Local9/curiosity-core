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
            EventHandlers["curiosity:Server:Skills:Increase"] += new Action<Player, string, int>(IncreasePlayerSkill);
            EventHandlers["curiosity:Server:Skills:Decrease"] += new Action<Player, string, int>(DecreasePlayerSkill);
            EventHandlers["curiosity:Server:Skills:IncreaseInternal"] += new Action<int, string, int>(IncreaseSkill);
            EventHandlers["curiosity:Server:Skills:DecreaseInternal"] += new Action<int, string, int>(DecreaseSkill);

            Tick += UpdateSkillsDictionary;
        }

        async void IncreasePlayerSkill([FromSource]Player player, string skill, int experience)
        {
            await Delay(0);
            int UserID = SessionManager.PlayerList[player.Handle].UserID;
            IncreaseSkill(UserID, skill, experience);
        }

        async void DecreasePlayerSkill([FromSource]Player player, string skill, int experience)
        {
            await Delay(0);
            int UserID = SessionManager.PlayerList[player.Handle].UserID;
            DecreaseSkill(UserID, skill, experience);
        }

        async Task UpdateSkillsDictionary()
        {
            while (skills == null)
            {
                await Delay(0);
                skills = await databaseSkills.GetSkills();
            }
        }

        void IncreaseSkill(int userId, string skill, int experience)
        {
            if (!skills.ContainsKey(skill))
            {
                Debug.WriteLine($"IncreaseSkill: Unknown Skill -> {skill}");
                return;
            }
            databaseSkills.IncreaseSkill(userId, skills[skill], experience);
        }

        void DecreaseSkill(int userId, string skill, int experience)
        {
            if (!skills.ContainsKey(skill))
            {
                Debug.WriteLine($"DecreaseSkill: Unknown Skill -> {skill}");
                return;
            }
            databaseSkills.DecreaseSkill(userId, skills[skill], experience);
        }

        void UpdateWorldSkill(int userId, int experience, bool removeXp)
        {
            string eventToTrigger;
            if (removeXp)
            {
                databaseSkills.DecreaseSkill(userId, skills[WORLD_SKILL], experience);
                eventToTrigger = "curiosity:Client:Rank:AddPlayerXP";
            }
            else
            {
                databaseSkills.IncreaseSkill(userId, skills[WORLD_SKILL], experience);
                eventToTrigger = "curiosity:Client:Rank:RemovePlayerXP";
            }
            Player player = SessionManager.GetPlayer(userId);
            player.TriggerEvent(eventToTrigger, experience);
        }
    }
}
