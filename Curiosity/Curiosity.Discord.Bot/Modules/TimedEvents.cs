using Curiosity.LifeV.Bot.Entities.CitizenFX;
using Curiosity.LifeV.Bot.Entities.Curiosity;
using Curiosity.LifeV.Bot.Models;
using Discord.Commands;
using Discord.WebSocket;
using MySqlConnector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using System.Timers;

namespace Curiosity.LifeV.Bot
{
    internal class TimedEvents : ModuleBase<SocketCommandContext>
    {
        private const long CURIOSITY_BOT_TEXT_CHANNEL = 773248492939247626;
        static Timer donationTimer;
        static Timer statusUpdater;

        private static DiscordSocketClient _client;
        private static ulong _guildId;

        public TimedEvents(DiscordSocketClient client, ulong guildId)
        {
            _client = client;
            _guildId = guildId;

            donationTimer = new Timer();
            donationTimer.Elapsed += new ElapsedEventHandler(OnDiscordDonationChecker);
            donationTimer.Interval = (1000 * 60) * 60; // EVERY HOUR
            donationTimer.Enabled = true;

            statusUpdater = new Timer();
            statusUpdater.Elapsed += new ElapsedEventHandler(OnStatusUpdater);
            statusUpdater.Interval = (1000 * 30); // EVERY 30 Seconds
            statusUpdater.Enabled = true;
        }

        private async void OnStatusUpdater(object sender, ElapsedEventArgs e)
        {
            string serverInformation = await Tools.HttpTools.GetUrlResultAsync($"http://5.9.0.85:30120/info.json");

            if (string.IsNullOrEmpty(serverInformation))
            {
                _client.SetGameAsync($"Server Offline");
                return;
            }

            string result = await Tools.HttpTools.GetUrlResultAsync($"http://5.9.0.85:30120/players.json");

            List<CitizenFxPlayers> lst = JsonConvert.DeserializeObject<List<CitizenFxPlayers>>(result);
            CitizenFxInfo info = JsonConvert.DeserializeObject<CitizenFxInfo>(serverInformation);

            _client.SetGameAsync($"P: {lst.Count}/32 | UT: {info.Variables["Uptime"]}");
        }

        private async void OnDiscordDonationChecker(object sender, ElapsedEventArgs e)
        {
            try
            {
                Console.WriteLine("Discord Donation Checker Running");

                List<User> donators = await new User().GetUsersWithDonationStatus();

                if (donators.Count > 0)
                {
                    if (_client != null)
                    {
                        SocketGuild socketGuild = _client.GetGuild(_guildId);

                        if (socketGuild == null)
                        {
                            Console.WriteLine("[ERROR] Guild was unable to be found");
                            return;
                        }

                        donators.ForEach(async user =>
                        {
                            int donatorRoleId = 1;
                            ulong discordId = 0;
                            if (ulong.TryParse($"{user.DiscordId}", out discordId))
                            {
                                if (discordId == 0)
                                {
                                    _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[DONATION] User: {user.Username}#{user.UserId} invalid discordId {discordId}, issue with conversion");
                                    return;
                                }

                                bool hasDonatorRole = false;

                                if (socketGuild != null)
                                {
                                    SocketGuildUser socketGuildUser = socketGuild.GetUser(discordId);

                                    int failureCount = 0;

                                    while (socketGuildUser == null)
                                    {
                                        await Task.Delay(10000);
                                        socketGuildUser = socketGuild.GetUser(discordId);

                                        if (failureCount >= 3)
                                            break;

                                        failureCount++;
                                    }

                                    if (socketGuildUser != null)
                                    {

                                        IReadOnlyCollection<SocketRole> roles = socketGuildUser.Roles;

                                        if (roles.Count == 0)
                                        {
                                            Console.WriteLine($"[INFO] Discord Donation Checker: No roles found; {discordId}");
                                            await user.RemoveDonatorStatus();
                                        }
                                        else
                                        {

                                            List<ulong> roleIdList = new List<ulong>();

                                            roles.ToList().ForEach(role =>
                                            {
                                                roleIdList.Add(role.Id);
                                            });

                                            hasDonatorRole = roleIdList.Contains(541955570601558036) // LIFE
                                                || roleIdList.Contains(588440994543042560) // L2
                                                || roleIdList.Contains(588443443496222720) // L1
                                                || roleIdList.Contains(588444129722105856); // L3

                                            if (roleIdList.Contains(541955570601558036))
                                            {
                                                donatorRoleId = 9;
                                            }

                                            if (roleIdList.Contains(588443443496222720)) // Lv1
                                            {
                                                donatorRoleId = 11;
                                            }

                                            if (roleIdList.Contains(588440994543042560)) // Lv2
                                            {
                                                donatorRoleId = 12;
                                            }

                                            if (roleIdList.Contains(588444129722105856)) // Lv3
                                            {
                                                donatorRoleId = 13;
                                            }

                                            if (hasDonatorRole)
                                            {
                                                if (user.UserRole == (Role)donatorRoleId) return;

                                                await user.AddDonatorStatus(donatorRoleId);

                                                _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[DONATION] U: {user.Username}#{user.UserId}, OR: {user.UserRole}, NR: {(Role)donatorRoleId}, D: {discordId}");
                                            }
                                            else
                                            {
                                                await user.RemoveDonatorStatus();
                                                _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[DONATION] U: {user.Username}#{user.UserId}, D: {discordId} | Removed Role");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // await user.RemoveDonatorStatus();
                                        Console.WriteLine("[ERROR] Discord Donation Checker: SocketGuildUser is null or no longer apart of the guild");
                                        _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[DONATION] User: {user.Username}#{user.UserId} is null or no longer apart of the guild (Attempted 3 times, D: {discordId})");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("[ERROR] Discord Donation Checker: socketGuild is null");
                                    _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[ERROR] socketGuild is null.");
                                }
                            }

                            await Task.Delay(2000);
                        });
                    }
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