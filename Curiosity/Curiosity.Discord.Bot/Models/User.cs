using Curiosity.Discord.Bot.Database;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Curiosity.Discord.Bot.Models
{
    public class User
    {
        public long UserId;
        public string Username;
        public string License;
        public long LifeExperience;
        public DateTime DateCreated;
        public DateTime LastSeen;
        public bool BannedPerm;
        public DateTime? BannedUntil;

        internal AppDb Db { get; set; }

        public User() { }

        internal User(AppDb db)
        {
            Db = db;
        }

        public async Task<User> FindUserAsync(ulong discordId)
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"call selUserByDiscordId(@discordId);";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@discordId",
                DbType = DbType.Int64,
                Value = discordId
            });
            var result = await ReadAllAsync(await cmd.ExecuteReaderAsync());
            return result.Count > 0 ? result[0] : null;
        }

        private async Task<List<User>> ReadAllAsync(DbDataReader reader)
        {
            var users = new List<User>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var user = new User(Db)
                    {
                        UserId = reader.GetFieldValue<long>("userId"),
                        Username = reader.GetFieldValue<string>("username"),
                        License = reader.GetFieldValue<string>("license"),
                        LifeExperience = reader.GetFieldValue<long>("lifeExperience"),
                        DateCreated = reader.GetFieldValue<DateTime>("dateCreated"),
                        LastSeen = reader.GetFieldValue<DateTime>("lastSeen"),
                        BannedPerm = reader.GetFieldValue<bool>("bannedPerm"),
                        BannedUntil = reader.GetFieldValue<DateTime>("bannedUntil"),
                    };
                    users.Add(user);
                }
            }
            return users;
        }
    }
}
