using Curiosity.Discord.Bot.Models;
using Discord.Commands;
using Discord.WebSocket;
using MySql.Data.MySqlClient;
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
                        else if (user.DiscordId == 0)
                        {
                            await user.RemoveDonatorStatus();
                        }
                        else
                        {
                            ulong discordId = 0;
                            if (ulong.TryParse($"{user.DiscordId}", out discordId))
                            {
                                bool hasDonatorRole = false;

                                if (_client != null)
                                {
                                    SocketGuild socketGuild = _client.GetGuild(_guildId);

                                    if (socketGuild != null)
                                    {
                                        SocketGuildUser socketGuildUser = socketGuild.GetUser(discordId);

                                        if (socketGuildUser != null)
                                        {

                                            IReadOnlyCollection<SocketRole> roles = socketGuildUser.Roles;

                                            if (roles.Count == 0)
                                            {
                                                Console.WriteLine($"[INFO] Discord Donation Checker: No roles found; {discordId}");
                                            }
                                            else
                                            {

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
                                        }
                                        else
                                        {
                                            await user.RemoveDonatorStatus();
                                            Console.WriteLine("[ERROR] Discord Donation Checker: SocketGuildUser is null or no longer apart of the guild");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("[ERROR] Discord Donation Checker: socketGuild is null");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"[ERROR] Discord Donation Checker: Client is null for ID {_guildId}");
                                }
                            }

                            await Task.Delay(2000);
                        }
                    });
                }

                Console.WriteLine("Discord Donation Checker Completed");
            }
            catch (MySqlException mex)
            {
                Console.WriteLine($"[ERROR] OnDiscordDonationChecker -> {mex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] OnDiscordDonationChecker -> {ex}");
            }
        }
    }
}