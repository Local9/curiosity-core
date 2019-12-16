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

        public User() { }

        public async Task<User> FindUserAsync(ulong discordId)
        {
            try
            {
                using var connection = await Database.DatabaseConfig.GetDatabaseConnection();
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();

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
            catch (Exception ex)
            {
                Console.WriteLine($"FindUserAsync -> {ex}");
                return default;
            }
        }

        private async Task<List<User>> ReadAllAsync(DbDataReader reader)
        {
            var users = new List<User>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var user = new User()
                    {
                        UserId = reader.GetFieldValue<long>("userId"),
                        Username = reader.GetFieldValue<string>("username"),
                        License = reader.GetFieldValue<string>("license"),
                        LifeExperience = reader.GetFieldValue<long>("lifeExperience"),
                        DateCreated = reader.GetFieldValue<DateTime>("dateCreated"),
                        LastSeen = reader.GetFieldValue<DateTime>("lastSeen"),
                        BannedPerm = reader.GetFieldValue<bool>("bannedPerm")
                    };

                    if (!DBNull.Value.Equals(reader["bannedUntil"]))
                        user.BannedUntil = reader.GetFieldValue<DateTime>("bannedUntil");

                    users.Add(user);
                }
            }
            return users;
        }
    }
}
