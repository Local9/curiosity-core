global using DSharpPlus;
global using Newtonsoft.Json;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Perseverance.Discord.Bot.AutomateScripts;
using Perseverance.Discord.Bot.Config;
using Perseverance.Discord.Bot.Entities;
using Perseverance.Discord.Bot.Logic;
using Perseverance.Discord.Bot.SlashCommands;

namespace Perseverance
{
    class Program
    {
        public readonly EventId BotEventId = new EventId(42, "Perseverance-Discord-Bot");
        public static ulong BOT_ERROR_TEXT_CHANNEL { get; private set; }
        public static ulong BOT_GUILD_ID { get; private set; }
        public static DiscordClient Client { get; private set; }
        public static Configuration Configuration { get; private set; }

        static GameServerStatus _gameServerStatus;
        static DonationProcessor _donationProcessor;

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        async Task RunBotAsync()
        {
            Configuration = await ApplicationConfig.GetConfig();

            Client = new(new DiscordConfiguration()
            {
                Token = Configuration.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            });

            if (Configuration.Channels.ContainsKey("error"))
                BOT_ERROR_TEXT_CHANNEL = Configuration.Channels["error"];

            BOT_GUILD_ID = Configuration.Guild;

            var slash = Client.UseSlashCommands();
            slash.RegisterCommands<BasicCommands>();
            slash.RegisterCommands<UserCommands>();
            slash.RegisterCommands<ServerCommands>();

            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientErrored;
            Client.MessageCreated += Client_MessageCreated;
            // Users
            Client.GuildMemberUpdated += Client_GuildMemberUpdated;

            await Client.ConnectAsync();

            _gameServerStatus = new(Client);
            _donationProcessor = new(Client);

            SendMessage(BOT_ERROR_TEXT_CHANNEL, $"Perseverance Discord Bot has started");

            await Task.Delay(-1);
        }

        private Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            // if message contains !ip
            if (e.Message.Content.Contains("!ip"))
            {
                e.Message.RespondAsync("Please use the Slash Command `/connect`");
            }

            if (e.Message.Content.Contains("lv!"))
            {
                e.Message.RespondAsync("Please check out my new slash commands! 🥳");
            }

            return Task.CompletedTask;
        }

        private Task Client_GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            if (e.Member.IsBot)
                return Task.CompletedTask;

            DiscordMemberLogic.UpdateDonationRole(e.Member);

            return Task.CompletedTask;
        }

        private Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, "Perseverance is ready to process events.");
            return Task.CompletedTask;
        }

        private Task Client_GuildAvailable(DiscordClient sender, DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            sender.Logger.LogInformation(BotEventId, $"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }

        private Task Client_ClientErrored(DiscordClient sender, DSharpPlus.EventArgs.ClientErrorEventArgs e)
        {
            sender.Logger.LogError(BotEventId, e.Exception, "Exception occured");
            return Task.CompletedTask;
        }

        public async static void SendMessage(ulong channelId, string message)
        {
            DiscordChannel discordChannel = await Client.GetChannelAsync(channelId);
            discordChannel.SendMessageAsync(message);
        }
    }
}