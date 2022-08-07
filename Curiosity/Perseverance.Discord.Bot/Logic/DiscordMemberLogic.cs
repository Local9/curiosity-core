using DSharpPlus.Entities;
using Perseverance.Discord.Bot.Config;
using Perseverance.Discord.Bot.Database.Store;
using Perseverance.Discord.Bot.Entities;
using Perseverance.Discord.Bot.Entities.Enums;
using Perseverance.Discord.Bot.Extensions;

namespace Perseverance.Discord.Bot.Logic
{
    internal static class DiscordMemberLogic
    {
        public static Configuration Configuration { get; private set; }
        
        public static async Task UpdateDonationRole(DiscordMember discordMember)
        {
            Configuration = await ApplicationConfig.GetConfig();
            List<DiscordRole> rolesAfter = discordMember.Roles.ToList();

            bool isDonator = false;
            int userRoleId = 1;

            bool isLifeSupporter = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["life"]).Any();
            bool isLevel3 = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["level3"]).Any();
            bool isLevel2 = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["level2"]).Any();
            bool isLevel1 = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["level1"]).Any();

            if (isLifeSupporter)
                userRoleId = (int)Role.DONATOR_LIFE;
            else if (isLevel3)
                userRoleId = (int)Role.DONATOR_LEVEL_3;
            else if (isLevel2)
                userRoleId = (int)Role.DONATOR_LEVEL_2;
            else if (isLevel1)
                userRoleId = (int)Role.DONATOR_LEVEL_1;

            DatabaseUser user = await DatabaseUser.GetAsync(discordMember.Id);
            int currentRole = (int)user.Role;

            if (user.IsStaff) return;
            if (userRoleId == currentRole) return;

            Role newRole = (Role)userRoleId;
            Program.SendMessage(Program.BOT_TEXT_CHANNEL, $"[ROLE CHANGE] {discordMember.Username} has changed their database role from {currentRole.GetDescription()} to {newRole.GetDescription()}");

            if (!isDonator)
            {
                // remove DB Role
                user.RemoveRole();
                return;
            }

            // add DB Role
            user.SetRole(userRoleId);
            
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.Color = DiscordColor.Green;
            embedBuilder.Title = "Thank you from Life V";
            embedBuilder.Description = "Thank you for your support, if your status on the server has not updated, please try reconnecting.";
            embedBuilder.AddField("User", user.Username, true);
            embedBuilder.AddField("Role", user.Role.GetDescription(), true);
            embedBuilder.WithTimestamp(DateTime.Now);
            embedBuilder.WithFooter("https://lifev.net");
            
            DiscordEmbed embed = embedBuilder.Build();
            
            await discordMember.SendMessageAsync(embed);
        }
    }
}
