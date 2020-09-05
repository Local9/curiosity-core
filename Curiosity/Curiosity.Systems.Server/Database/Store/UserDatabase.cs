using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Extensions;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Database.Store
{
    class UserDatabase
    {
        public static async Task<CuriosityUser> Get(Player player, ulong discordId)
        {
            try
            {
                Logger.Debug($"User: {player.Name}, DiscordId: {discordId}");

                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@usernameIn", player.Name },
                    { "@discordIdIn", discordId }
                };

                string myQuery = "CALL spGetUser(@usernameIn, @discordIdIn);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                        return null;

                    CuriosityUser curiosityUser = new CuriosityUser();

                    foreach (Dictionary<string, object> kv in keyValuePairs)
                    {
                        curiosityUser.UserId = kv["UserID"].ToLong();
                        curiosityUser.DateCreated = kv["DateCreated"].ToDateTime();
                        curiosityUser.LatestActivity = kv["LastJoined"].ToDateTime();
                        curiosityUser.IsBannedPerm = kv["IsPermBanned"].ToBoolean();
                        curiosityUser.IsBanned = kv["IsBanned"].ToBoolean();
                        curiosityUser.Role = (Role)kv["RoleID"].ToInt();
                        curiosityUser.QueuePriority = kv["QueuePriority"].ToInt();
                        curiosityUser.LatestName = player.Name;
                        curiosityUser.DiscordId = discordId;

                        if (kv.ContainsValue("bannedUntil"))
                            curiosityUser.BannedUntil = kv["bannedUntil"].ToDateTime();
                    }

                    return curiosityUser;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return null;
            }
        }
    }
}
