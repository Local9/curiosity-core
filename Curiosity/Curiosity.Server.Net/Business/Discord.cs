using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Server.net.Helpers;
using Curiosity.Shared.Server.net.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Business
{
    static class Discord
    {
        static Request request = new Request();
        static string discordGuild;
        static string discordBotKey;

        static bool discordTimedOut = false;
        static int discordRateLimitSeconds = 2;

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
            while (discordTimedOut)
            {
                await Server.Delay(discordRateLimitSeconds * 1000);
                discordTimedOut = false;
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
                if (discordTimedOut)
                {
                    Helpers.Notifications.Advanced($"Discord", $"Hello ~g~{player.Name}~s~, Discord is currently not allowing connections, we cannot confirm your role.", 63, player, NotificationType.CHAR_LIFEINVADER);
                    return privilegeIn;
                }

                RequestResponse requestResponse = await DiscordRequest("GET", $"guilds/{discordGuild}/members/{discordId}", string.Empty);
                int rateLimitRemaining = int.Parse(requestResponse.headers["X-RateLimit-Remaining"]);

                if (rateLimitRemaining <= 2)
                {
                    discordTimedOut = true;
                    discordRateLimitSeconds = int.Parse(requestResponse.headers["X-RateLimit-Reset-After"]);
                }

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
                        discordTimedOut = true;
                        discordRateLimitSeconds = 60;
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

        public static async Task<bool> CheckDiscordIdIsInGuild(Player player, ulong discordId)
        {
            bool IsMember = false;

            RequestResponse requestResponse = await DiscordRequest("GET", $"guilds/{discordGuild}/members/{discordId}", string.Empty);

            if (requestResponse.status == System.Net.HttpStatusCode.NotFound)
            {
                Log.Info($"DiscordClient : {player.Name} is NOT a member of the Discord Guild.");
                player.Drop($"This server requires that your are a member of their Discord.\nDiscord URL: discord.lifev.net");
                return IsMember;
            }

            if (!(requestResponse.status == System.Net.HttpStatusCode.OK))
            {
                Log.Error($"DiscordClient : Error communicating with Discord");
                player.Drop($"Error communicating with Discord, please raise a support ticket or try connecting again later.");
                return IsMember;
            }

            Member discordMember = JsonConvert.DeserializeObject<Member>(requestResponse.content);

            IsMember = discordMember.JoinedAt.HasValue;

            Log.Success($"DiscordClient : {player.Name} is a member of the Discord Guild.");

            return IsMember;
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

    class DiscordWebhook
    {

    }

    public partial class Member
    {
        [JsonProperty("nick")]
        public object Nick { get; set; }

        [JsonProperty("user", NullValueHandling = NullValueHandling.Ignore)]
        public User User { get; set; }

        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Roles { get; set; }

        [JsonProperty("premium_since", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? PremiumSince { get; set; }

        [JsonProperty("deaf", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Deaf { get; set; }

        [JsonProperty("mute", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Mute { get; set; }

        [JsonProperty("joined_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? JoinedAt { get; set; }
    }

    public partial class User
    {
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }

        [JsonProperty("discriminator", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(ParseStringConverter))]
        public long? Discriminator { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string Avatar { get; set; }
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}
