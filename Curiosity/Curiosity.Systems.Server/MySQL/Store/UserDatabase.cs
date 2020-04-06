using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.MySQL.Store
{
    class UserDatabase
    {
        public static async Task<CuriosityUser> Get(string license, Player player, ulong discordId)
        {
            Logger.Debug($"User: {player.Name}, License: {license}, DiscordId: {discordId}");

            using (var db = new MySqlDatabase())
            {
                await db.Connection.OpenAsync();

                using (var cmd = db.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "spGetUser_v2";
                    cmd.Parameters.AddWithValue("@licenseIn", license);
                    cmd.Parameters.AddWithValue("@usernameIn", player.Name);
                    cmd.Parameters.AddWithValue("@discordIdIn", discordId);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            if (!reader.HasRows)
                            {
                                player.Drop($"Sorry {player.Name}, an error occurred while you were trying to connect to the server or update your characters information, please try to connect again. If the issue persists visit our Discord @ {CuriosityPlugin.DiscordUrl}");
                                throw new Exception($"SQL ERROR -> Failed to find information for {player.Name} [{player.Identifiers["license"]}]");
                            }

                            CuriosityUser curiosityUser = new CuriosityUser()
                            {
                                UserId = reader.GetInt64(0),
                                License = reader.GetString(1),
                                LifeExperience = reader.GetInt64(2),
                                DateCreated = reader.GetDateTime(3),
                                LatestActivity = reader.GetDateTime(4),
                                BannedPerm = reader.GetBoolean(5),
                                Banned = reader.GetBoolean(7),
                                QueuePriority = reader.GetInt32(8),
                                QueueLevel = reader.GetInt32(9)
                            };

                            if (!reader.IsDBNull(6))
                                curiosityUser.BannedUntil = reader.GetDateTime(6);

                            curiosityUser.Role = (Role)reader.GetInt32(10);
                            curiosityUser.LastName = player.Name;
                            curiosityUser.DiscordId = discordId;

                            return curiosityUser;
                        }
                    }
                }
            }
            return null;
        }
    }
}
