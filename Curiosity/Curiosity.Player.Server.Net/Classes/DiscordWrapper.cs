﻿using CitizenFX.Core.Native;
using Curiosity.Server.net.Enums.Discord;
using Curiosity.Shared.Server.net.Helpers;
using DiscordWebhook;
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

        public static void Init()
        {
            if (Server.isLive)
            {
                server.RegisterEventHandler("curiosity:Server:Discord:ChatMessage", new Action<string, string>(SendDiscordChatMessage));
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

            await SendDiscordSimpleMessage(WebhookChannel.Chat, name, message);
        }

        public static async Task SendDiscordEmbededMessage(WebhookChannel webhookChannel, string name, string title, string description, DiscordColor discordColor)
        {
            try
            {
                Entity.DiscordWebhook discordWebhook = webhooks[webhookChannel];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = $"`{DateTime.Now.ToString(DATE_FORMAT)}`";
                webhook.Username = name;

                Embed embed = new Embed();
                embed.Author = new EmbedAuthor { Name = name, IconUrl = discordWebhook.Avatar };
                embed.Title = title;
                embed.Description = description;
                embed.Color = (int)discordColor;
                embed.Thumbnail = new EmbedThumbnail { Url = discordWebhook.Avatar };

                webhook.Embeds.Add(embed);
                await Server.Delay(0);
                await webhook.Send();
                await Server.Delay(0);
                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Log.Error($"SendDiscordMessage() -> {ex.Message}");
            }
        }

        public static async Task SendDiscordSimpleMessage(WebhookChannel webhookChannel, string name, string message)
        {
            try
            {
                Entity.DiscordWebhook discordWebhook = webhooks[webhookChannel];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = $@"`{DateTime.Now.ToString(DATE_FORMAT)}` {name}: {message}";
                webhook.Username = name;

                await Server.Delay(0);
                await webhook.Send();
                await Server.Delay(0);
                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Log.Error($"SendDiscordSimpleMessage() -> {ex.Message}");
            }
        }
    }
}
