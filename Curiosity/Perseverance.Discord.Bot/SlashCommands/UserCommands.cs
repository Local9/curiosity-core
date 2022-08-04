using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Perseverance.Discord.Bot.Database.Store;

namespace Perseverance.Discord.Bot.SlashCommands
{
    [SlashCommandGroup("user", "User related commands.")]
    public class UserCommands : ApplicationCommandModule
    {
        [SlashCommand("whois", "Get information about a user")]
        public async Task PingCommand(InteractionContext ctx, [Option("user", "User to lookup")] DiscordUser user)
        {
            DatabaseUser databaseUser = await Database.Store.DatabaseUser.GetUserAsync(user.Id);
            string message = $"User '{user.Username}' not found.";

            if (databaseUser is not null)
                message = $"{databaseUser}";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
            {
                Content = message
            });
        }
    }
}
