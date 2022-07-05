using Curiosity.Framework.Server.Attributes;
using Curiosity.Framework.Server.Managers;
using Curiosity.Framework.Server.Models;
using Curiosity.Framework.Server.Web.Discord.API;
using Curiosity.Framework.Shared;
using System.Text.RegularExpressions;

namespace Curiosity.Framework.Server.Web.Discord
{
    public enum WebhookChannel
    {
        ServerPlayerLog,
        ServerErrorLog,
        ServerDebugLog,
        ServerChatLog
    }

    public enum DiscordColor : int
    {
        White = 16777215,
        Black = 0,
        Red = 16738657,
        Green = 7855479,
        Blue = 11454159,
        Orange = 16757575
    }

    public class DiscordClient : Manager<DiscordClient>
    {
        List<DiscordMessage> discordMessages = new();

        static ServerConfig _srvCfg => ServerConfiguration.GetServerConfig;

        static Request request = new Request();
        public Dictionary<WebhookChannel, string> Webhooks = new Dictionary<WebhookChannel, string>();
        static long lastUpdate = GetGameTimer();
        static bool IsDelayRunnning = false;

        private static Regex _compiledUnicodeRegex = new Regex(@"[^\u0000-\u007F]", RegexOptions.Compiled);

        public override void Begin()
        {
            Logger.Trace($"INIT DISCORD CLIENT");
        }

        public String StripUnicodeCharactersFromString(string inputValue)
        {
            return _compiledUnicodeRegex.Replace(inputValue, String.Empty);
        }

        [TickHandler]
        private async Task OnDiscordWebhookUpdate()
        {
            if ((GetGameTimer() - lastUpdate) > 120000)
            {
                lastUpdate = GetGameTimer();
                UpdateWebhooks();
            }

            while (Webhooks.Count == 0)
            {
                UpdateWebhooks();
                await BaseScript.Delay(1000);
                if (Webhooks.Count == 0)
                {
                    Logger.Error($"No Discord Webhooks returned, trying again in five seconds.");
                    await BaseScript.Delay(5000);
                }
            }

            await BaseScript.Delay(10000);
        }

        private async void UpdateWebhooks()
        {
            try
            {
                while (!PluginManager.IsServerReady)
                {
                    await BaseScript.Delay(1000);
                }

                Webhooks = new Dictionary<WebhookChannel, string>()
                {
                    { WebhookChannel.ServerPlayerLog, _srvCfg.Discord.Webhooks.ServerPlayerLog },
                    { WebhookChannel.ServerErrorLog, _srvCfg.Discord.Webhooks.ServerError },
                    { WebhookChannel.ServerDebugLog, _srvCfg.Discord.Webhooks.ServerDebug },
                };
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<RequestResponse> DiscordWebsocket(string method, string url, string jsonData = "")
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Authorization", $"Bot {_srvCfg.Discord.BotKey}");
            return await request.HttpAsync($"{url}", method, jsonData, headers);
        }

        public async Task<RequestResponse> DiscordRequest(string method, string endpoint, string jsonData = "")
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Authorization", $"Bot {_srvCfg.Discord.BotKey}");
            return await request.HttpAsync($"https://discordapp.com/api/{endpoint}", method, jsonData, headers);
        }

        public async Task<string> Avatar(ulong discordId)
        {
            string url = $"https://api.discord.wf/v2/users/{discordId}/avatar";
            RequestResponse requestResponse = await request.HttpAsync(url);
            if (requestResponse.status == System.Net.HttpStatusCode.OK)
            {
                DiscordAvatar avatar = JsonConvert.DeserializeObject<DiscordAvatar>(requestResponse.content);
                return avatar.Avatarurl;
            }
            return string.Empty;
        }

        public async Task SendDiscordEmbededMessage(WebhookChannel webhookChannel, string name, string title, string description, DiscordColor discordColor)
        {
            try
            {
                if (!Webhooks.ContainsKey(webhookChannel))
                {
                    Logger.Warn($"SendDiscordEmbededMessage() -> Discord {webhookChannel} Webhook Missing");
                    return;
                }

                if (IsDelayRunnning) return;

                string cleanName = StripUnicodeCharactersFromString(name);

                string discordWebhook = Webhooks[webhookChannel];

                Webhook webhook = new Webhook(discordWebhook);

                webhook.Username = cleanName;

                Embed embed = new Embed();
                embed.Author = new EmbedAuthor { Name = cleanName };
                embed.Title = StripUnicodeCharactersFromString(title);
                embed.Description = StripUnicodeCharactersFromString(description);
                embed.Color = (int)discordColor;

                webhook.Embeds.Add(embed);
                await Common.MoveToMainThread();
                await webhook.Send();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Logger.Error($"SendDiscordEmbededMessage() -> {ex.Message}");
            }
        }

        public async Task SendDiscordSimpleMessage(WebhookChannel webhookChannel, string username, string name, string message)
        {
            try
            {
                string discordWebhook = Webhooks[webhookChannel];

                Webhook webhook = new Webhook(discordWebhook);

                webhook.Content = StripUnicodeCharactersFromString($"{name} > {message.Trim('"')}");
                webhook.Username = StripUnicodeCharactersFromString(username);

                await Common.MoveToMainThread();

                await webhook.Send();

                await Common.MoveToMainThread();
            }
            catch (Exception ex)
            {
                Logger.Error($"SendDiscordSimpleMessage() -> {ex.Message}");
            }
        }

        public async void AddDiscordMessageToQueue(WebhookChannel webhookChannel, string message)
        {
            await AddDiscordMessageToQueue(webhookChannel, "SERVER", "SERVER", message);
        }

        public async Task AddDiscordMessageToQueue(WebhookChannel webhookChannel, string username, string name, string message)
        {
            DiscordMessage discordMessage = new();
            discordMessage.WebhookChannel = webhookChannel;
            discordMessage.Username = username;
            discordMessage.Name = name;
            discordMessage.Message = message;
            discordMessages.Add(discordMessage);

            if (discordMessages.Count >= 5)
            {
                List<DiscordMessage> messages = new(discordMessages);
                foreach (DiscordMessage msg in messages)
                {
                    discordMessages.Remove(msg);
                }

                foreach (DiscordMessage msg in messages)
                {
                    await SendDiscordSimpleMessage(msg.WebhookChannel, msg.Username, msg.Name, msg.Message);
                }
            }
        }
    }

    class DiscordMessage
    {
        public WebhookChannel WebhookChannel;
        public string Username;
        public string Name;
        public string Message;
    }
}
