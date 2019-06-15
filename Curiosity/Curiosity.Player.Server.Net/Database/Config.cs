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

        public static async Task<Dictionary<Enums.Discord.Webhook, Entity.DiscordWebhook>> GetDiscordWebhooksAsync(int serverId)
        {
            Dictionary<Enums.Discord.Webhook, Entity.DiscordWebhook> wh = new Dictionary<Enums.Discord.Webhook, Entity.DiscordWebhook>();

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
                    Enums.Discord.Webhook webhook = (Enums.Discord.Webhook)int.Parse($"{pairs["discordTypeId"]}");

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
