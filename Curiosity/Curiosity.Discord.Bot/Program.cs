using Curiosity.LifeV.Bot.Entities;
using Curiosity.LifeV.Bot.EventHandler;
using Curiosity.LifeV.Bot.Methods;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot
{
    class Program
    {
        Regex urlRE = new Regex("([a-zA-Z0-9]+://)?([a-zA-Z0-9_]+:[a-zA-Z0-9_]+@)?([a-zA-Z0-9.-]+\\.[A-Za-z]{2,4})(:[0-9]+)?([^ ])+");

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private TimedEvents _timedEvents;

        private GuildMemberUpdated _guildMemberUpdated;

        private DiscordConfiguration discordConfiguration;

        private MessageHandlers messageHandlers;

        private ulong guildId;
        Random Random = new Random();

        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private async Task RunBotAsync()
        {
            var json = "";
            using (var fs = File.OpenRead(@"config\appsettings.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            discordConfiguration = JsonConvert.DeserializeObject<DiscordConfiguration>(json);

            DiscordSocketConfig discordSocketConfig = new DiscordSocketConfig()
            {
                TotalShards = 1,
                MessageCacheSize = 0,
                AlwaysDownloadUsers = true,
                LogLevel = LogSeverity.Info,
                GatewayIntents = GatewayIntents.All
            };

            _client = new DiscordSocketClient(discordSocketConfig);

            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            _client.Log += Client_Log;

            await RegisterCommandsAsync();

            bool testing = Convert.ToBoolean(discordConfiguration.BotSettings["Testing"]);

            string discordToken = testing ? discordConfiguration.BotSettings["TokenBeta"] : discordConfiguration.BotSettings["TokenLive"];
            string guildIdStr = testing ? discordConfiguration.BotSettings["GuildBeta"] : discordConfiguration.BotSettings["GuildLive"];

            await _client.LoginAsync(TokenType.Bot, discordToken, true);

            await _client.StartAsync();

            await _client.SetStatusAsync(UserStatus.Online);
            await _client.SetGameAsync("Starting up...");

            if (ulong.TryParse(guildIdStr, out guildId))
            {
                _timedEvents = new TimedEvents(_client, guildId);

                GuildMemberUpdated._client = _client;
                GuildMemberUpdated._guildId = guildId;
            }

            _client.GuildMemberUpdated += GuildMemberUpdated.Handle;

            messageHandlers = new MessageHandlers(_client, guildId);

            await Task.Delay(-1);
        }

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            try
            {
                if (message is null || message.Author.IsBot) return;
                var context = new SocketCommandContext(_client, message);

                int argPos = 0;

                if (message.MentionedUsers.Select(u => u).Where(x => x.IsBot && x.Id == _client.CurrentUser.Id).ToList().Count > 0)
                {
                    PingHandler.ReactToMention(message, context);
                }

                // messageHandlers.DeleteAnythingWithTheWordNitro(message, context);

                messageHandlers.HandleCustomResponseMessage(message, context);

                if (message.HasStringPrefix(discordConfiguration.BotSettings["Prefix"], ref argPos))
                {
                    // will delay for 1 second
                    await Task.Delay(1000);

                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess)
                    {
                        if (result.Error == CommandError.UnknownCommand)
                        {
                            var msg = await context.Channel.SendMessageAsync("Unknown Command");

                            await Task.Delay(5000);

                            await msg.DeleteAsync();
                        }
                    }
                }

                messageHandlers.StickyMessage(message, context);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: EXCEPTION");
                Console.WriteLine($"ERROR: Message causing error '{message?.Content}'");
                Console.WriteLine($"ERROR: Message Posted by '{message?.Author}'");
                Console.WriteLine(ex);
            }
        }
    }
}
