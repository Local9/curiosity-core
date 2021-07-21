﻿using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
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

        public MessageHandlers(DiscordSocketClient client, ulong guildId)
        {
            _client = client;
            _guildId = guildId;
        }

        public async Task HandleUrl(SocketUserMessage message, SocketCommandContext context)
        {
            //if (message.Content.Contains("cfx.re"))
            //    return;

            //if (message.Content.Contains("fivem.net"))
            //    return;

            //if (message.Content.Contains("lifev.net"))
            //    return;

            //if (message.Content.Contains("giphy.com"))
            //    return;

            //if (message.Content.Contains("tenor.com"))
            //    return;

            //if (message.Content.Contains("twitch.tv"))
            //    return;

            if (message.Content.Contains("bit.ly") || message.Content.Contains(".ru/") || message.Content.Contains("ramlucky.xyz"))
            {
                await message.DeleteAsync();

                var msg = await context.Channel.SendMessageAsync("Message deleted, possible scam link");

                await Task.Delay(5000);

                await msg.DeleteAsync();

                _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[SCAM LINK] User: {message.Author.Mention} posted a possible scam link and it has been removed.\nRemoved Content;\n```{message.Content}```");
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

            var msg = await context.Channel.SendMessageAsync("Weapons are currently in refactoring with the new framework, this is to allow you to purchase licenses for weapons and earn components. There is no E.T.A. as to when this will be completed as ::1 is also trying to support OneSync and the Casino or latest updates to GTA.");

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
