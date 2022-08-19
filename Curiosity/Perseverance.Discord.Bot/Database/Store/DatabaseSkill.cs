using Dapper;
using System.ComponentModel;

namespace Perseverance.Discord.Bot.Database.Store
{
    internal class DatabaseSkill
    {
        const string SQL_GET_TOP = "select Top from curiosity.cur_skill where Top is not null;";
        
        const string SQL_GET_TOP_PLAYERS = "select u.username as Key, cs.Value " +
            "from curiosity.user u " +
            "inner join curiosity.character c on u.userId = c.userId " +
            "inner join curiosity.cur_skill_character cs on c.characterId = cs.characterId " +
            "inner join curiosity.cur_skill s on cs.skillId = s.skillId " +
            "where " +
            "u.bannedPerm = 0 " +
            "and u.userId not in (14, 15) " +
            "and s.top = pSkill " +
            "and c.serverId = 5 " +
            "-- and datediff(NOW(), u.lastSeen) <= 30 " +
            "order by cs.Value desc " +
            "LIMIT 10;";

        #region Fields
        [Description("SkillId")]
        public int SkillId { get; set; }
        
        [Description("Label")]
        public string Label { get; set; }
        
        [Description("Description")]
        public string Description { get; set; }
        
        [Description("Top")]
        public string Top { get; set; }
        #endregion

        internal static async Task<List<string>> GetSkillsTopAsync()
        {
            List<string> result = await DapperDatabase<string>.GetListAsync(SQL_GET_TOP);
            return result;
        }

        internal static async Task<Dictionary<string, string>> GetSkillsTopPlayers(string skill)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("pSkill", skill);
            Dictionary<string, string> result = await DapperDatabase<Dictionary<string, string>>.GetDictionaryAsync(SQL_GET_TOP_PLAYERS, dynamicParameters);
            if (result == null)
                return new Dictionary<string, string>();
            return result;
        }
    }
}
