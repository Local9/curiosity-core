using CitizenFX.Core.Native;
using Curiosity.Server.net.Enums.Discord;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes
{
    class DiscordWrapper
    {
        static Server server = Server.GetInstance();
        static Dictionary<Webhook, Entity.DiscordWebhook> webhooks = new Dictionary<Webhook, Entity.DiscordWebhook>();

        static long setupChecker = API.GetGameTimer();

        public static void Init()
        {
            server.RegisterTickHandler(SetupDiscordWebhooksDictionary);
        }

        static async Task SetupDiscordWebhooksDictionary()
        {
            if ((API.GetGameTimer() - setupChecker) > 5000)
            {
                if (Server.serverId != 0)
                {
                    webhooks = await Database.Config.GetDiscordWebhooksAsync(Server.serverId);

                    if (webhooks.Count > 0)
                        server.DeregisterTickHandler(SetupDiscordWebhooksDictionary);
                }
            }
        }
    }
}
