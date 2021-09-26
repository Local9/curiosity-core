using Curiosity.LifeV.Bot.Entities.CitizenFX;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot.Methods
{
    class MessageHandlers
    {
        private const long CURIOSITY_BOT_TEXT_CHANNEL = 773248492939247626; // todo: move this some where central
        private static DiscordSocketClient _client;
        private static ulong _guildId;

        static DateTime lastSentTrigger = DateTime.Now.AddMinutes(-10);
        static Dictionary<ulong, DateTime> discordChannel = new Dictionary<ulong, DateTime>();

        static Dictionary<ulong, KeyValuePair<ulong, string>> stickyMessages = new Dictionary<ulong, KeyValuePair<ulong, string>>();

        private Dictionary<string, string> servers = new Dictionary<string, string>()
        {
            { "Worlds", "5.9.0.85:30120" }
        };

        public MessageHandlers(DiscordSocketClient client, ulong guildId)
        {
            _client = client;
            _guildId = guildId;
        }

        public static void AddSticky(ulong channelId, ulong messageId, string message)
        {
            if (stickyMessages.ContainsKey(channelId))
            {
                stickyMessages[channelId] = new KeyValuePair<ulong, string>(messageId, message);
            }
            else
            {
                stickyMessages.Add(channelId, new KeyValuePair<ulong, string>(messageId, message));
            }
        }

        public void AddStickyMessage(ulong channelId, ulong messageId, string message)
        {
            if (stickyMessages.ContainsKey(channelId))
            {
                stickyMessages[channelId] = new KeyValuePair<ulong, string>(messageId, message);
            }
            else
            {
                stickyMessages.Add(channelId, new KeyValuePair<ulong, string>(messageId, message));
            }
        }

        public static ulong RemoveSticky(ulong channelId)
        {
            if (stickyMessages.ContainsKey(channelId))
            {
                KeyValuePair<ulong, string> messageItem = stickyMessages[channelId];
                stickyMessages.Remove(channelId);
                return messageItem.Key;
            }
            return 0;
        }

        public void RemoveStickMessage(ulong channelId)
        {
            stickyMessages.Remove(channelId);
        }

        public async Task StickyMessage(SocketUserMessage message, SocketCommandContext context)
        {
            if (stickyMessages.ContainsKey(message.Channel.Id))
            {
                ulong channelId = message.Channel.Id;
                KeyValuePair<ulong, string> messageItem = stickyMessages[channelId];

                ulong messageId = messageItem.Key;

                await message.Channel.DeleteMessageAsync(messageId);
                var messageResult = await message.Channel.SendMessageAsync($"STICKY\r{messageItem.Value}");
                ulong newMessageId = messageResult.Id;

                AddStickyMessage(channelId, newMessageId, messageItem.Value);
            }
        }

        public async Task HandleAutomatedResponses(SocketUserMessage message, SocketCommandContext context)
        {
            if (message.Content.Contains("bit.ly")
                || message.Content.Contains(".ru/")
                || message.Content.Contains(".link/")
                || message.Content.Contains("ramlucky.xyz")
                || message.Content.Contains("discord.shop")
                || message.Content.Contains("free discord nitro")
                || message.Content.Contains("Free Discord Nitro")
                || message.Content.Contains(".com/airdrop")
                || message.Content.Contains("steam/gifts")
                || message.Content.Contains("Get Discord Nitro")
                || message.Content.Contains("offer is valid")
                )
            {
                await message.DeleteAsync();

                var msg = await context.Channel.SendMessageAsync("Message deleted, possible scam link");

                await Task.Delay(5000);

                await msg.DeleteAsync();

                _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[SCAM LINK] User: {message.Author.Mention} posted a possible scam link and it has been removed.\nRemoved Content;\n```{message.Content}```");
            }

            if (message.Content.Contains("!ip"))
            {
                var msg = await context.Channel.SendMessageAsync("Wish people would just stop joining random Discord Guilds to just type !ip, honestly, do you think we're all the same?! I envy everyone you have never met.");
                
                await Task.Delay(5000);

                await msg.DeleteAsync();
            }
        }

        public async Task HandleCustomResponseMessage(SocketUserMessage message, SocketCommandContext context)
        {
            string messageContent = message.Content.ToLower();
            KeyValuePair<string, string> server = servers.ElementAt(0);

            if (messageContent.Contains("!ip"))
            {
                await context.Channel.SendMessageAsync($"http://connect.lifev.net or `connect {server.Value}`");
            }

            if (messageContent.Contains("server on?") || messageContent.Contains("server up?"))
            {
                try
                {
                    string result = await Tools.HttpTools.GetUrlResultAsync($"http://{server.Value}/players.json");

                    List<CitizenFxPlayers> lst = JsonConvert.DeserializeObject<List<CitizenFxPlayers>>(result);

                    await Tools.HttpTools.GetUrlResultAsync($"http://{server.Value}/info.json");
                    await context.Channel.SendMessageAsync($"Server is online with {lst.Count} players; http://connect.lifev.net");
                }
                catch (Exception ex)
                {
                    await context.Channel.SendMessageAsync("Either the server is currently offline, or there is another issue.");
                }
            }

            if (messageContent.Contains("fivem down?"))
            {
                await context.Channel.SendMessageAsync("Best place to check; https://status.cfx.re/");
            }
        }

        public async Task HandleGunMessage(SocketUserMessage message, SocketCommandContext context)
        {
            if (message.Channel.Id == 599956067686023176) return;

            if (discordChannel.ContainsKey(message.Channel.Id))
            {
                DateTime dateTime = discordChannel[message.Channel.Id];

                if (DateTime.Now.Subtract(dateTime).TotalMinutes < 5) return;
            }

            await Task.Delay(1000);

            var msg = await context.Channel.SendMessageAsync("shush");

            await Task.Delay(10000);

            await msg.DeleteAsync();

            lastSentTrigger = DateTime.Now;

            if (discordChannel.ContainsKey(message.Channel.Id))
            {
                discordChannel[message.Channel.Id] = lastSentTrigger;
            }
            else
            {
                discordChannel.Add(message.Channel.Id, lastSentTrigger);
            }
        }
    }
}
