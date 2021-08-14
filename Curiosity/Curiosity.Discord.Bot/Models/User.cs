using Curiosity.LifeV.Bot.Entities.Curiosity;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot.Models
{
    public class User
    {
        public long UserId;
        public string Username;
        public long Experience;
        public DateTime DateCreated;
        public DateTime LastSeen;
        public bool BannedPerm;
        public DateTime? BannedUntil;
        public ulong? DiscordId;
        public Role? UserRole;

        public bool IsStaff => (UserRole == Role.COMMUNITY_MANAGER || UserRole == Role.MODERATOR || UserRole == Role.ADMINISTRATOR || UserRole == Role.SENIOR_ADMIN || UserRole == Role.HEAD_ADMIN || UserRole == Role.DEVELOPER || UserRole == Role.PROJECT_MANAGER);

        public User() { }

        public async Task<User> FindUserByCuriosityUserId(long userId)
        {
            try
            {
                using var connection = await Database.DatabaseConfig.GetDatabaseConnection();
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"call selUser(@identifer);";
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@identifer",
                    DbType = DbType.Int64,
                    Value = userId
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

        public async Task<List<User>> GetTopUsers(string stat)
        {
            try
            {
                using var connection = await Database.DatabaseConfig.GetDatabaseConnection();
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"call selStatsTopUsers(@TopStat);";

                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@TopStat",
                    DbType = DbType.String,
                    Value = stat
                });

                var result = await ReadAllAsync(await cmd.ExecuteReaderAsync());
                return result.Count > 0 ? result : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetTopUsers -> {ex}");
                return default;
            }
        }

        public async Task<List<User>> GetUsersWithDonationStatus()
        {
            try
            {
                using var connection = await Database.DatabaseConfig.GetDatabaseConnection();
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"call selDonatingUsers();";
                var result = await ReadAllAsync(await cmd.ExecuteReaderAsync());
                return result.Count > 0 ? result : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"selDonatingUsers -> {ex}");
                return default;
            }
        }

        public async Task RemoveDonatorStatus()
        {
            try
            {
                using var connection = await Database.DatabaseConfig.GetDatabaseConnection();
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"call upUserRemoveDonatorRole(@userId);";
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@userId",
                    DbType = DbType.Int64,
                    Value = this.UserId
                });

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RemoveDonatorStatus -> {ex}");
            }
        }

        public async Task AddDonatorStatus(int donatorRoleId)
        {
            try
            {
                using var connection = await Database.DatabaseConfig.GetDatabaseConnection();
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"call upUserAddDonatorRole(@userId, @DonatorRoleId);";
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@userId",
                    DbType = DbType.Int64,
                    Value = this.UserId
                });
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@DonatorRoleId",
                    DbType = DbType.Int64,
                    Value = donatorRoleId
                });

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AddDonatorStatus -> {ex}");
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
                        DateCreated = reader.GetFieldValue<DateTime>("dateCreated"),
                        LastSeen = reader.GetFieldValue<DateTime>("lastSeen"),
                        BannedPerm = reader.GetFieldValue<bool>("bannedPerm"),
                    };

                    if (!DBNull.Value.Equals(reader["bannedUntil"]))
                        user.BannedUntil = reader.GetFieldValue<DateTime>("bannedUntil");

                    if (!DBNull.Value.Equals(reader["discordId"]))
                        user.DiscordId = reader.GetFieldValue<ulong>("discordId");

                    if (!DBNull.Value.Equals(reader["roleId"]))
                    {
                        int roleId = reader.GetFieldValue<int>("roleId");
                        user.UserRole = (Role)roleId;
                    }


                    users.Add(user);
                }
            }
            return users;
        }
    }
}
