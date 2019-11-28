using CitizenFX.Core.Native;
using Curiosity.Server.net.Enums.Discord;
using Curiosity.Shared.Server.net.Helpers;
using System;
using System.Collections.Generic;
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

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Discord:ChatMessage", new Action<string, string>(SendDiscordChatMessage));
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

        static async void SendDiscordChatMessage(string name, string message)
        {
            if (!webhooks.ContainsKey(WebhookChannel.Chat))
            {
                Log.Warn($"SendDiscordChatMessage() -> Discord Chat Webhook Missing");
                return;
            }

            await SendDiscordSimpleMessage(WebhookChannel.Chat, "World", name, message);
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

                //Webhook webhook = new Webhook(discordWebhook.Url);

                //webhook.AvatarUrl = discordWebhook.Avatar;
                //webhook.Content = $"`{DateTime.Now.ToString(DATE_FORMAT)}`";
                //webhook.Username = "Staff";

                //Embed embed = new Embed();
                //embed.Author = new EmbedAuthor { Name = adminName, IconUrl = discordWebhook.Avatar };
                //embed.Title = $"Player: {player}";

                //embed.Description = $" **{action}**: {reason}";
                //if (!string.IsNullOrEmpty(duration))
                //    embed.Description = $" **{action}**: {reason} \n **Duration**: {duration}";

                //embed.Color = (int)DiscordColor.Orange;
                //if (action == "Ban")
                //    embed.Color = (int)DiscordColor.Red;

                //embed.Thumbnail = new EmbedThumbnail { Url = discordWebhook.Avatar };

                //webhook.Embeds.Add(embed);
                //await Server.Delay(0);
                //GHMatti.Http.RequestResponse requestResponse = await webhook.Send();

                //if (requestResponse.status == (System.Net.HttpStatusCode)429)
                //{
                //    long timer = (long)double.Parse(requestResponse.headers["X-RateLimit-Reset-After"]);
                //    DelayTriggered(timer);
                //}

                //if (requestResponse.status != (System.Net.HttpStatusCode)200)
                //{
                //    Log.Info($"SendDiscordEmbededMessage() -> Status: {requestResponse.status} - {requestResponse.content}");
                //}

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

                //Webhook webhook = new Webhook(discordWebhook.Url);

                //webhook.AvatarUrl = discordWebhook.Avatar;
                //webhook.Content = $"`{DateTime.Now.ToString(DATE_FORMAT)}`";
                //webhook.Username = $"Report by {reporterName}";

                //Embed embed = new Embed();
                //embed.Author = new EmbedAuthor { Name = reporterName, IconUrl = discordWebhook.Avatar };
                //embed.Title = $"Player: {playerBeingReported}";
                //embed.Description = $"Reason: {reason}";
                //embed.Color = (int)DiscordColor.Blue;
                //embed.Thumbnail = new EmbedThumbnail { Url = discordWebhook.Avatar };

                //webhook.Embeds.Add(embed);
                //await Server.Delay(0);
                //GHMatti.Http.RequestResponse requestResponse = await webhook.Send();

                //if (requestResponse.status == (System.Net.HttpStatusCode)429)
                //{
                //    long timer = (long)double.Parse(requestResponse.headers["X-RateLimit-Reset-After"]);
                //    DelayTriggered(timer);
                //}

                //if (requestResponse.status != (System.Net.HttpStatusCode)200)
                //{
                //    Log.Info($"SendDiscordEmbededMessage() -> Status: {requestResponse.status} - {requestResponse.content}");
                //}

                await Task.FromResult(0);
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

                Entity.DiscordWebhook discordWebhook = webhooks[webhookChannel];

                //Webhook webhook = new Webhook(discordWebhook.Url);

                //webhook.AvatarUrl = discordWebhook.Avatar;
                //webhook.Content = $"`{DateTime.Now.ToString(DATE_FORMAT)}`";
                //webhook.Username = name;

                //Embed embed = new Embed();
                //embed.Author = new EmbedAuthor { Name = name, IconUrl = discordWebhook.Avatar };
                //embed.Title = title;
                //embed.Description = description;
                //embed.Color = (int)discordColor;
                //embed.Thumbnail = new EmbedThumbnail { Url = discordWebhook.Avatar };

                //webhook.Embeds.Add(embed);
                //await Server.Delay(0);
                //requestResponse = await webhook.Send();

                //if (requestResponse.status == (System.Net.HttpStatusCode)429)
                //{
                //    long timer = (long)double.Parse(requestResponse.headers["X-RateLimit-Reset-After"]);
                //    DelayTriggered(timer);
                //}

                //if (requestResponse.status != (System.Net.HttpStatusCode)200)
                //{
                //    Log.Info($"SendDiscordEmbededMessage() -> Status: {requestResponse.status} - {requestResponse.content}");
                //}

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

                //Webhook webhook = new Webhook(discordWebhook.Url);

                //webhook.AvatarUrl = discordWebhook.Avatar;
                //webhook.Content = $"`{DateTime.Now.ToString(DATE_FORMAT)}`\n{name}: {message}";
                //webhook.Username = username;

                //await Server.Delay(0);
                //GHMatti.Http.RequestResponse requestResponse = await webhook.Send();

                //if (requestResponse.status == (System.Net.HttpStatusCode)429)
                //{
                //    long timer = (long)double.Parse(requestResponse.headers["X-RateLimit-Reset-After"]);
                //    DelayTriggered(timer);
                //}

                //if (requestResponse.status != (System.Net.HttpStatusCode)200)
                //{
                //    Log.Info($"SendDiscordEmbededMessage() -> Status: {requestResponse.status} - {requestResponse.content}");
                //}

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Log.Error($"SendDiscordSimpleMessage() -> {ex.Message}");
            }
        }
    }
}
