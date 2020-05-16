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
        public static async Task<CuriosityUser> Get(string license, Player player, ulong discordId)
        {
            try
            {
                Logger.Debug($"User: {player.Name}, License: {license}, DiscordId: {discordId}");

                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@licenseIn", license },
                    { "@usernameIn", player.Name },
                    { "@discordIdIn", discordId }
                };

                string myQuery = "CALL curiosity.spGetUser_v2(@licenseIn, @usernameIn, @discordIdIn);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                        return null;

                    CuriosityUser curiosityUser = new CuriosityUser();

                    foreach (Dictionary<string, object> kv in keyValuePairs)
                    {
                        curiosityUser.UserId = kv["userId"].ToLong();
                        curiosityUser.License = kv["license"].ToString();
                        curiosityUser.LifeExperience = kv["lifeExperience"].ToLong();
                        curiosityUser.DateCreated = kv["dateCreated"].ToDateTime();
                        curiosityUser.LatestActivity = kv["lastSeen"].ToDateTime();
                        curiosityUser.BannedPerm = kv["bannedPerm"].ToBoolean();
                        curiosityUser.Banned = kv["banned"].ToBoolean();
                        curiosityUser.QueuePriority = kv["queuePriority"].ToInt();
                        curiosityUser.QueueLevel = kv["queueLevel"].ToInt();
                        curiosityUser.Role = (Role)kv["roleId"].ToInt();
                        curiosityUser.LastName = player.Name;
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
