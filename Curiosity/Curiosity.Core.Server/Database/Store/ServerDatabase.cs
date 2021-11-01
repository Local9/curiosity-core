using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Web;
using Curiosity.Core.Server.Web.Discord.Entity;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using GHMatti.Data.MySQL.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Database.Store
{
    class ServerDatabase
    {
        public static async Task<bool> CheckServerKey(int serverId)
        {
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@ServerID", serverId }
                };

                string myQuery = "CALL selServer(@ServerID);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    await BaseScript.Delay(0);

                    if (keyValuePairs.Count == 0)
                        return false;

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return false;
            }
        }

        internal static async Task<Dictionary<WebhookChannel, DiscordWebhook>> GetDiscordWebhooks(int serverId)
        {
            Dictionary<WebhookChannel, DiscordWebhook> wh = new Dictionary<WebhookChannel, DiscordWebhook>();
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@ServerID", serverId }
                };

                string myQuery = "CALL selDiscordWebhooks(@ServerID);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    await BaseScript.Delay(0);

                    if (keyValuePairs.Count == 0)
                        return wh;

                    foreach (Dictionary<string, object> kv in keyValuePairs)
                    {
                        WebhookChannel webhook = (WebhookChannel)int.Parse($"{kv["discordTypeId"]}");

                        DiscordWebhook discordWebhook = new DiscordWebhook
                        {
                            Name = $"{kv["name"]}",
                            Avatar = $"{kv["avatarUrl"]}",
                            Url = $"{kv["url"]}"
                        };

                        wh.Add(webhook, discordWebhook);
                    }

                    return wh;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return wh;
            }
        }

        internal async static Task<List<LogItem>> GetList(LogGroup logGroup)
        {
            List<LogItem> lst = new List<LogItem>();
            try
            {
                Dictionary<string, object> myParams = new Dictionary<string, object>()
                {
                    { "@logGroupId", (int)logGroup }
                };

                string myQuery = "CALL selLogReasons(@logGroupId);";

                using (var result = MySqlDatabase.mySQL.QueryResult(myQuery, myParams))
                {
                    ResultSet keyValuePairs = await result;

                    await BaseScript.Delay(0);

                    if (keyValuePairs.Count == 0)
                        return lst;

                    foreach (Dictionary<string, object> kv in keyValuePairs)
                    {
                        LogItem logItem = new LogItem();
                        logItem.LogTypeId = kv["logTypeId"].ToInt();
                        logItem.Group = $"{kv["group"]}";
                        logItem.Description = $"{kv["reason"]}";

                        lst.Add(logItem);
                    }

                    return lst;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
                return lst;
            }
        }
    }
}
