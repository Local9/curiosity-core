using Curiosity.LifeV.Bot.Entities.CitizenFX;
using Curiosity.LifeV.Bot.Tools;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Curiosity.LifeV.Bot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private List<ulong> usedCommandRecently = new List<ulong>();

        [Command("ping")]
        public async Task Ping(SocketUser user = null)
        {
            if (user == null)
                user = Context.User;

            Random random = new Random();

            if (user.Id == 307265731333062656) // Trouble
            {

                List<string> responses = new List<string>() { "Leave me alone...", "Bug off...", "I'll smack ya..." };
                await ReplyAsync(responses[random.Next(responses.Count)]);
                return;
            }

            if (user.Id == 250431901674897409) // luna
            {
                EmbedBuilder builder = new EmbedBuilder();
                List<string> responses = new List<string>() {
                    "https://antonycook.uk/gif/pong.gif",
                    "https://antonycook.uk/gif/pong2.gif",
                    "https://antonycook.uk/gif/pong3.gif",
                };
                builder.WithImageUrl(responses[random.Next(responses.Count)]);
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if (user.Id != 191686898450825217)
            {
                List<string> responses = new List<string>() {
                    "pong.... I guess",
                    $"I'm not here for your own enjoyment {user.Username}",
                    "Just leave me alone..."
                };
                await ReplyAsync(responses[random.Next(responses.Count)]);
                return;
            }

            await ReplyAsync($"Hi boss, all is doing well.");
        }

        [Command("server")]
        public async Task Server(params string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    await ReplyAsync("Missing server param, please use as so: lv!server elv|dlv", false);
                }
                else
                {

                    Dictionary<string, (string, string, string)> servers = new Dictionary<string, (string, string, string)>();
                    servers.Add("1", ("Live V Public", "5.9.0.85", "30120"));
                    servers.Add("2", ("Life V Patreon", "5.9.0.85", "30121"));

                    string serverKey = args[0];

                    if (!servers.ContainsKey(serverKey))
                    {
                        await ReplyAsync("Servers available: 1 - Public, 2 - Patreon", false);
                    }
                    else
                    {
                        string serverName = servers[serverKey].Item1;
                        string serverUri = servers[serverKey].Item2;
                        string serverPort = servers[serverKey].Item3;

                        string serverInformation = await Tools.HttpTools.GetUrlResultAsync($"http://{serverUri}:{serverPort}/info.json");

                        if (string.IsNullOrEmpty(serverInformation))
                        {
                            await ReplyAsync("Server did not respond, it might be offline.");
                        }
                        else
                        {
                            string result = await Tools.HttpTools.GetUrlResultAsync($"http://{serverUri}:{serverPort}/players.json");
                            List<CitizenFxPlayers> lst = JsonConvert.DeserializeObject<List<CitizenFxPlayers>>(result);
                            CitizenFxInfo info = JsonConvert.DeserializeObject<CitizenFxInfo>(serverInformation);

                            int countOfPlayers = lst.Count;

                            EmbedBuilder builder = new EmbedBuilder();

                            string playersOnline = "No players online";

                            if (countOfPlayers > 0)
                            {
                                playersOnline = "";
                                lst.ForEach(item =>
                                {
                                    playersOnline += $"{item.Name}, ";
                                });

                                playersOnline = playersOnline.Substring(0, playersOnline.Length - 2);
                            }

                            builder
                                .WithTitle($"{serverName}")
                                .AddField("Server Uptime", $"{info.Variables["Uptime"]}", true)
                                .AddField("Player Count", $"{countOfPlayers}", true)
                                .AddField("Players", $"{playersOnline}", false)
                                .WithColor(Color.Blue)
                                .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                                .WithCurrentTimestamp()
                                .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

                            // builder.Url = "fivem://connect/5.9.0.85:30120";

                            await ReplyAsync($"`connect server.lifev.net:{serverPort}`", false, builder.Build());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync("Sorry, something went wrong when trying to communicate with the server.");
                Console.WriteLine($"Server: {ex}");
                throw;
            }
        }

        [Command("help")]
        public async Task Help()
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder
                .AddField("Help Commands",
                "lv!help - What you're looking at right now" +
                "\nlv!server 1/2 - Will display server information" +
                "\nlv!account - Show your Curiosity Server account" +
                "\nlv!top <param> - pilot, trucking, fire, police, knowledge, train, taxi, fishing, hunting, farming, bus, mechanic, ems, freight" +
                "\nlv!donate - Update users donation status"
                ).WithColor(Color.Blue)
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .WithCurrentTimestamp()
                    .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

            await ReplyAsync(embed: builder.Build());
        }

        [Command("account")]
        public async Task Account(SocketUser user = null)
        {
            try
            {
                if (user == null)
                    user = Context.User;

                Models.User dbUser = await new Models.User().FindUserAsync(user.Id);

                if (dbUser == null)
                {
                    await ReplyAsync("User was not found or has not connected to the server.");
                }
                else
                {
                    EmbedBuilder builder = new EmbedBuilder();

                    builder
                        .AddField("Player", $"{dbUser.Username}", true)
                        .AddField("Role", StringValueAttribute.GetStringValue(dbUser.UserRole), true)
                        .AddField("Experience", $"{dbUser.LifeExperience:#,###,###}")
                        .AddField("Server First Joined", $"{dbUser.DateCreated.ToString("yyyy-MM-dd HH:mm")}", true)
                        .AddField("Server Last Seen", $"{dbUser.LastSeen.ToString("yyyy-MM-dd HH:mm")}", true)
                        .WithColor(Color.Blue)
                            .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                            .WithCurrentTimestamp()
                            .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

                    await ReplyAsync(embed: builder.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR");
                Console.WriteLine($"{ex.Message}");
                Console.WriteLine($"{ex}");
                Console.WriteLine($"----");

                await ReplyAsync("Sorry there was an issue trying to find your account, please inform the developers via a ticket so they can look into it.");
            }
        }

        [Command("donate"), Summary("Check users donation status and update if required.")]
        public async Task Donate(SocketUser user = null)
        {
            if (user == null)
                user = Context.User;

            Models.User dbUser = await new Models.User().FindUserAsync(user.Id);

            await Context.Message.DeleteAsync();

            if (dbUser == null)
            {
                await ReplyAsync($"User '{user.Username}' was not found, they may have never connected to the server.");
            }
            else
            {
                bool hasDonatorRole = false;

                IReadOnlyCollection<SocketRole> roles = Context.Guild.GetUser(user.Id).Roles;

                List<ulong> roleIdList = new List<ulong>();

                roles.ToList().ForEach(role =>
                {
                    roleIdList.Add(role.Id);
                });

                hasDonatorRole = roleIdList.Contains(541955570601558036) // LIFE
                            || roleIdList.Contains(588440994543042560) // L2
                            || roleIdList.Contains(588443443496222720) // L1
                            || roleIdList.Contains(588444129722105856); // L3

                int donatorRoleId = 9;
                string tier = "Life V Life Time Supporter";

                if (roleIdList.Contains(588443443496222720)) // Lv1
                {
                    donatorRoleId = 11;
                    tier = "Life V Supporter I";
                }

                if (roleIdList.Contains(588440994543042560)) // Lv2
                {
                    donatorRoleId = 12;
                    tier = "Life V Supporter II";
                }

                if (roleIdList.Contains(588444129722105856)) // Lv3
                {
                    donatorRoleId = 13;
                    tier = "Life V Supporter III";
                }

                string statusStr = "Failed";

                if (hasDonatorRole)
                {
                    await dbUser.AddDonatorStatus(donatorRoleId);
                    statusStr = $"Is a {tier} Donator";
                }
                else
                {
                    await dbUser.RemoveDonatorStatus();
                    statusStr = "Is not a Donator";
                }

                EmbedBuilder builder = new EmbedBuilder();

                builder
                    .AddField("Player", $"{dbUser.Username}", true)
                    .AddField("Status", $"{statusStr}", true)
                    .WithColor(hasDonatorRole ? Color.Green : Color.Blue)
                        .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                        .WithCurrentTimestamp()
                        .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

                await Context.User.SendMessageAsync(embed: builder.Build());

                await ReplyAsync($"Updated donator status for {dbUser.Username} to {statusStr}", false);
            }
        }

        [Command("top")]
        public async Task Top(params string[] args)
        {
            try
            {
                List<Models.User> dbUsers = new List<Models.User>();

                List<string> routes = new List<string> { "pilot", "trucking", "fire", "police", "knowledge", "train", "taxi", "fishing", "hunting", "farming", "bus", "mechanic", "ems", "freight" };
                string route = string.Empty;

                if (args.Length == 0)
                {
                    dbUsers = await new Models.User().GetTopUsers(string.Empty);
                }
                else
                {
                    if (!routes.Contains(args[0]))
                    {
                        string cmds = string.Join(", ", routes);
                        await ReplyAsync($"Command not found. Available: {cmds}");
                        return;
                    }
                    else
                    {
                        route = args[0];
                        dbUsers = await new Models.User().GetTopUsers(route);
                    }
                }

                if (dbUsers == null)
                {
                    await ReplyAsync("No information was returned.");
                }
                else
                {
                    string topUsers = "```autohotkey";
                    topUsers += "\nRank | Name";
                    int count = 1;

                    dbUsers.ForEach(user =>
                    {
                        topUsers += $"\n[{count:00}]    > {user.Username}";

                        if (!string.IsNullOrEmpty(route))
                        {
                            topUsers += $"\n              Experience: {user.Experience:#,###,##0}";
                        }
                        else
                        {
                            topUsers += $"\n        Total Experience: {user.LifeExperience:#,###,##0}";
                        }

                        count++;
                    });

                    topUsers += "```";

                    await ReplyAsync($"{topUsers}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Top -> {ex}");
                throw;
            }

        }
    }
}
