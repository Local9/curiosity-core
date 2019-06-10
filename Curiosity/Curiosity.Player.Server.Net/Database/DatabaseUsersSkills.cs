using CitizenFX.Core;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

using GlobalEntity = Curiosity.Global.Shared.net.Entity;

namespace Curiosity.Server.net.Database
{
    public class DatabaseUsersSkills : BaseScript
    {
        static MySQL mySql;

        public static void Init()
        {
            mySql = Database.mySQL;
        }

        public static async Task<Dictionary<string, GlobalEntity.Skills>> GetSkills()
        {
            Dictionary<string, GlobalEntity.Skills> skillsDictionary = new Dictionary<string, GlobalEntity.Skills>();

            string query = "select skillId, skillTypeId, description, label, labelDescription from curiosity.skill;";

            using (var result = mySql.QueryResult(query))
            {
                ResultSet keyValuePairs = await result;

                await Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    Log.Warn("SKILLS -> No skills found");
                    return skillsDictionary;
                }

                foreach(Dictionary<string, object> keyValues in keyValuePairs)
                {
                    GlobalEntity.Skills skills = new GlobalEntity.Skills
                    {
                        Id = int.Parse($"{keyValues["skillId"]}"),
                        TypeId = int.Parse($"{keyValues["skillTypeId"]}"),
                        Description = $"{keyValues["description"]}",
                        Label = $"{keyValues["label"]}",
                        LabelDescription = $"{keyValues["labelDescription"]}"
                    };
                    skillsDictionary.Add($"{keyValues["description"]}", skills);
                }

                return skillsDictionary;
            }
        }

        public static async Task<Dictionary<string, GlobalEntity.Skills>> GetSkills(int characterId)
        {
            Dictionary<string, GlobalEntity.Skills> skillsDictionary = new Dictionary<string, GlobalEntity.Skills>();

            string query = "select skill.skillId, skill.skillTypeId, skill.description, skill.label, skill.labelDescription, character_skill.experience from curiosity.character_skill inner join skill on character_skill.skillId = skill.skillId where character_skill.characterId = @characterId order by skill.skillTypeId;";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@characterId", characterId);

            using (var result = mySql.QueryResult(query, myParams))
            {
                ResultSet keyValuePairs = await result;

                await Delay(0);

                if (keyValuePairs.Count == 0)
                {
                    // Log.Warn($"SKILLS -> No skills found for user {userId}, possible they are new.");
                    return new Dictionary<string, GlobalEntity.Skills>();
                }

                foreach (Dictionary<string, object> keyValues in keyValuePairs)
                {
                    GlobalEntity.Skills skills = new GlobalEntity.Skills
                    {
                        Id = int.Parse($"{keyValues["skillId"]}"),
                        TypeId = int.Parse($"{keyValues["skillTypeId"]}"),
                        Description = $"{keyValues["description"]}",
                        Label = $"{keyValues["label"]}",
                        LabelDescription = $"{keyValues["labelDescription"]}",
                        Value = int.Parse($"{keyValues["experience"]}")
                    };
                    skillsDictionary.Add($"{keyValues["description"]}", skills);
                }

                return skillsDictionary;
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
