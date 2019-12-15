using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
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


            if (user.Id == 307265731333062656)
            {
                await ReplyAsync("Leave me alone mate...");
                return;
            }

            if (user.Id == 191686898450825217)
            {
                await ReplyAsync("Hi boss");
                return;
            }

            await ReplyAsync($"pong");
        }

        [Command("server")]
        public async Task Server(SocketUser user = null)
        {
            try
            {
                string result = await Tools.HttpTools.GetUrlResultAsync("http://5.9.0.85:30120/players.json");
                string serverInformation = await Tools.HttpTools.GetUrlResultAsync("http://5.9.0.85:30120/info.json");

                List<Entities.CitizenFxPlayers> lst = JsonConvert.DeserializeObject<List<Entities.CitizenFxPlayers>>(result);
                Entities.CitizenFxInfo info = JsonConvert.DeserializeObject<Entities.CitizenFxInfo>(serverInformation);

                int countOfPlayers = lst.Count;

                EmbedBuilder builder = new EmbedBuilder();

                string playersOnline = "```";

                lst.ForEach(item =>
                {
                    playersOnline += $"{item.Name}, ";
                });

                playersOnline = playersOnline.Substring(0, playersOnline.Length - 2);

                playersOnline += "```";


                builder
                    .AddField("Emergency Life V", "5.9.0.85:30120")
                    .AddField("Server Uptime", $"{info.Variables["Uptime"]}", true)
                    .AddField("Player Count", $"{countOfPlayers}", true)
                    .AddField("Players", $"{playersOnline}", false)
                    .WithColor(Color.Blue)
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .WithCurrentTimestamp()
                    .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

                await ReplyAsync("", false, builder.Build());   
            }
            catch (Exception ex)
            {
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
                "\nlv!server - Will display server information"
                ).WithColor(Color.Blue)
                    .WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl())
                    .WithCurrentTimestamp()
                    .WithFooter("Forums: https://forums.lifev.net", Context.Guild.IconUrl);

            await ReplyAsync("", false, builder.Build());
        }
    }
}
