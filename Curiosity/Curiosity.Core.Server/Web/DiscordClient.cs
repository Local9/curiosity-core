﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Managers;
using Curiosity.Systems.Library.Models.Discord;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Web
{
    public class DiscordClient : Manager<DiscordClient>
    {
        static Request request = new Request();
        public static DiscordClient DiscordInstance;

        public override void Begin()
        {
            DiscordInstance = this;
        }

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

        public async Task<bool> CheckDiscordIdIsInGuild(Player player)
        {
            bool IsMember = false;

            string discordIdStr = player.Identifiers["discord"];

            ulong discordId = 0;
            if (!ulong.TryParse(discordIdStr, out discordId))
            {
                Logger.Info($"DiscordClient : {player.Name} Discord Information was invalid.");
                player.Drop($"Discord Identity failed validation, please restart FiveM and Discord. After you have opened Discord, then open FiveM.");
                return IsMember;
            }

            if (discordId == 0)
            {
                Logger.Info($"DiscordClient : {player.Name} Discord Information was invalid.");
                player.Drop($"Discord Identity failed validation, please restart FiveM and Discord. After you have opened Discord, then open FiveM.");
                return IsMember;
            }

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

            string verifiedRoleConvar = API.GetConvar("discord_verified_roleId", "ROLE_NOT_SET");

            if (verifiedRoleConvar != "ROLE_NOT_SET")
            {
                if (discordMember.Roles.Contains($"{verifiedRoleConvar}"))
                {
                    Logger.Success($"DiscordClient : {player.Name} is a verified member of the Discord Guild.");
                    return true;
                }
            }

            IsMember = discordMember.JoinedAt.HasValue;

            Logger.Success($"DiscordClient : {player.Name} is a member of the Discord Guild.");

            return IsMember;
        }
    }
}
