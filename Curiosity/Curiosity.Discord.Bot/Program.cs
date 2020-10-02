using Curiosity.LifeV.Bot.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private TimedEvents _timedEvents;

        private DiscordConfiguration discordConfiguration;

        private ulong guildId;

        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private async Task RunBotAsync()
        {
            var json = "";
            using (var fs = File.OpenRead(@"config\appsettings.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            discordConfiguration = JsonConvert.DeserializeObject<DiscordConfiguration>(json);

            _client = new DiscordSocketClient();
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

            List<string> startupScripts = new List<string>() {
                "Happy Troublesum? happy are we now?",
                "Troublesum wanted me to say something",
                "Don't ping me",
                "I have a million ideas. They all point to certain death.",
                "It hated me because I talked to it.",
                "I’d give you advice, but you wouldn’t listen. No one ever does."
            };
            await _client.SetGameAsync(startupScripts[new Random().Next(startupScripts.Count)]);

            if (ulong.TryParse(guildIdStr, out guildId))
            {
                _timedEvents = new TimedEvents(_client, guildId);
            }

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
                var context = new SocketCommandContext(_client, message);
                if (message is null || message.Author.IsBot) return;

                int argPos = 0;

                if (message.MentionedUsers.Select(u => u).Where(x => x.IsBot && x.Id == _client.CurrentUser.Id).ToList().Count > 0)
                {
                    EmbedBuilder builder = new EmbedBuilder();

                    builder.WithImageUrl("https://cdn.discordapp.com/attachments/138522037181349888/438774275546152960/Ping_Discordapp_GIF-downsized_large.gif");

                    await context.Channel.SendMessageAsync("", false, builder.Build());
                }
                else if (message.HasStringPrefix(discordConfiguration.BotSettings["Prefix"], ref argPos))
                {
                    // will delay for 1 second
                    await Task.Delay(1000);

                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    if (!result.IsSuccess)
                    {
                        if (result.Error == CommandError.UnknownCommand)
                        {
                            await context.Channel.SendMessageAsync("Unknown Command");
                            return;
                        }
                        Console.WriteLine(result.Error);
                        Console.WriteLine(result.ErrorReason);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: EXCEPTION");
                Console.WriteLine($"ERROR: Message causing error '{message.Content}'");
                Console.WriteLine($"ERROR: Message Posted by '{message.Author}'");
                Console.WriteLine(ex);
            }
        }
    }
}
