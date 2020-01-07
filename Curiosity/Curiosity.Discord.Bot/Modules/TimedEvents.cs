using Curiosity.Discord.Bot.Models;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Curiosity.Discord.Bot
{
    internal class TimedEvents : ModuleBase<SocketCommandContext>
    {
        static Timer aTimer;
        private static DiscordSocketClient _client;
        private static ulong _guildId;

        public TimedEvents(DiscordSocketClient client, ulong guildId)
        {
            _client = client;
            _guildId = guildId;

            aTimer = new Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnDiscordDonationChecker);
            aTimer.Interval = (1000 * 60) * 60; // EVERY HOUR
            aTimer.Enabled = true;
        }

        private async void OnDiscordDonationChecker(object sender, ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine("Discord Donation Checker Running");

                List<User> donators = await new User().GetUsersWithDonationStatus();

                if (donators.Count > 0)
                {
                    donators.ForEach(async user =>
                    {
                        if (user.DiscordId == null)
                        {
                            await user.RemoveDonatorStatus();
                        }
                        else
                        {
                            ulong discordId = 0;
                            if (ulong.TryParse($"{user.DiscordId}", out discordId))
                            {
                                bool hasDonatorRole = false;
                                IReadOnlyCollection<SocketRole> roles = _client.GetGuild(_guildId).GetUser(discordId).Roles;

                                List<ulong> roleIdList = new List<ulong>();

                                roles.ToList().ForEach(role =>
                                {
                                    roleIdList.Add(role.Id);
                                });

                                hasDonatorRole = roleIdList.Contains(541955570601558036) || roleIdList.Contains(588440994543042560) || roleIdList.Contains(588443443496222720) || roleIdList.Contains(588444129722105856);

                                if (hasDonatorRole)
                                {
                                    await user.AddDonatorStatus();
                                }
                                else
                                {
                                    await user.RemoveDonatorStatus();
                                }
                            }

                            await Task.Delay(2000);
                        }
                    });
                }

                Console.WriteLine("Discord Donation Checker Completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnDiscordDonationChecker -> {ex}");
            }
        }
    }
}