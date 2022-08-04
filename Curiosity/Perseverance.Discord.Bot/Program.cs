global using Newtonsoft.Json;
global using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Perseverance.Discord.Bot.AutomateScripts;
using Perseverance.Discord.Bot.Config;
using Perseverance.Discord.Bot.Entities;
using Perseverance.Discord.Bot.SlashCommands;
using DSharpPlus.Entities;

namespace Perseverance
{
    class Program
    {
        public readonly EventId BotEventId = new EventId(42, "Perseverance-Discord-Bot");
        public static ulong CURIOSITY_BOT_TEXT_CHANNEL { get; private set; }
        public static DiscordClient Client { get; private set; }
        public static Configuration Configuration { get; private set; }

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
                Intents = DiscordIntents.AllUnprivileged,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            });

            if (Configuration.Channels.ContainsKey("error"))
                CURIOSITY_BOT_TEXT_CHANNEL = Configuration.Channels["error"];

            var slash = Client.UseSlashCommands();
            slash.RegisterCommands<BasicCommands>();

            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientErrored;

            await Client.ConnectAsync();

            GameServerStatus gameServerStatus = new GameServerStatus();

            await Task.Delay(-1);
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