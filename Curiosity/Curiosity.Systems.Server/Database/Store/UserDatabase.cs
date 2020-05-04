using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Curiosity.Systems.Server.Database.Store
{
    class UserDatabase
    {
        public static async Task<CuriosityUser> Get(string license, Player player, ulong discordId)
        {
            Logger.Debug($"User: {player.Name}, License: {license}, DiscordId: {discordId}");

            Dictionary<string, object> myParams = new Dictionary<string, object>()
            {
                { "@licenseIn", license },
                { "@usernameIn", player.Name },
                { "@discordIdIn", discordId }
            };

            string myQuery = "CALL curiosity.spGetUser_v2(@license, @username, @discordId);";

            using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
            {
                ResultSet keyValuePairs = await result;

                if (keyValuePairs.Count == 0)
                    return null;

                CuriosityUser curiosityUser = new CuriosityUser();

                foreach (Dictionary<string, object> kv in keyValuePairs)
                {
                    curiosityUser.UserId = (long)kv["userId"];
                    curiosityUser.License = (string)kv["license"];
                    curiosityUser.LifeExperience = (long)kv["lifeExperience"];
                    curiosityUser.DateCreated = (DateTime)kv["dateCreated"];
                    curiosityUser.LatestActivity = (DateTime)kv["lastSeen"];
                    curiosityUser.BannedPerm = (bool)kv["bannedPerm"];
                    curiosityUser.Banned = (bool)kv["banned"];
                    curiosityUser.QueuePriority = (int)kv["queuePriority"];
                    curiosityUser.QueueLevel = (int)kv["queueLevel"];
                    curiosityUser.Role = (Role)kv["roleId"];
                    curiosityUser.LastName = player.Name;
                    curiosityUser.DiscordId = discordId;

                    if (kv.ContainsValue("bannedUntil"))
                        curiosityUser.BannedUntil = (DateTime)kv["bannedUntil"];
                }

                return curiosityUser;
            }
        }
    }
}
