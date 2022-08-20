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
        
        [SlashCommand("connect", "Information to connect to the main server")]
        public async Task ConnectCommand(InteractionContext ctx)
        {
            List<Server> servers = ApplicationConfig.Servers;
            Server server = servers[0];
            // return an embed message
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.Color = DiscordColor.Green;
            embedBuilder.Title = $"{server.Label}";
            embedBuilder.Description = $"You can connect to the server using the following;";
            embedBuilder.AddField("URL", $"http://connect.lifev.net", false);
            embedBuilder.AddField("Console IP", $"`connect {server.IP}`", false);
            embedBuilder.WithTimestamp(DateTime.Now);
            embedBuilder.WithFooter("lifev.net");

            await ctx.CreateResponseAsync(embedBuilder);
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

            // return an embed message
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.Color = DiscordColor.Green;
            embedBuilder.Title = $"{textInfo.ToTitleCase(wiki)}";
            embedBuilder.Description = $"Please fine below the URL to your query;";
            embedBuilder.AddField("URL", $"{url}", false);
            embedBuilder.WithTimestamp(DateTime.Now);
            embedBuilder.WithFooter("lifev.net");

            await ctx.CreateResponseAsync(embedBuilder);
        }

    }
}
