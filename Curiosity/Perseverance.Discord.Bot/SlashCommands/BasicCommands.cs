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
        
        [SlashCommand("connect", "A button to help connect to the FiveM servers.")]
        public async Task ConnectCommand(InteractionContext ctx, [Option("server", "Server to get information.")] eServerList serverIndex = eServerList.LifeVWorlds)
        {
            // Get server from config
            List<Server> servers = ApplicationConfig.Servers;

            if (servers.Count == 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "No servers found in the config."
                });
                return;
            }

            // get server player information
            Server server = servers[(int)serverIndex];

            try
            {
                await Utils.HttpTools.GetUrlResultAsync($"http://{server.IP}/info.json");
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "Server failed to respond, possible the server is currently offline."
                });
                return;
            }

            DiscordLinkButtonComponent discordButtonComponent = new DiscordLinkButtonComponent(server.Connect, $"Click to join the '{server.Label} FiveM Server'");

            DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder();
            message.AddComponents(discordButtonComponent);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
        }

        [SlashCommand("forums", "A button to connect to the Life V Forums.")]
        public async Task ForumCommand(InteractionContext ctx)
        {
            DiscordLinkButtonComponent discordButtonComponent = new DiscordLinkButtonComponent($"http://forums.lifev.net", $"Click to visit the 'Life V Forums'");

            DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder();
            message.AddComponents(discordButtonComponent);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
        }

        [SlashCommand("gaming", "A button to connect to the Life V Network Gaming Discord.")]
        public async Task GamingCommand(InteractionContext ctx)
        {
            DiscordLinkButtonComponent discordButtonComponent = new DiscordLinkButtonComponent($"http://gaming.lifev.net", $"Click to join the 'Life V Network Gaming Discord'");

            DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder();
            message.AddComponents(discordButtonComponent);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
        }

        [SlashCommand("status", "Get server status")]
        public async Task StatusCommand(InteractionContext ctx, [Option("server", "Server to get information from.")] eServerList serverIndex = eServerList.LifeVWorlds)
        {
            List<Server> servers = ApplicationConfig.Servers;

            if (servers.Count == 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "No servers found in the config."
                });
                return;
            }

            Server server = servers[(int)serverIndex];
            string serverInformation = string.Empty;

            try
            {
                serverInformation = await Utils.HttpTools.GetUrlResultAsync($"http://{server.IP}/info.json");
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "Server failed to respond, possible the server is currently offline."
                });
                return;
            }

            string players = await Utils.HttpTools.GetUrlResultAsync($"http://{server.IP}/players.json");

            List<CitizenFxPlayer> playersList = JsonConvert.DeserializeObject<List<CitizenFxPlayer>>(players);
            CitizenFxInfo info = JsonConvert.DeserializeObject<CitizenFxInfo>(serverInformation);

            // return an embed message
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.Color = DiscordColor.Green;
            embedBuilder.Title = $"{server.Label}";
            embedBuilder.Description = $"Players: {playersList.Count}/{info.Variables["sv_maxClients"]}";
            embedBuilder.AddField("Currently Connected", $"{string.Join(", ", playersList)}", false);
            embedBuilder.WithTimestamp(DateTime.Now);
            embedBuilder.WithFooter("lifev.net");

            await ctx.CreateResponseAsync(embedBuilder);
        }

        public enum Wiki
        {
            Police,
            FireFighter,
            EMT,
            Trucking,
            Piloting,
            BusDriver,
            Fishing,
            Bitcoin,
            DrugDealing,
            Drifting,
            FirstSteps,
            Start,
            GettingStarted,
            Donate,
            Patreon,
            Tebex
        }

        [SlashCommand("wiki", "Information from our wiki @ wiki.lifev.net")]
        public async Task WikiCommand(InteractionContext ctx, [Option("wiki", "Wiki Page.")] Wiki wiki)
        {
            string url = "https://wiki.lifev.net/index.php?title=Guide:";

            switch (wiki)
            {
                case Wiki.Police:
                    url = $"{url}Police";
                    break;
                case Wiki.FireFighter:
                    url = $"{url}Fire_Fighter";
                    break;
                case Wiki.EMT:
                    url = $"{url}EMT";
                    break;
                case Wiki.Trucking:
                    url = $"{url}Trucking";
                    break;
                case Wiki.Piloting:
                    url = $"{url}Piloting";
                    break;
                case Wiki.BusDriver:
                    url = $"{url}Bus_Driver";
                    break;
                case Wiki.Fishing:
                    url = $"{url}Fisher";
                    break;
                case Wiki.Bitcoin:
                    url = $"{url}Bitcoin";
                    break;
                case Wiki.DrugDealing:
                    url = $"{url}Drug_Dealing";
                    break;
                case Wiki.Drifting:
                    url = $"{url}Drifting";
                    break;
                case Wiki.Start:
                case Wiki.FirstSteps:
                case Wiki.GettingStarted:
                    url = $"{url}First_Steps";
                    break;
                case Wiki.Donate:
                case Wiki.Patreon:
                case Wiki.Tebex:
                    url = $"{url}Donate";
                    break;
                default:
                    await ctx.CreateResponseAsync($"'{wiki}' is unknown, please check <https://wiki.lifev.net> to see if the page exists.");
                    return;
            }

            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;

            DiscordLinkButtonComponent discordButtonComponent = new DiscordLinkButtonComponent(url, $"Open Wiki Page for '{textInfo.ToTitleCase(wiki.ToString())}'");

            DiscordInteractionResponseBuilder message = new DiscordInteractionResponseBuilder();
            message.AddComponents(discordButtonComponent);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, message);
        }

    }
}
