using CitizenFX.Core;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Database
{
    public class DatabaseUsersSkills : BaseScript
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static async Task<Dictionary<string, int>> GetSkills()
        {
            Dictionary<string, int> skills = new Dictionary<string, int>();

            string query = "select skillId, description from curiosity.skill;";

            using (var result = mySql.QueryResult(query))
            {
                ResultSet keyValuePairs = await result;

                await Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    Log.Warn("SKILLS -> No skills found");
                    return skills;
                }

                foreach(Dictionary<string, object> keyValues in keyValuePairs)
                {
                    skills.Add($"{keyValues["description"]}", int.Parse($"{keyValues["skillId"]}"));
                }

                return skills;
            }
        }

        public static async Task<Dictionary<string, int>> GetSkills(int characterId)
        {
            Dictionary<string, int> skills = new Dictionary<string, int>();

            string query = "select description, experience from curiosity.character_skill inner join skill on character_skill.skillId = skill.skillId where character_skill.characterId = @characterId;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterId", characterId);

            using (var result = mySql.QueryResult(query, myParams))
            {
                ResultSet keyValuePairs = await result;

                await Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    // Debug.WriteLine($"SKILLS -> No skills found for user {userId}, possible they are new.");
                    return null;
                }

                foreach (Dictionary<string, object> keyValues in keyValuePairs)
                {
                    skills.Add($"{keyValues["description"]}", int.Parse($"{keyValues["experience"]}"));
                }

                return skills;
            }
        }

        public static void IncreaseSkill(long characterId, int skillId, int experience)
        {
            string query = "INSERT INTO curiosity.character_skill (`characterId`,`skillId`,`experience`)" +
                " VALUES (@characterId, @skillId, @experience)" +
                " ON DUPLICATE KEY UPDATE `experience` = `experience` + @experience;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterId", characterId);
            myParams.Add("@skillId", skillId);
            myParams.Add("@experience", experience);

            mySql.Query(query, myParams);
        }

        public static void DecreaseSkill(long characterId, int skillId, int experience)
        {
            string query = "INSERT INTO curiosity.character_skill (`characterId`,`skillId`,`experience`)" +
                " VALUES (@characterId, @skillId, @experience)" +
                " ON DUPLICATE KEY UPDATE `experience` = `experience` - @experience;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterId", characterId);
            myParams.Add("@skillId", skillId);
            myParams.Add("@experience", experience);

            mySql.Query(query, myParams);
        }
    }
}
