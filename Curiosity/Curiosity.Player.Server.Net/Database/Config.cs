using Curiosity.Global.Shared.net.Enums;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Database
{
    class Config
    {
        static MySQL mySQL;

        public static void Init()
        {
            mySQL = Database.mySQL;
        }

        public static async Task<Dictionary<string, Privilege>> GetDiscordRolesAsync()
        {
            Dictionary<string, Privilege> privileges = new Dictionary<string, Privilege>();

            string query = "call selDiscordRoles();";

            using (var result = mySQL.QueryResult(query))
            {
                ResultSet rs = await result;
                await Server.Delay(0);
                if (rs.Count == 0)
                {
                    return privileges;
                }

                foreach (Dictionary<string, object> pairs in rs)
                {
                    Privilege privilege = (Privilege)int.Parse($"{pairs["roleId"]}");

                    privileges.Add($"{pairs["discordId"]}", privilege);
                }

                return privileges;
            }
        }

        public static async Task<Dictionary<Enums.Discord.WebhookChannel, Entity.DiscordWebhook>> GetDiscordWebhooksAsync(int serverId)
        {
            Dictionary<Enums.Discord.WebhookChannel, Entity.DiscordWebhook> wh = new Dictionary<Enums.Discord.WebhookChannel, Entity.DiscordWebhook>();

            string query = "call selDiscordWebhooks(@serverId);";

            Dictionary<string, object> myParams = new Dictionary<string, object>();
            myParams.Add("@serverId", serverId);

            using (var result = mySQL.QueryResult(query, myParams))
            {
                ResultSet rs = await result;
                await Server.Delay(0);
                if (rs.Count == 0)
                {
                    return wh;
                }

                foreach(Dictionary<string, object> pairs in rs)
                {
                    Enums.Discord.WebhookChannel webhook = (Enums.Discord.WebhookChannel)int.Parse($"{pairs["discordTypeId"]}");

                    Entity.DiscordWebhook discordWebhook = new Entity.DiscordWebhook
                    {
                        Name = $"{pairs["name"]}",
                        Avatar = $"{pairs["avatarUrl"]}",
                        Url = $"{pairs["url"]}"
                    };

                    wh.Add(webhook, discordWebhook);
                }

                return wh;
            }
        }
    }
}
