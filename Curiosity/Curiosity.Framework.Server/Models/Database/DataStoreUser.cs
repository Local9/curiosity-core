using Curiosity.Framework.Server.Database;
using Curiosity.Framework.Shared;
using Dapper;

using System.ComponentModel;

namespace Curiosity.Framework.Server.Models.Database
{
    public partial class DataStoreUser
    {
        #region FIELDS
        [Description("userId")]
        public int UserID { get; private set; }

        [Description("username")]
        public string Username { get; private set; }

        [Description("license")]
        public string License { get; private set; }

        [Description("discordId")]
        public ulong DiscordId { get; private set; }

        [Description("dateCreated")]
        public DateTime DateCreated { get; private set; }

        [Description("lastSeen")]
        public DateTime LastSeen { get; private set; }

        [Description("bannedPerm")]
        public int BannedPermanently { get; private set; }

        [Description("roleId")]
        public int RoleId { get; private set; }

        [Description("isPassive")]
        public int IsPassive { get; private set; }

        #endregion

        public List<DataStoreCharacter> Characters { get; private set; } = new();
        public DataStoreCharacter CurrentCharacter { get; private set; }

        const string SQL_USER_GET = "select * from curiosity.user u where u.discordId = @pDiscordId;";
        const string SQL_USER_INSERT = "insert into curiosity.user (username, license, lastSeen, discordId, roleId, IsPassive) values (@pUsername, @pDiscordId, CURRENT_TIMESTAMP, @pDiscordId, 1, 1);";
        const string SQL_CHARACTERS_GET = "select * from curiosity.character c where c.UserID = @pUserId and c.ServerId = @pServerId;";
        const string SQL_CHARACTER_GET = "select * from curiosity.character c where c.UserID = @pUserId and c.ServerId = @pServerId and c.characterId = @pCharacterId;";

        internal static async Task<DataStoreUser> GetUserAsync(string playerName, ulong discordId, bool withCharacters = false)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("pDiscordId", discordId);
            DataStoreUser user = await DapperDatabase<DataStoreUser>.GetSingleAsync(SQL_USER_GET, dynamicParameters);

            await Common.MoveToMainThread();

            if (user is null)
            {
                dynamicParameters.Add("pUsername", playerName);
                bool success = await DapperDatabase<DataStoreUser>.ExecuteAsync(SQL_USER_INSERT, dynamicParameters);

                await Common.MoveToMainThread();

                if (success)
                {
                    user = await DapperDatabase<DataStoreUser>.GetSingleAsync(SQL_USER_GET, dynamicParameters);
                    
                    await Common.MoveToMainThread();
                }
            }

            if (withCharacters)
                await user.GetCharactersAsync();

            user.Username = playerName;

            return user;
        }

        internal async Task GetCharactersAsync()
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("pUserId", UserID);
            dynamicParameters.Add("pServerId", PluginManager.ServerID);
            var _characters = await DapperDatabase<DataStoreCharacter>.GetListAsync(SQL_CHARACTERS_GET, dynamicParameters);
            await Common.MoveToMainThread();
            Characters = _characters;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
