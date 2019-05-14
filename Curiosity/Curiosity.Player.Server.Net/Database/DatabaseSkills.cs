using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CitizenFX.Core;

namespace Curiosity.Server.net.Database
{
    public class DatabaseSkills : BaseScript
    {
        MySQL mySql;

        static DatabaseSkills databaseSkills;

        public static DatabaseSkills GetInstance()
        {
            return databaseSkills;
        }

        public DatabaseSkills()
        {
            mySql = DatabaseSettings.GetInstance().mySQL;
            databaseSkills = this;
        }

        public async Task<Dictionary<string, int>> GetSkills()
        {
            Dictionary<string, int> skills = new Dictionary<string, int>();

            string query = "select skillId, description from skills where serverId = @serverId or serverId is null;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@serverId", Server.serverId);

            using (var result = mySql.QueryResult(query, myParams))
            {
                ResultSet keyValuePairs = await result;

                await Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    Debug.WriteLine("SKILLS -> No skills found");
                    return null;
                }

                foreach(Dictionary<string, object> keyValues in keyValuePairs)
                {
                    skills.Add($"{keyValues["description"]}", int.Parse($"{keyValues["skillId"]}"));
                }

                return skills;
            }
        }

        public async Task<Dictionary<string, int>> GetSkills(long userId)
        {
            Dictionary<string, int> skills = new Dictionary<string, int>();

            string query = "select description, experience from userskills inner join skills on userskills.skillId = skills.skillId where (serverId = @serverId or serverId is null) and userskills.userId = @userId;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@serverId", Server.serverId);
            myParams.Add("@userId", userId);

            using (var result = mySql.QueryResult(query, myParams))
            {
                ResultSet keyValuePairs = await result;

                await Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    Debug.WriteLine("SKILLS -> No skills found");
                    return null;
                }

                foreach (Dictionary<string, object> keyValues in keyValuePairs)
                {
                    skills.Add($"{keyValues["description"]}", int.Parse($"{keyValues["experience"]}"));
                }

                return skills;
            }
        }

        public void IncreaseSkill(long userId, int skillId, int experience)
        {
            string query = "INSERT INTO userskills (`userId`,`skillId`,`experience`)" +
                " VALUES (@userId, @skillId, @experience)" +
                " ON DUPLICATE KEY UPDATE `experience` = `experience` + @experience;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@skillId", skillId);
            myParams.Add("@experience", experience);

            mySql.Query(query, myParams);
        }

        public void DecreaseSkill(long userId, int skillId, int experience)
        {
            string query = "INSERT INTO userskills (`userId`,`skillId`,`experience`)" +
                " VALUES (@userId, @skillId, @experience)" +
                " ON DUPLICATE KEY UPDATE `experience` = `experience` - @experience;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@userId", userId);
            myParams.Add("@skillId", skillId);
            myParams.Add("@experience", experience);

            mySql.Query(query, myParams);
        }
    }
}
