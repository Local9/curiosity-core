using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class UserDatabase
    {
        public static async Task<CuriosityUser> Get(Player player)
        {
            try
            {
                var discordIdStr = player.Identifiers["discord"];
                ulong discordId = 0;

                if (!ulong.TryParse(discordIdStr, out discordId))
                {
                    player.Drop("Error creating login session, Discord ID not found.");
                    API.CancelEvent();
                    return null;
                }

                if (discordId == 0)
                {
                    player.Drop("Error creating login session, Discord ID not found.");
                    API.CancelEvent();
                    return null;
                }

                Logger.Debug($"User: {player.Name}, DiscordId: {discordId}");

                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@Username", player.Name },
                    { "@DiscordID", discordId }
                };

                string myQuery = "CALL spGetUser(@Username, @DiscordID);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    if (keyValuePairs.Count == 0)
                        return null;

                    CuriosityUser curiosityUser = new CuriosityUser();

                    foreach (Dictionary<string, object> kv in keyValuePairs)
                    {
                        curiosityUser.UserId = kv["userId"].ToLong();
                        curiosityUser.DateCreated = kv["dateCreated"].ToDateTime();
                        curiosityUser.LatestActivity = kv["lastSeen"].ToDateTime();
                        curiosityUser.IsBannedPerm = kv["bannedPerm"].ToBoolean();
                        curiosityUser.IsBanned = kv["banned"].ToBoolean();
                        curiosityUser.Role = (Role)kv["roleId"].ToInt();
                        curiosityUser.QueuePriority = kv["queuePriority"].ToInt();
                        curiosityUser.LatestName = player.Name;
                        curiosityUser.DiscordId = discordId;

                        object bannedUntilObj;
                        if (kv.TryGetValue("bannedUntil", out bannedUntilObj))
                        {
                            if (bannedUntilObj != null)
                                curiosityUser.BannedUntil = bannedUntilObj.ToDateTime();
                        }
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

        internal static async Task<bool> LogKick(long userId, long staffUserId, int reasonId, int userCharacterId)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@userId", userId },
                    { "@loggedById", staffUserId },
                    { "@logTypeId", reasonId },
                    { "@characterId", userCharacterId }
                };

                string myQuery = "call spLogKickedUser(@userId, @loggedById, @logTypeId, @characterId);";

                await MySqlDatabase.mySQL.Query(myQuery, myParams);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return false;
            }
        }

        internal static async Task<bool> LogBan(long userId, long staffUserId, int reasonId, long userCharacterId, bool permBan, DateTime bannedUntil)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@userId", userId },
                    { "@loggedById", staffUserId },
                    { "@logTypeId", reasonId },
                    { "@characterId", userCharacterId },
                    { "@permBan", permBan },
                    { "@bannedUntil", bannedUntil },
                };

                string myQuery = "call spLogBannedUser(@userId, @loggedById, @logTypeId, @characterId, @permBan, @bannedUntil);";

                await MySqlDatabase.mySQL.Query(myQuery, myParams);

                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return false;
            }
        }
    }
}
