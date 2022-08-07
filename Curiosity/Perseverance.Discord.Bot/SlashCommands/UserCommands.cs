using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Perseverance.Discord.Bot.Database.Store;
using Perseverance.Discord.Bot.Logic;

namespace Perseverance.Discord.Bot.SlashCommands
{
    [SlashCommandGroup("user", "User related commands.")]
    public class UserCommands : ApplicationCommandModule
    {
        [SlashCommand("whois", "Get information about a user")]
        public async Task WhoIsCommand(InteractionContext ctx, [Option("user", "User to lookup")] DiscordUser user)
        {
            DatabaseUser databaseUser = await Database.Store.DatabaseUser.GetAsync(user.Id);
            string message = $"User '{user.Username}' not found.";

            if (databaseUser is not null)
                message = $"{databaseUser}";

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
            {
                Content = message
            });
        }


        [SlashCommand("donate", "Checks the users donation status and messages them.")]
        public async Task DonateCommand(InteractionContext ctx, [Option("user", "User to lookup")] DiscordUser user)
        {
            DiscordMember member = await ctx.Guild.GetMemberAsync(user.Id);
            DiscordMemberLogic.UpdateDonationRole(member);
        }
    }
}
