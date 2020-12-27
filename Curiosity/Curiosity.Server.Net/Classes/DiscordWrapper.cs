using CitizenFX.Core.Native;
using Curiosity.Server.net.Entity.Discord;
using Curiosity.Server.net.Enums.Discord;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Classes
{
    class DiscordWrapper
    {
        static Server server = Server.GetInstance();
        static Dictionary<WebhookChannel, Entity.DiscordWebhook> webhooks = new Dictionary<WebhookChannel, Entity.DiscordWebhook>();

        static long setupChecker = API.GetGameTimer();
        static string DATE_FORMAT = "yyyy-MM-dd HH:mm";
        public static bool isConfigured = false;

        static bool IsDelayRunnning = false;
        static long DelayMillis = 0;

        private static Regex _compiledUnicodeRegex = new Regex(@"[^\u0000-\u007F]", RegexOptions.Compiled);

        public static String StripUnicodeCharactersFromString(string inputValue)
        {
            return _compiledUnicodeRegex.Replace(inputValue, String.Empty);
        }

        public static void Init()
        {
            if (Server.isLive)
            {
                server.RegisterEventHandler("curiosity:Server:Discord:Report", new Action<string, string, string>(SendDiscordReportMessage));
            }

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
                    {
                        isConfigured = true;
                        server.DeregisterTickHandler(SetupDiscordWebhooksDictionary);
                    }
                }
            }
        }

        public static async void SendDiscordChatMessage(string name, string message)
        {
            if (!webhooks.ContainsKey(WebhookChannel.Chat))
            {
                Log.Warn($"SendDiscordChatMessage() -> Discord Chat Webhook Missing");
                return;
            }

            await SendDiscordSimpleMessage(WebhookChannel.Chat, Server.hostname, name, message);
        }

        public static async void SendDiscordPlayerLogMessage(string message)
        {
            if (!webhooks.ContainsKey(WebhookChannel.PlayerLog))
            {
                Log.Warn($"SendDiscordChatMessage() -> Discord Player Webhook Missing");
                return;
            }

            try
            {
                Entity.DiscordWebhook discordWebhook = webhooks[WebhookChannel.PlayerLog];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = StripUnicodeCharactersFromString($"`{DateTime.Now.ToString(DATE_FORMAT)}`: {message}");
                webhook.Username = StripUnicodeCharactersFromString(Server.hostname);

                await webhook.Send();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Log.Error($"SendDiscordPlayerLogMessage() -> {ex.Message}");
            }

        }

        public static async void SendDiscordStaffMessage(string adminName, string player, string action, string reason, string duration)
        {
            try
            {
                if (!webhooks.ContainsKey(WebhookChannel.StaffLog))
                {
                    Log.Warn($"SendDiscordStaffMessage() -> Discord {WebhookChannel.StaffLog} Webhook Missing");
                    return;
                }

                if (IsDelayRunnning) return;

                Entity.DiscordWebhook discordWebhook = webhooks[WebhookChannel.StaffLog];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = $"`{DateTime.Now.ToString(DATE_FORMAT)}`";
                webhook.Username = "Staff";

                Embed embed = new Embed();
                embed.Author = new EmbedAuthor { Name = adminName, IconUrl = discordWebhook.Avatar };
                embed.Title = StripUnicodeCharactersFromString($"Player: {player}");

                embed.Description = StripUnicodeCharactersFromString($" **{action}**: {reason}");
                if (!string.IsNullOrEmpty(duration))
                    embed.Description = StripUnicodeCharactersFromString($" **{action}**: {reason} \n **Duration**: {duration}");

                embed.Color = (int)DiscordColor.Orange;
                if (action == "Ban")
                    embed.Color = (int)DiscordColor.Red;

                embed.Thumbnail = new EmbedThumbnail { Url = discordWebhook.Avatar };

                webhook.Embeds.Add(embed);
                await Server.Delay(0);
                await webhook.Send();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Log.Error($"SendDiscordStaffMessage() -> {ex.Message}");
            }
        }

        public static async void SendDiscordReportMessage(string reporterName, string playerBeingReported, string reason)
        {
            try
            {
                if (!webhooks.ContainsKey(WebhookChannel.Report))
                {
                    Log.Warn($"SendDiscordReportMessage() -> Discord {WebhookChannel.Report} Webhook Missing");
                    return;
                }

                if (IsDelayRunnning) return;

                Entity.DiscordWebhook discordWebhook = webhooks[WebhookChannel.Report];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = $"`{DateTime.Now.ToString(DATE_FORMAT)}`";
                string cleanName = StripUnicodeCharactersFromString(Server.hostname);
                webhook.Username = cleanName;

                Embed embed = new Embed();
                embed.Author = new EmbedAuthor { Name = $"Report By: {reporterName}", IconUrl = discordWebhook.Avatar };
                embed.Title = $"Report";
                embed.Description = $"Player: {playerBeingReported}\nReason: {reason}";
                embed.Color = (int)DiscordColor.Blue;
                embed.Thumbnail = new EmbedThumbnail { Url = discordWebhook.Avatar };

                webhook.Embeds.Add(embed);

                await webhook.Send();
            }
            catch (Exception ex)
            {
                Log.Error($"SendDiscordReportMessage() -> {ex.Message}");
            }
        }

        public static async Task SendDiscordEmbededMessage(WebhookChannel webhookChannel, string name, string title, string description, DiscordColor discordColor)
        {
            try
            {
                if (!webhooks.ContainsKey(webhookChannel))
                {
                    Log.Warn($"SendDiscordEmbededMessage() -> Discord {webhookChannel} Webhook Missing");
                    return;
                }

                if (IsDelayRunnning) return;

                string cleanName = StripUnicodeCharactersFromString(name);

                Entity.DiscordWebhook discordWebhook = webhooks[webhookChannel];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = $"`{DateTime.Now.ToString(DATE_FORMAT)}`";
                webhook.Username = cleanName;

                Embed embed = new Embed();
                embed.Author = new EmbedAuthor { Name = cleanName, IconUrl = discordWebhook.Avatar };
                embed.Title = StripUnicodeCharactersFromString(title);
                embed.Description = StripUnicodeCharactersFromString(description);
                embed.Color = (int)discordColor;
                embed.Thumbnail = new EmbedThumbnail { Url = discordWebhook.Avatar };

                webhook.Embeds.Add(embed);
                await Server.Delay(0);
                await webhook.Send();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Log.Error($"SendDiscordEmbededMessage() -> {ex.Message}");
            }
        }

        static async void DelayTriggered(long delayMillis)
        {
            DelayMillis = delayMillis * 1000;
        }

        static async Task OnDelayCooldownTask()
        {
            long gameTimer = API.GetGameTimer();
            IsDelayRunnning = true;

            while ((API.GetGameTimer() - gameTimer) < DelayMillis)
            {
                await Server.Delay(1000);
            }

            server.DeregisterTickHandler(OnDelayCooldownTask);
            IsDelayRunnning = false;
        }

        public static async Task SendDiscordSimpleMessage(WebhookChannel webhookChannel, string username, string name, string message)
        {
            try
            {
                Entity.DiscordWebhook discordWebhook = webhooks[webhookChannel];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = StripUnicodeCharactersFromString($"{name} > {message}");
                webhook.Username = StripUnicodeCharactersFromString(username);

                await webhook.Send();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Log.Error($"SendDiscordSimpleMessage() -> {ex.Message}");
            }
        }

        class WebhookMessage
        {

        }
    }
}
