using Curiosity.LifeV.Bot.Entities.Curiosity;
using Curiosity.LifeV.Bot.Tools;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot.EventHandler
{
    class GuildMemberUpdated
    {
        private const long CURIOSITY_BOT_TEXT_CHANNEL = 773248492939247626;
        public static DiscordSocketClient _client;
        public static ulong _guildId;

        private static int _errorCount = 0;

        public static object StringValue { get; private set; }

        public static async Task Handle(SocketGuildUser before, SocketGuildUser after)
        {
            try
            {
                if (before.Roles.Count != after.Roles.Count)
                {
                    Models.User dbUser = await new Models.User().FindUserAsync(after.Id);

                    if (dbUser == null) return;

                    List<ulong> newUserRoles = new List<ulong>();

                    after.Roles.ToList().ForEach(role =>
                    {
                        newUserRoles.Add(role.Id);
                    });

                    bool hasDonatorRole = newUserRoles.Contains(541955570601558036) // LIFE
                        || newUserRoles.Contains(588440994543042560) // L2
                        || newUserRoles.Contains(588443443496222720) // L1
                        || newUserRoles.Contains(588444129722105856); // L3

                    Role donatorRole = Role.USER;

                    if (newUserRoles.Contains(541955570601558036))
                    {
                        donatorRole = Role.DONATOR_LIFE;
                    }

                    if (newUserRoles.Contains(588443443496222720)) // Lv1
                    {
                        donatorRole = Role.DONATOR_LEVEL_1;
                    }

                    if (newUserRoles.Contains(588440994543042560)) // Lv2
                    {
                        donatorRole = Role.DONATOR_LEVEL_2;
                    }

                    if (newUserRoles.Contains(588444129722105856)) // Lv3
                    {
                        donatorRole = Role.DONATOR_LEVEL_3;
                    }

                    if (dbUser.UserRole == donatorRole) return; // No change, feck off

                    if (hasDonatorRole)
                    {
                        await dbUser.AddDonatorStatus((int)donatorRole);
                    }
                    else
                    {
                        await dbUser.RemoveDonatorStatus();
                    }

                    EmbedBuilder builder = new EmbedBuilder();

                    builder.AddField("Player", $"{dbUser.Username}#{dbUser.UserId}");

                    if (hasDonatorRole)
                    {
                        builder.AddField("Supporter Level", $"{StringValueAttribute.GetStringValue(donatorRole)}");
                    }
                    else
                    {
                        builder.AddField("Supporter Level", $"We're sorry to see you go, if there is anything you'd like use to improve please let us know on our forums. We hope to see you return.");
                    }

                    builder.WithColor(hasDonatorRole ? Color.Green : Color.Blue)
                        .WithCurrentTimestamp()
                        .WithFooter("Forums: https://forums.lifev.net");

                    SocketGuildUser user = _client.GetGuild(_guildId).GetUser(after.Id);
                    if (user != null)
                    {
                        user.SendMessageAsync(embed: builder.Build());
                        _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm")}] User {user.Username}, Role Change {donatorRole}, DiscordID: {user.Id}");
                    }
                }
                
            }
            catch (Exception ex)
            {
                _errorCount++;

                if (_errorCount >= 3) return;

                EmbedBuilder builder = new EmbedBuilder();

                builder
                    .AddField("Message", $"{ex.Message}", false)
                    .AddField("Stack", $"{ex}", false)
                    .WithColor(Color.Red)
                    .WithCurrentTimestamp();

                _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync(embed: builder.Build());
            }
        }
    }
}
