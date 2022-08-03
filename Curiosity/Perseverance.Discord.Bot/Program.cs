using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Perseverance.Discord.Bot.Config;
using Perseverance.Discord.Bot.Entities;
using Perseverance.Discord.Bot.SlashCommands;

namespace Perseverance
{
    class Program
    {
        public readonly EventId BotEventId = new EventId(42, "Perseverance-Discord-Bot");

        public DiscordClient Client { get; set; }

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.RunBotAsync().GetAwaiter().GetResult();
        }

        async Task RunBotAsync()
        {
            Configuration _config = await ApplicationConfig.GetConfig();

            Client = new(new DiscordConfiguration()
            {
                Token = _config.Token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            });

            var slash = Client.UseSlashCommands();
            slash.RegisterCommands<BasicCommands>();

            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientErrored;

            await Client.ConnectAsync();
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
    }
}