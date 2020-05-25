using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.LifeV.Bot.Modules
{
    [Group("admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        // lv!admin user <userId>
        [Command("user")]
        [Summary("Return information from Curiosity")]
        [RequireUserPermission(Discord.ChannelPermission.MoveMembers)]
        [Alias("u", "whois")]
        public async Task GetUser([Summary("Curiosity UserId")] int? userId = null)
        {
            if (userId == null)
            {
                await Context.Channel.SendMessageAsync($"UserId must be supplied");
                return;
            }

            await Context.Channel.SendMessageAsync($"hello world");
        }
    }
}
