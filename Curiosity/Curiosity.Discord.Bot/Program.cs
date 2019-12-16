using Curiosity.Discord.Bot.Entities;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Linq;
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

            
            bool testing = Convert.ToBoolean(discordConfiguration.BotSettings["Testing"]);

            string discordToken = testing ? discordConfiguration.BotSettings["TokenBeta"] : discordConfiguration.BotSettings["TokenLive"];

            await _client.LoginAsync(TokenType.Bot, discordToken, true);

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
    }
}
