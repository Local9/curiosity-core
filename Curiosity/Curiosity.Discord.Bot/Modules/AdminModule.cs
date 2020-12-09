using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot.Modules
{
    [Group("admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        // lv!admin user <userId>
        [Command("user")]
        [Summary("Return information from Curiosity")]
        [Alias("u", "whois")]
        public async Task GetUser([Summary("Curiosity UserId")] long? userId = null)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;

            var user = Context.User;

            // Permission Check
            Models.User dbAdminUser = await new Models.User().FindUserAsync(user.Id);

            if (!dbAdminUser.IsStaff)
            {
                await Context.Channel.SendMessageAsync($"Admin permission required");
                return;
            }

            // User

            if (userId == null)
            {
                await Context.Channel.SendMessageAsync($"UserId must be supplied");
                return;
            }

            long uId = (long)userId;

            Models.User dbUser = await new Models.User().FindUserByCuriosityUserId(uId);

            if (dbUser == null)
            {
                await ReplyAsync("User was not found.");
            }
            else
            {
                await Context.Message.DeleteAsync();

                EmbedBuilder builder = new EmbedBuilder();

                string discordId = (dbUser.DiscordId == null || dbUser.DiscordId == 0) ? $"Unknown" : $"{dbUser.DiscordId}";

                builder
                    .AddField("Player", $"{dbUser.Username}", true)
                    .AddField("Role", $"{textInfo.ToTitleCase(dbUser.UserRole.ToString())}", true)
                    .AddField("Experience", $"{dbUser.LifeExperience:#,###,###}")
                    .AddField("Server: First Joined", $"{dbUser.DateCreated.ToString("yyyy-MM-dd HH:mm")}", true)
                    .AddField("Server: Last Seen", $"{dbUser.LastSeen.ToString("yyyy-MM-dd HH:mm")}", true)
                    .AddField("License", $"{dbUser.License}", false)
                    .AddField("Stored DiscordID", $"{discordId}", false)
                    .AddField("Perm Banned", $"{dbUser.BannedPerm}", true)

                    .WithColor(Color.Blue)
                        .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                        .WithCurrentTimestamp()
                        .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

                if (dbUser.BannedUntil != null)
                {
                    builder.AddField("Banned Until", $"{dbUser.BannedUntil?.ToString("yyyy-MM-dd HH:mm")}", true);
                }

                if (dbUser.DiscordId != null && dbUser.DiscordId > 0)
                {

                    builder.AddField("Discord Information", "Below is information on the Discord Member.", false);
                    SocketGuildUser guildUser = Context.Guild.Users.Select(x => x).Where(y => y.Id == dbUser.DiscordId).FirstOrDefault();

                    if (guildUser != null)
                    {
                        if (guildUser.JoinedAt.HasValue)
                        {
                            builder.AddField("Joined", $"{guildUser.JoinedAt.Value.ToString("yyyy-MM-dd HH:mm")}", true);
                        }

                        if (guildUser.PremiumSince.HasValue)
                        {
                            builder.AddField("Boosted", $"{guildUser.PremiumSince.Value.ToString("yyyy-MM-dd HH:mm")}", true);
                        }

                        builder.AddField("Status", $"{guildUser.Status}");
                    }
                    else
                    {
                        builder.AddField("Discord Presence", "User has left this guild.", true);
                    }
                }

                Embed embed = builder.Build();

                await ReplyAsync("", false, embed);
            }
        }// lv!admin user <userId>
        [Command("log")]
        [Summary("Return users log from Curiosity")]
        [Alias("l", "history")]
        public async Task GetUserLog([Summary("Curiosity UserId")] long? userId = null)
        {
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            TextInfo textInfo = cultureInfo.TextInfo;

            var user = Context.User;

            // Permission Check
            Models.User dbAdminUser = await new Models.User().FindUserAsync(user.Id);

            if (!dbAdminUser.IsStaff)
            {
                await Context.Channel.SendMessageAsync($"Admin permission required");
                return;
            }

            // User

            if (userId == null)
            {
                await Context.Channel.SendMessageAsync($"UserID must be supplied");
                return;
            }

            long uId = (long)userId;

            List<Models.Log> dbUserLog = await new Models.Log().GetUserLogAsync(uId);

            if (dbUserLog == null)
            {
                await ReplyAsync("User was not found.");
            }
            else
            {
                await Context.Message.DeleteAsync();

                string message = "Last 10 Logged Results" +
                    "```" +
                    "\n+------------------+-----------------------------------------------+-----------------------------------+" +
                    "\n| Timestamp        | Details                                       | By                                |" +
                    "\n+------------------+-----------------------------------------------+-----------------------------------+";

                dbUserLog.ForEach(log =>
                {
                    message += "\n| ";
                    message += log.Timestamp.ToString("yyyy-MM-dd HH:mm").PadRight(16, ' ');
                    message += " | ";
                    message += log.Details.PadRight(45, ' ');
                    message += " | ";
                    message += log.LoggedBy.PadRight(33, ' ');
                    message += " |";
                });

                message += "\n+------------------+-----------------------------------------------+-----------------------------------+" +
                    "\n```";

                await ReplyAsync(message, false);
            }
        }

        //public class CleanModule : ModuleBase<SocketCommandContext>
        //{
        //    // lv!admin user <userId>
        //    [Command]
        //    [Summary("Clear the channel of messages")]
        //    [RequireUserPermission(Discord.ChannelPermission.ManageChannels)]
        //    public async Task DefaultCleanAsync()
        //    {

        //    }

        //    // lv!admin user <userId>
        //    [Command("messages")]
        //    [Summary("Clear the channel of messages")]
        //    [RequireUserPermission(Discord.ChannelPermission.ManageChannels)]
        //    public async Task CleanAsync([Summary("Number of messages to remove")] int count)
        //    {

        //    }
        //}
    }
}
