using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Perseverance.Discord.Bot.Config;
using Perseverance.Discord.Bot.Entities;
using System.Diagnostics;
using System.Globalization;

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
        
        [SlashCommand("connect", "A button to connect to the Life V Worlds server.")]
        public async Task ConnectCommand(InteractionContext ctx)
        {
            DiscordLinkButtonComponent discordButtonComponent = new DiscordLinkButtonComponent($"http://connect.lifev.net", $"Click to join the 'Life V Worlds Server'");

            DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder();
            message.AddComponents(discordButtonComponent);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
        }

        [SlashCommand("wiki", "Information from our wiki @ wiki.lifev.net")]
        public async Task WikiCommand(InteractionContext ctx, [Option("wiki", "Wiki Page.")] string wiki)
        {
            string url = "https://wiki.lifev.net/index.php?title=Guide:";

            switch (wiki.ToLower())
            {
                case "police":
                    url = $"{url}Police";
                    break;
                case "firefighter":
                    url = $"{url}Fire_Fighter";
                    break;
                case "emt":
                    url = $"{url}EMT";
                    break;
                case "trucking":
                case "truck":
                    url = $"{url}Trucking";
                    break;
                case "piloting":
                case "pilot":
                    url = $"{url}Piloting";
                    break;
                case "bus":
                case "busdriver":
                    url = $"{url}Bus_Driver";
                    break;
                case "fisher":
                case "fishing":
                    url = $"{url}Fisher";
                    break;
                case "bitcoin":
                    url = $"{url}Bitcoin";
                    break;
                case "drugdealing":
                    url = $"{url}Drug_Dealing";
                    break;
                case "drifting":
                    url = $"{url}Drifting";
                    break;
                case "firststeps":
                    url = $"{url}First_Steps";
                    break;
                default:
                    await ctx.CreateResponseAsync($"'{wiki}' is unknown, please check <https://wiki.lifev.net> to see if the page exists.");
                    return;
            }

            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;

            DiscordLinkButtonComponent discordButtonComponent = new DiscordLinkButtonComponent(url, $"Open Wiki Page for '{textInfo.ToTitleCase(wiki)}'");

            DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder();
            message.AddComponents(discordButtonComponent);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
        }

    }
}
