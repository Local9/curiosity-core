using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Web;
using Curiosity.Core.Server.Web.Discord.Entity;
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
    }
}
