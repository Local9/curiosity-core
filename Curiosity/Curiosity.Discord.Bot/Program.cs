using Curiosity.Discord.Bot.Database;
using Curiosity.Discord.Bot.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Discord.Bot
{
    class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        private DiscordConfiguration discordConfiguration;

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

            await _client.LoginAsync(TokenType.Bot, discordConfiguration.BotSettings["Token"], true);

            await _client.StartAsync();

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
            var context = new SocketCommandContext(_client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if (message.HasStringPrefix(discordConfiguration.BotSettings["Prefix"], ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                if (!result.IsSuccess)
                {
                    if (result.Error == CommandError.UnknownCommand)
                    {
                        await context.Channel.SendMessageAsync("Unknown Command");
                        return;
                    }
                    Console.WriteLine(result.Error);
                }
            }
        }
    }
}
