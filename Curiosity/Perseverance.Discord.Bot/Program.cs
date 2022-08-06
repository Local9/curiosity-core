global using Newtonsoft.Json;
global using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Perseverance.Discord.Bot.AutomateScripts;
using Perseverance.Discord.Bot.Config;
using Perseverance.Discord.Bot.Entities;
using Perseverance.Discord.Bot.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Perseverance.Discord.Bot.Entities.Enums;
using Perseverance.Discord.Bot.Database.Store;

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
            slash.RegisterCommands<UserCommands>();

            Client.Ready += Client_Ready;
            Client.GuildAvailable += Client_GuildAvailable;
            Client.ClientErrored += Client_ClientErrored;
            // Users
            Client.GuildMemberUpdated += Client_GuildMemberUpdated;

            await Client.ConnectAsync();

            GameServerStatus gameServerStatus = new GameServerStatus();

            await Task.Delay(-1);
        }

        private async Task<Task> Client_GuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            if (e.Member.IsBot)
                return Task.CompletedTask;

            List<DiscordRole> rolesAfter = e.RolesAfter.ToList();
            List<DiscordRole> rolesBefore = e.RolesBefore.ToList();

            bool isDonator = false;
            int userRoleId = 1;

            bool isLifeSupporter = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["life"]).Any();
            bool isLevel3 = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["level3"]).Any();
            bool isLevel2 = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["level2"]).Any();
            bool isLevel1 = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["level1"]).Any();

            if (isLifeSupporter)
                userRoleId = (int)Role.DONATOR_LIFE;
            else if (isLevel3)
                userRoleId = (int)Role.DONATOR_LEVEL_3;
            else if (isLevel2)
                userRoleId = (int)Role.DONATOR_LEVEL_2;
            else if (isLevel1)
                userRoleId = (int)Role.DONATOR_LEVEL_1;

            DatabaseUser user = await DatabaseUser.GetAsync(e.Member.Id);

            if (!isDonator)
            {
                // remove DB Role
                user.RemoveRole();
                return Task.CompletedTask;
            }

            // add DB Role
            user.SetRole(userRoleId);
            
            
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