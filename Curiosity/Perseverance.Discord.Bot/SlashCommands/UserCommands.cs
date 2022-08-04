using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Perseverance.Discord.Bot.Database.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Perseverance.Discord.Bot.SlashCommands
{
    public class UserCommands : ApplicationCommandModule
    {
        [SlashCommand("whois", "Get information about a user")]
        public async Task PingCommand(InteractionContext ctx, [Option("user", "User to lookup")] DiscordUser user)
        {
            DatabaseUser databaseUser = await Database.Store.DatabaseUser.GetUserAsync(user.Id);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
            {
                Content = $"{databaseUser}"
            });
        }
    }
}
