using Curiosity.LifeV.Bot.Entities;
using Curiosity.LifeV.Bot.EventHandler;
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

        private GuildMemberUpdated _guildMemberUpdated;

        private DiscordConfiguration discordConfiguration;

        private ulong guildId;

        DateTime lastSentTrigger = DateTime.Now.AddMinutes(-10);

        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private async Task RunBotAsync()
        {
            var json = "";
            using (var fs = File.OpenRead(@"config\appsettings.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            discordConfiguration = JsonConvert.DeserializeObject<DiscordConfiguration>(json);

            //DiscordSocketConfig discordSocketConfig = new DiscordSocketConfig()
            //{
            //    TotalShards = 1,
            //    MessageCacheSize = 0,
            //    ExclusiveBulkDelete = true,
            //    AlwaysDownloadUsers = true,
            //    LogLevel = LogSeverity.Info,
            //    GatewayIntents =
            //        GatewayIntents.Guilds |
            //        GatewayIntents.GuildMembers |
            //        GatewayIntents.GuildMessageReactions |
            //        GatewayIntents.GuildMessages |
            //        GatewayIntents.GuildVoiceStates
            //};

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
            await _client.SetGameAsync("Starting up...");

            if (ulong.TryParse(guildIdStr, out guildId))
            {
                _timedEvents = new TimedEvents(_client, guildId);

                GuildMemberUpdated._client = _client;
                GuildMemberUpdated._guildId = guildId;
            }

            _client.GuildMemberUpdated += GuildMemberUpdated.Handle;

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
                    EmbedBuilder builder = new EmbedBuilder();

                    builder.WithImageUrl("https://cdn.discordapp.com/attachments/138522037181349888/438774275546152960/Ping_Discordapp_GIF-downsized_large.gif");

                    await context.Channel.SendMessageAsync(embed: builder.Build());
                }
                else if (message.Content.Contains("guns") || message.Content.Contains("weapons") || message.Content.Contains("weapon") || message.Content.Contains("gun"))
                {
                    if (message.Channel.Id == 599956067686023176) return;

                    if (DateTime.Now.Subtract(lastSentTrigger).TotalMinutes < 5) return;

                    await Task.Delay(1000);
                    await context.Channel.SendMessageAsync("Weapons are currently in refactoring with the new framework, this is to allow you to purchase licenses for weapons and earn components. There is no E.T.A. as to when this will be completed as ::1 is also trying to support OneSync and the Casino or latest updates to GTA.");

                    lastSentTrigger = DateTime.Now;
                    return;
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
                Console.WriteLine($"ERROR: Message causing error '{message?.Content}'");
                Console.WriteLine($"ERROR: Message Posted by '{message?.Author}'");
                Console.WriteLine(ex);
            }
        }
    }
}
