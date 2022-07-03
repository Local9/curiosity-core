#if CLIENT
using Curiosity.Framework.Client.Engine;
#endif

#if SERVER
using Curiosity.Framework.Server;
using Curiosity.Framework.Server.Database;
using Dapper;
#endif

using System.ComponentModel;

namespace Curiosity.Framework.Shared.Models
{
    public class User
    {
#region FIELDS
        [JsonIgnore]
        [Description("userId")]
        public int UserID { get; private set; }

        [Description("username")]
        public string Username { get; private set; }

        [JsonIgnore]
        [Description("license")]
        public string License { get; private set; }

        [JsonIgnore]
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

        public List<Character> Characters { get; private set; } = new();

#if CLIENT
        internal SoundEngine Sound => new SoundEngine();
#endif

#if SERVER
        const string SQL_GET_USER = "call spGetUser(@pUsername, @pDiscordId);";
        const string SQL_GET_CHARACTERS = "call spGetCharacter(@pDiscordID, @pServerID);";

        public async Task<User> GetUserAsync(string username, string discordId, bool withCharacters = false)
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("pUsername", username);
            dynamicParameters.Add("pDiscordId", discordId);
            User user = await DapperDatabase<User>.GetSingleAsync(SQL_GET_USER, dynamicParameters);
            
            await Common.MoveToMainThread();

            if (withCharacters)
                await GetCharactersAsync();

            return user;
        }

        public async Task GetCharactersAsync()
        {
            DynamicParameters dynamicParameters = new DynamicParameters();
            dynamicParameters.Add("pDiscordId", DiscordId);
            dynamicParameters.Add("pServerID", PluginManager.ServerID);
            Characters = await DapperDatabase<Character>.GetListAsync(SQL_GET_CHARACTERS, dynamicParameters);
            await Common.MoveToMainThread();
        }
#endif
    }
}
