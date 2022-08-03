using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Diagnostics;

namespace Perseverance.Discord.Bot.SlashCommands
{
    public class BasicCommands : ApplicationCommandModule
    {
        [SlashCommand("ping", "Do I have to explain this?! Well, what else do you think it does?!")]
        public async Task PingCommand(InteractionContext ctx)
        {
            var timer = new Stopwatch();
            timer.Start();
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            timer.Stop();
            TimeSpan timeSpan = timer.Elapsed;
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"pong! {timeSpan.Milliseconds}ms"));
        }
    }
}
