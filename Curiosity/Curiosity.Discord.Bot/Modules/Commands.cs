﻿using Curiosity.Discord.Bot.Entities.CitizenFX;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Curiosity.Discord.Bot.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private List<ulong> usedCommandRecently = new List<ulong>();

        [Command("ping")]
        public async Task Ping(SocketUser user = null)
        {
            if (user == null)
                user = Context.User;


            if (user.Id == 307265731333062656) // Trouble
            {

                List<string> responses = new List<string>() { "Leave me alone...", "Bug off...", "I'll smack ya..." };
                Random random = new Random();

                await ReplyAsync(responses[random.Next(responses.Count)]);
                return;
            }

            if (user.Id == 250431901674897409) // luna
            {
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithImageUrl("https://tenor.com/view/tom-and-jerry-ping-pong-in-pain-gif-15478835");
                await ReplyAsync("", false, builder.Build());
                return;
            }

            if (user.Id != 191686898450825217)
            {
                await ReplyAsync("And who the hell are you?!");
                return;
            }

            await ReplyAsync($"Hi boss, all is doing well.");
        }

        [Command("server")]
        public async Task Server(SocketUser user = null)
        {
            try
            {
                string serverInformation = await Tools.HttpTools.GetUrlResultAsync("http://5.9.0.85:30120/info.json");

                if (string.IsNullOrEmpty(serverInformation))
                {
                    await ReplyAsync("Server did not respond, it might be offline?!");
                }
                else
                {
                    string result = await Tools.HttpTools.GetUrlResultAsync("http://5.9.0.85:30120/players.json");
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
                        .WithTitle("Emergency Life V")
                        .AddField("Server Uptime", $"{info.Variables["Uptime"]}", true)
                        .AddField("Player Count", $"{countOfPlayers}", true)
                        .AddField("Players", $"{playersOnline}", false)
                        .WithColor(Color.Blue)
                        .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                        .WithCurrentTimestamp()
                        .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

                    // builder.Url = "fivem://connect/5.9.0.85:30120";

                    await ReplyAsync("`connect server.lifev.net:30120`", false, builder.Build());
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync("Sorry, something went wrong when trying to communicate with the server.", false);
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
                "\nlv!server - Will display server information" +
                "\nlv!account - Show you're Curiosity Server account" +
                "\nlv!top - Top 10 Players by Life V Experience"
                ).WithColor(Color.Blue)
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .WithCurrentTimestamp()
                    .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

            await ReplyAsync("", false, builder.Build());
        }

        [Command("account")]
        public async Task Account(SocketUser user = null)
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
                    .AddField("Experience", $"{dbUser.LifeExperience:#,###,###}")
                    .AddField("Server First Joined", $"{dbUser.DateCreated.ToString("yyyy-MM-dd HH:mm")}", true)
                    .AddField("Server Last Seen", $"{dbUser.LastSeen.ToString("yyyy-MM-dd HH:mm")}", true)
                    .WithColor(Color.Blue)
                        .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                        .WithCurrentTimestamp()
                        .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

                await ReplyAsync("", false, builder.Build());
            }
        }

        [Command("top")]
        public async Task Top(SocketUser user = null)
        {
            try
            {
                if (user == null)
                    user = Context.User;

                List<Models.User> dbUsers = await new Models.User().GetTopUsers();

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
                        topUsers += $"\n        Total Experience: {user.LifeExperience:#,###,##0}";
                        count++;
                    });

                    topUsers += "```";

                    await ReplyAsync($"{topUsers}", false);
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
