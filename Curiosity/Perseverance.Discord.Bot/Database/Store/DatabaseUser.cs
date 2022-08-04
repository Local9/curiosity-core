using Dapper;
using Perseverance.Discord.Bot.Entities.Enums;
using Perseverance.Discord.Bot.Extensions;
using System.ComponentModel;

namespace Perseverance.Discord.Bot.Database.Store
{
    internal class DatabaseUser
    {
        const string SQL_USER_GET = "select * from curiosity.user u where u.discordId = @pDiscordId;";
        const string SQL_USER_GET_DONATORS = "select * from curiosity.user u where u.roleId in (9, 11, 12, 13);";

        [Description("userId")]
        public int UserID { get; private set; }
        
        [Description("username")]
        public string Username { get; private set; }

        [Description("discordId")]
        public ulong DiscordId { get; private set; }

        [Description("dateCreated")]
        public DateTime DateCreated { get; private set; }

        [Description("lastSeen")]
        public DateTime LastSeen { get; private set; }

        [Description("roleId")]
        public Role Role { get; private set; }

        internal static async Task<DatabaseUser> GetAsync(ulong discordId)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("pDiscordId", discordId);
            DatabaseUser user = await DapperDatabase<DatabaseUser>.GetSingleAsync(SQL_USER_GET, dynamicParameters);
            return user;
        }

        internal static async Task<List<DatabaseUser>> GetDonatorsAsync()
        {
            return await DapperDatabase<DatabaseUser>.GetListAsync(SQL_USER_GET_DONATORS);
        }

        public override string ToString()
        {
            return $"User: {Username}\nJoined: {DateCreated:yyyy-MM-dd HH:mm}\nLast Seen: {LastSeen:yyyy-MM-dd HH:mm}\nRole: {Role.GetDescription()}";
        }
    }
}
