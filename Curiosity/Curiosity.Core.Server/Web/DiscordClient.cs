﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Managers;
using Curiosity.Core.Server.Web.Discord.Entity;
using Curiosity.Systems.Library.Models.Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Web
{
    public enum WebhookChannel
    {
        Chat = 1,
        Report,
        StaffLog,
        ServerLog,
        ServerErrors,
        PlayerLog,
        ServerEventLog
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
        static Request request = new Request();
        public static DiscordClient DiscordInstance;
        static Dictionary<WebhookChannel, DiscordWebhook> webhooks = new Dictionary<WebhookChannel, DiscordWebhook>();
        static DateTime lastUpdate = DateTime.Now;
        static string DATE_FORMAT = "yyyy-MM-dd HH:mm";
        static bool IsDelayRunnning = false;
        static long DelayMillis = 0;

        private static Regex _compiledUnicodeRegex = new Regex(@"[^\u0000-\u007F]", RegexOptions.Compiled);

        public override void Begin()
        {
            DiscordInstance = this;
        }

        public static String StripUnicodeCharactersFromString(string inputValue)
        {
            return _compiledUnicodeRegex.Replace(inputValue, String.Empty);
        }

        [TickHandler]
        private async void OnDiscordWebhookUpdate()
        {
            if (DateTime.Now.Subtract(lastUpdate).TotalSeconds > 120)
            {
                // update every 2 minutes
                lastUpdate = DateTime.Now;
                UpdateWebhooks();
            }

            while (webhooks.Count == 0)
            {
                // init
                UpdateWebhooks();
                await BaseScript.Delay(1000);
                if (webhooks.Count == 0)
                {
                    Logger.Error($"No Discord Webhooks returned, trying again in five seconds.");
                    await BaseScript.Delay(5000);
                }
            }

            await BaseScript.Delay(10000);
        }

        private static async Task UpdateWebhooks()
        {
            if (PluginManager.ServerId > 0)
            {
                webhooks = await Database.Store.ServerDatabase.GetDiscordWebhooks(PluginManager.ServerId);
            }
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
                player.Drop($"Discord Identity failed validation, please restart FiveM and Discord. After you have opened Discord, then open FiveM.\n\nThese must be running on the same machine.");
                return IsMember;
            }

            if (discordId == 0)
            {
                Logger.Info($"DiscordClient : {player.Name} Discord Information was invalid.");
                player.Drop($"Discord Identity failed validation, please restart FiveM and Discord. After you have opened Discord, then open FiveM.\n\nThese must be running on the same machine.");
                return IsMember;
            }

            RequestResponse requestResponse = await DiscordRequest("GET", $"guilds/{PluginManager.DiscordGuildId}/members/{discordId}", string.Empty);

            if (requestResponse.status == System.Net.HttpStatusCode.NotFound)
            {
                Logger.Info($"DiscordClient : {player.Name} is NOT a member of the Discord Guild.");
                player.Drop($"This server requires you to be a member of their Discord and Verified.\n\nDiscord URL: {PluginManager.DiscordUrl}");
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
                    Logger.Debug($"DiscordClient : {player.Name} is a verified member of the Discord Guild.");
                    return true;
                }
                else
                {
                    Logger.Debug($"DiscordClient : {player.Name} is not a verified member of the Discord Guild.");
                    player.Drop($"This server requires you to be a member of their Discord and Verified.\n\nDiscord URL: {PluginManager.DiscordUrl}");
                    return false;
                }
            }

            IsMember = discordMember.JoinedAt.HasValue;

            Logger.Success($"DiscordClient : {player.Name} is a member of the Discord Guild.");

            return IsMember;
        }

        public async void SendChatMessage(string name, string message)
        {
            if (!webhooks.ContainsKey(WebhookChannel.Chat))
            {
                Logger.Warn($"SendDiscordChatMessage() -> Discord Chat Webhook Missing");
                return;
            }

            await SendDiscordSimpleMessage(WebhookChannel.Chat, PluginManager.Hostname, name, message);
        }

        public async void SendDiscordServerEventLogMessage(string message)
        {
            if (!webhooks.ContainsKey(WebhookChannel.ServerEventLog))
            {
                Logger.Warn($"SendDiscordChatMessage() -> Discord Server Event Webhook Missing");
                return;
            }

            try
            {
                DiscordWebhook discordWebhook = webhooks[WebhookChannel.ServerEventLog];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = StripUnicodeCharactersFromString($"`{DateTime.Now.ToString(DATE_FORMAT)}`: {message}");
                webhook.Username = StripUnicodeCharactersFromString(PluginManager.Hostname);

                await webhook.Send();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Logger.Error($"SendDiscordPlayerLogMessage() -> {ex.Message}");
            }

        }

        public async void SendDiscordPlayerLogMessage(string message)
        {
            if (!webhooks.ContainsKey(WebhookChannel.PlayerLog))
            {
                Logger.Warn($"SendDiscordChatMessage() -> Discord Player Log Webhook Missing");
                return;
            }

            try
            {
                DiscordWebhook discordWebhook = webhooks[WebhookChannel.PlayerLog];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = StripUnicodeCharactersFromString($"`{DateTime.Now.ToString(DATE_FORMAT)}`: {message}");
                webhook.Username = StripUnicodeCharactersFromString(PluginManager.Hostname);

                await webhook.Send();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Logger.Error($"SendDiscordPlayerLogMessage() -> {ex.Message}");
            }

        }

        public async void SendDiscordStaffMessage(string adminName, string player, string action, string reason, string duration)
        {
            try
            {
                if (!webhooks.ContainsKey(WebhookChannel.StaffLog))
                {
                    Logger.Warn($"SendDiscordStaffMessage() -> Discord {WebhookChannel.StaffLog} Webhook Missing");
                    return;
                }

                if (IsDelayRunnning) return;

                DiscordWebhook discordWebhook = webhooks[WebhookChannel.StaffLog];

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
                await BaseScript.Delay(0);
                await webhook.Send();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Logger.Error($"SendDiscordStaffMessage() -> {ex.Message}");
            }
        }

        public async void SendDiscordReportMessage(string reporterName, string playerBeingReported, string reason)
        {
            try
            {
                if (!webhooks.ContainsKey(WebhookChannel.Report))
                {
                    Logger.Warn($"SendDiscordReportMessage() -> Discord {WebhookChannel.Report} Webhook Missing");
                    return;
                }

                if (IsDelayRunnning) return;

                DiscordWebhook discordWebhook = webhooks[WebhookChannel.Report];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = $"`{DateTime.Now.ToString(DATE_FORMAT)}`";
                string cleanName = StripUnicodeCharactersFromString(PluginManager.Hostname);
                webhook.Username = cleanName;

                Embed embed = new Embed();
                embed.Author = new EmbedAuthor { Name = $"Report By: {reporterName}", IconUrl = discordWebhook.Avatar };
                embed.Title = $"Report";
                embed.Description = $"Player: {playerBeingReported}\nReason: {reason}";
                embed.Color = (int)DiscordColor.Blue;
                embed.Thumbnail = new EmbedThumbnail { Url = discordWebhook.Avatar };

                webhook.Embeds.Add(embed);
                await BaseScript.Delay(0);
                await webhook.Send();
            }
            catch (Exception ex)
            {
                Logger.Error($"SendDiscordReportMessage() -> {ex.Message}");
            }
        }

        public async Task SendDiscordEmbededMessage(WebhookChannel webhookChannel, string name, string title, string description, DiscordColor discordColor)
        {
            try
            {
                if (!webhooks.ContainsKey(webhookChannel))
                {
                    Logger.Warn($"SendDiscordEmbededMessage() -> Discord {webhookChannel} Webhook Missing");
                    return;
                }

                if (IsDelayRunnning) return;

                string cleanName = StripUnicodeCharactersFromString(name);

                DiscordWebhook discordWebhook = webhooks[webhookChannel];

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
                await BaseScript.Delay(0);
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
                DiscordWebhook discordWebhook = webhooks[webhookChannel];

                Webhook webhook = new Webhook(discordWebhook.Url);

                webhook.AvatarUrl = discordWebhook.Avatar;
                webhook.Content = StripUnicodeCharactersFromString($"{name} > {message}");
                webhook.Username = StripUnicodeCharactersFromString(username);

                await webhook.Send();

                await Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Logger.Error($"SendDiscordSimpleMessage() -> {ex.Message}");
            }
        }

        async void DelayTriggered(long delayMillis)
        {
            DelayMillis = delayMillis * 1000;
        }

        async Task OnDelayCooldownTask()
        {
            long gameTimer = API.GetGameTimer();
            IsDelayRunnning = true;

            while ((API.GetGameTimer() - gameTimer) < DelayMillis)
            {
                await BaseScript.Delay(1000);
            }

            PluginManager.Instance.DetachTickHandler(OnDelayCooldownTask);
            IsDelayRunnning = false;
        }
    }
}
