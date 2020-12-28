using CitizenFX.Core;
using Curiosity.Systems.Library.Models.Discord;
using Curiosity.Core.Server.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Web
{
    class DiscordClient
    {
        static Request request = new Request();

        public async Task<RequestResponse> DiscordWebsocket(string method, string url, string jsonData)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Authorization", $"Bot {PluginManager.DiscordBotKey}");
            return await request.Http($"{url}", method, jsonData, headers);
        }

        public async Task<RequestResponse> DiscordRequest(string method, string endpoint, string jsonData)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Authorization", $"Bot {PluginManager.DiscordBotKey}");
            return await request.Http($"https://discordapp.com/api/{endpoint}", method, jsonData, headers);
        }

        public async Task<bool> CheckDiscordIdIsInGuild(Player player, ulong discordId)
        {
            bool IsMember = false;

            RequestResponse requestResponse = await DiscordRequest("GET", $"guilds/{PluginManager.DiscordGuildId}/members/{discordId}", string.Empty);

            if (requestResponse.status == System.Net.HttpStatusCode.NotFound)
            {
                Logger.Info($"DiscordClient : {player.Name} is NOT a member of the Discord Guild.");
                player.Drop($"This server requires that your are a member of their Discord.\nDiscord URL: {PluginManager.DiscordUrl}");
                return IsMember;
            }

            if (!(requestResponse.status == System.Net.HttpStatusCode.OK))
            {
                Logger.Error($"DiscordClient : Error communicating with Discord");
                player.Drop($"Error communicating with Discord, please raise a support ticket or try connecting again later.");
                return IsMember;
            }

            Member discordMember = JsonConvert.DeserializeObject<Member>(requestResponse.content);

            IsMember = discordMember.JoinedAt.HasValue;

            Logger.Success($"DiscordClient : {player.Name} is a member of the Discord Guild.");

            return IsMember;
        }
    }
}
