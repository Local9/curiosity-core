using CitizenFX.Core.Native;
using CitizenFX.Core;
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

        static bool isDiscordTimedOut = false;

        static ConcurrentDictionary<string, Privilege> privileges = new ConcurrentDictionary<string, Privilege>();
        static ConcurrentDictionary<long, DateTime> RanDiscord = new ConcurrentDictionary<long, DateTime>();

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

            CleanUp();
            DiscordReset();
        }

        static async void CleanUp()
        {
            while (true)
            {
                await Server.Delay((1000 * 60) * 120);
                ConcurrentDictionary<long, DateTime> RanDiscordToCheck = new ConcurrentDictionary<long, DateTime>(RanDiscord);
                foreach (KeyValuePair<long, DateTime> keyValuePair in RanDiscordToCheck)
                {
                    if ((DateTime.Now - keyValuePair.Value).TotalMinutes > 90)
                    {
                        RanDiscord.TryRemove(keyValuePair.Key, out DateTime dateTime);
                    }
                }
            }
        }

        static async void DiscordReset()
        {
            while (isDiscordTimedOut)
            {
                await Server.Delay((1000 * 60) * 30);
                isDiscordTimedOut = false;
            }
        }

        static public async Task<RequestResponse> DiscordWebsocket(string method, string url, string jsonData)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Authorization", $"Bot {discordBotKey}");
            return await request.Http($"{url}", method, jsonData, headers);
        }

        static async Task<RequestResponse> DiscordRequest(string method, string endpoint, string jsonData)
        {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                headers.Add("Content-Type", "application/json");
                headers.Add("Authorization", $"Bot {discordBotKey}");
                return await request.Http($"https://discordapp.com/api/{endpoint}", method, jsonData, headers);
        }

        public static async Task<Privilege> DiscordPrivilege(long discordId, Privilege privilegeIn, Player player)
        {
            try
            {
                if (isDiscordTimedOut) {
                    Helpers.Notifications.Advanced($"Discord", $"Hello ~g~{player.Name}~s~, Discord is currently not allowing connections, we cannot confirm your role.", 63, player, NotificationType.CHAR_LIFEINVADER);
                    return privilegeIn;
                }

                if (RanDiscord.ContainsKey(discordId))
                {
                    DateTime checkDate = RanDiscord[discordId];
                    DateTime dateNow = DateTime.Now;

                    TimeSpan timeSpan = dateNow - checkDate;

                    if (timeSpan.TotalMinutes <= 30) return privilegeIn;
                }

                RanDiscord.TryAdd(discordId, DateTime.Now);

                RequestResponse requestResponse = await DiscordRequest("GET", $"guilds/{discordGuild}/members/{discordId}", string.Empty);
                Privilege privilege = privilegeIn;
                if (requestResponse.status == System.Net.HttpStatusCode.OK)
                {
                    Entity.Discord.Member member = JsonConvert.DeserializeObject<Entity.Discord.Member>(requestResponse.content);

                    if (member.Roles.Length == 0)
                    {
                        privilege = Privilege.USER;
                    }
                    else
                    {
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

                                if (privilege == privilegeIn)
                                {
                                    // NO CHANGE
                                    return privilege;
                                }

                                //await Classes.DiscordWrapper.SendDiscordEmbededMessage(Enums.Discord.WebhookChannel.ServerLog, API.GetConvar("server_message_name", "SERVERNAME_MISSING"), "Discord Role Update",
                                //    $"Player: {player.Name}\nDiscord: {discordId}\nPriviledgeIn: {privilegeIn}\nPriviledgeMatch: {privilege}\nJSON: {JsonConvert.SerializeObject(member)}",
                                //    Enums.Discord.DiscordColor.Blue);
                                break;
                            }
                            else
                            {
                                privilege = Privilege.USER;
                            }
                        }
                    }

                    return privilege;
                }
                else if (requestResponse.status == System.Net.HttpStatusCode.NotFound)
                {
                    //await Classes.DiscordWrapper.SendDiscordEmbededMessage(Enums.Discord.WebhookChannel.ServerLog, API.GetConvar("server_message_name", "SERVERNAME_MISSING"), "Discord Warning", $"User was not found:\nName: {player.Name}\nDiscord: {discordId}\nRole: {privilegeIn}", Enums.Discord.DiscordColor.Orange);
                    Helpers.Notifications.Advanced($"Discord", $"Hello ~g~{player.Name}~s~, if you'd like to join our Discord please visit ~y~discord.lifev.net", 63, player, NotificationType.CHAR_LIFEINVADER);
                    Log.Verbose($"DiscordPrivilege -> User {player.Name} was not found on the Discord server.");
                    return Privilege.USER;
                }
                else
                {
                    //await Classes.DiscordWrapper.SendDiscordEmbededMessage(Enums.Discord.WebhookChannel.ServerLog, API.GetConvar("server_message_name", "SERVERNAME_MISSING"), "Discord Error", $"An error occured, please check the config:\nDiscord Response: {requestResponse.status}\nName: {player.Name}\nDiscord: {discordId}\nRole: {privilegeIn}", Enums.Discord.DiscordColor.Orange);
                    Log.Warn($"DiscordPrivilege -> An error occured, please check the config: Error {requestResponse.status}");

                    if ($"{requestResponse.status}" == "TooManyRequests")
                    {
                        isDiscordTimedOut = true;
                    }

                    return privilege;
                }
            }
            catch (Exception ex)
            {
                //await Classes.DiscordWrapper.SendDiscordEmbededMessage(Enums.Discord.WebhookChannel.ServerLog, API.GetConvar("server_message_name", "SERVERNAME_MISSING"), "Discord Error", $"DiscordPrivilege() -> {ex.Message}\nName: {player.Name}\nDiscord: {discordId}\nRole: {privilegeIn}", Enums.Discord.DiscordColor.Red);
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
