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
using System.Threading.Tasks;
using System.Timers;

namespace Curiosity.LifeV.Bot
{
    internal class TimedEvents : ModuleBase<SocketCommandContext>
    {
        private const long CURIOSITY_BOT_TEXT_CHANNEL = 773248492939247626;
        static Timer donationTimer;
        static Timer banTimer;
        static Timer statusUpdater;

        private static DiscordSocketClient _client;
        private static ulong _guildId;

        private Dictionary<string, string> servers = new Dictionary<string, string>()
        {
            { "Public", "5.9.0.85:30120" },
            { "Members", "5.9.0.85:30121" }
        };
        private int currentServer = 0;

        public TimedEvents(DiscordSocketClient client, ulong guildId)
        {
            _client = client;
            _guildId = guildId;

            donationTimer = new Timer();
            donationTimer.Elapsed += new ElapsedEventHandler(OnDiscordDonationChecker);
            donationTimer.Interval = (1000 * 60) * 60; // EVERY HOUR
            donationTimer.Enabled = true;

            banTimer = new Timer();
            banTimer.Elapsed += new ElapsedEventHandler(OnBanProcessing);
            banTimer.Interval = (1000 * 60) * 30; // EVERY 30 minutes
            banTimer.Enabled = true;

            statusUpdater = new Timer();
            statusUpdater.Elapsed += new ElapsedEventHandler(OnStatusUpdater);
            statusUpdater.Interval = (1000 * 30); // EVERY 30 Seconds
            statusUpdater.Enabled = true;
        }

        private async void OnStatusUpdater(object sender, ElapsedEventArgs e)
        {
            try
            {
                currentServer++;

                if (currentServer >= servers.Count)
                    currentServer = 0;

                KeyValuePair<string, string> server = servers.ElementAt(currentServer);

                string serverInformation = string.Empty;

                try
                {
                    serverInformation = await Tools.HttpTools.GetUrlResultAsync($"http://{server.Value}/info.json");
                }
                catch (Exception ex)
                {
                    _client.SetGameAsync($"{server.Key} : Server Offline");
                    return;
                }

                string result = await Tools.HttpTools.GetUrlResultAsync($"http://{server.Value}/players.json");

                List<CitizenFxPlayers> lst = JsonConvert.DeserializeObject<List<CitizenFxPlayers>>(result);
                CitizenFxInfo info = JsonConvert.DeserializeObject<CitizenFxInfo>(serverInformation);

                _client.SetGameAsync($"{server.Key} {lst.Count}/32");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] OnStatusUpdater -> {ex.Message}");
            }
        }

        private async void OnBanProcessing(object sender, ElapsedEventArgs e)
        {
            try
            {
                using var connection = await Database.DatabaseConfig.GetDatabaseConnection();
                await connection.OpenAsync();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = @"call spProcessBans();";
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CRITICAL] OnStatusUpdater -> {ex.Message}");
            }
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
                            Role donatorRole = Role.USER;
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
                                                donatorRole = Role.DONATOR_LIFE;
                                            }

                                            if (roleIdList.Contains(588443443496222720)) // Lv1
                                            {
                                                donatorRole = Role.DONATOR_LEVEL_1;
                                            }

                                            if (roleIdList.Contains(588440994543042560)) // Lv2
                                            {
                                                donatorRole = Role.DONATOR_LEVEL_2;
                                            }

                                            if (roleIdList.Contains(588444129722105856)) // Lv3
                                            {
                                                donatorRole = Role.DONATOR_LEVEL_3;
                                            }

                                            if (hasDonatorRole)
                                            {
                                                if (user.UserRole == donatorRole) return;

                                                await user.AddDonatorStatus((int)donatorRole);

                                                _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[DONATION] U: {user.Username}#{user.UserId}, OR: {user.UserRole}, NR: {donatorRole}, D: {discordId}");
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
                                        Console.WriteLine("[ERROR] Discord Donation Checker: SocketGuildUser is null or no longer apart of the guild");
                                        _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[DONATION] User: {user.Username}#{user.UserId} is null or no longer apart of the guild (Attempted 3 times, D: {discordId})");
                                        await user.RemoveDonatorStatus(); // just fucking removing it
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("[ERROR] Discord Donation Checker: socketGuild is null");
                                    _client.GetGuild(_guildId).GetTextChannel(CURIOSITY_BOT_TEXT_CHANNEL).SendMessageAsync($"[ERROR] socketGuild is null.");
                                }
                            }

                            await Task.Delay(5000);
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