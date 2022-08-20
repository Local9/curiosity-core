using Dapper;
using System.ComponentModel;

namespace Perseverance.Discord.Bot.Database.Store
{
    internal class DatabaseSkill
    {
        const string SQL_GET_TOP = "select Top from curiosity.cur_skill where Top is not null;";
        
        const string SQL_GET_TOP_PLAYERS = "call selServerTopPlayer(@pServerId, @pSkillTop);";

        #region Fields
        [Description("SkillId")]
        public int SkillId { get; set; }
        
        [Description("Label")]
        public string Label { get; set; }
        
        [Description("Description")]
        public string Description { get; set; }
        
        [Description("Top")]
        public string Top { get; set; }

        #region TOP
        [Description("Username")]
        public string Username { get; set; }

        [Description("Experience")]
        public ulong Experience { get; set; }
        #endregion
        #endregion

        internal static async Task<List<string>> GetSkillsTopAsync()
        {
            List<string> result = await DapperDatabase<string>.GetListAsync(SQL_GET_TOP);
            return result;
        }

        internal static async Task<IEnumerable<DatabaseSkill>> GetSkillsTopPlayers(int serverId, string skill)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("pServerId", serverId);
            dynamicParameters.Add("pSkillTop", skill);
            IEnumerable<DatabaseSkill> result = await DapperDatabase<DatabaseSkill>.GetListAsync(SQL_GET_TOP_PLAYERS, dynamicParameters);
            if (result == null)
                return Enumerable.Empty<DatabaseSkill>();
            return result;
        }
    }
}
