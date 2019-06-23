using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Server.net.Helpers;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Curiosity.Server.net.Business
{
    static class Discord
    {
        static Request request = new Request();
        static string discordGuild;
        static string discordBotKey;

        static ConcurrentDictionary<string, Privilege> privileges = new ConcurrentDictionary<string, Privilege>();

        public static void Init()
        {
            discordGuild = API.GetConvar("discord_guild", "MISSING");
            discordBotKey = API.GetConvar("discord_bot", "MISSING");

            if (discordGuild == "MISSING")
            {
                Log.Warn("discord_guild: Missing or not configured");
            }


            if (discordBotKey == "MISSING")
            {
                Log.Warn("discord_bot: Missing or not configured");
            }

            if (discordGuild != "MISSING" && discordBotKey != "MISSING")
            {
                CheckDiscordSetup();
            }
        }

        static async Task<RequestResponse> DiscordRequest(string method, string endpoint, string jsonData)
        {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Content-Type", "application/json");
                headers.Add("Authorization", $"Bot {discordBotKey}");
                return await request.Http($"https://discordapp.com/api/{endpoint}", method, jsonData, headers);
        }

        public static async Task<Privilege> DiscordPrivilege(long discordId, Privilege privilegeIn, string name)
        {
            try
            {
                RequestResponse requestResponse = await DiscordRequest("GET", $"guilds/{discordGuild}/members/{discordId}", string.Empty);
                Privilege privilege = privilegeIn;
                if (requestResponse.status == System.Net.HttpStatusCode.OK)
                {
                    Entity.Discord.Member member = JsonConvert.DeserializeObject<Entity.Discord.Member>(requestResponse.content);

                    foreach (string role in member.Roles)
                    {
                        if (privileges.Count == 0)
                        {
                            privilege = Privilege.USER;
                            break;
                        }

                        if (privileges.ContainsKey(role))
                        {
                            privilege = privileges[role];
                            break;
                        }
                        else
                        {
                            privilege = Privilege.USER;
                        }
                    }
                    return privilege;
                }
                else if (requestResponse.status == System.Net.HttpStatusCode.NotFound)
                {
                    await Classes.DiscordWrapper.SendDiscordEmbededMessage(Enums.Discord.WebhookChannel.ServerLog, API.GetConvar("server_message_name", "SERVERNAME_MISSING"), "Discord Warning", $"User was not found: {name}|{discordId}|{privilegeIn}", Enums.Discord.DiscordColor.Orange);
                    Log.Verbose($"User was not found on the Discord server.");
                    return Privilege.USER;
                }
                else
                {
                    await Classes.DiscordWrapper.SendDiscordEmbededMessage(Enums.Discord.WebhookChannel.ServerLog, API.GetConvar("server_message_name", "SERVERNAME_MISSING"), "Discord Error", $"An error occured, please check the config:\nDiscord Response: {requestResponse.status}\nPlayer: {name}|{discordId}|{privilegeIn}", Enums.Discord.DiscordColor.Orange);
                    Log.Warn($"An error occured, please check the config: Error {requestResponse.status}");
                    return privilege;
                }
            }
            catch (Exception ex)
            {
                await Classes.DiscordWrapper.SendDiscordEmbededMessage(Enums.Discord.WebhookChannel.ServerLog, API.GetConvar("server_message_name", "SERVERNAME_MISSING"), "Discord Error", $"DiscordPrivilege() -> {ex.Message}\nPlayer: {name}|{discordId}|{privilegeIn}", Enums.Discord.DiscordColor.Red);
                Log.Error($"DiscordPrivilege() -> {ex.Message}");
                return privilegeIn;
            }
        }

        static async void CheckDiscordSetup()
        {
            try
            {
                while (!Server.serverActive)
                {
                    await Server.Delay(0);
                }

                privileges = await Database.Config.GetDiscordRolesAsync();

                if (privileges.Count == 0)
                {
                    Log.Warn("Discord Roles: Missing or not configured");
                }

                RequestResponse requestResponse = await DiscordRequest("GET", $"guilds/{discordGuild}", string.Empty);
                if (requestResponse.status == System.Net.HttpStatusCode.OK)
                {
                    Entity.Discord.Guild guild = JsonConvert.DeserializeObject<Entity.Discord.Guild>(requestResponse.content);
                    Log.Success($"Discord System set to: {guild.Name} ({guild.Id})");
                }
                else
                {
                    Log.Warn($"An error occured, please check the config: Error {requestResponse.status}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"DiscordRequest() -> {ex.Message}");
            }
        }
    }
}
