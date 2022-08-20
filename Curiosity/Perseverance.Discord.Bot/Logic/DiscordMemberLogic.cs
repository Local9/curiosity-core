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
            eRole userRoleId = eRole.USER;

            bool isLifeSupporter = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["life"]).Any();
            bool isLevel3 = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["level3"]).Any();
            bool isLevel2 = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["level2"]).Any();
            bool isLevel1 = rolesAfter.Where(x => x.Id == Configuration.DonatorRoles["level1"]).Any();

            if (isLifeSupporter)
                userRoleId = eRole.DONATOR_LIFE;
            else if (isLevel3)
                userRoleId = eRole.DONATOR_LEVEL_3;
            else if (isLevel2)
                userRoleId = eRole.DONATOR_LEVEL_2;
            else if (isLevel1)
                userRoleId = eRole.DONATOR_LEVEL_1;

            DatabaseUser user = await DatabaseUser.GetAsync(discordMember.Id);
            eRole currentRole = (eRole)user.Role;

            if (user.IsStaff) return;
            if (userRoleId == currentRole) return;

            isDonator = userRoleId == eRole.DONATOR_LIFE || userRoleId == eRole.DONATOR_LEVEL_3 || userRoleId == eRole.DONATOR_LEVEL_2 || userRoleId == eRole.DONATOR_LEVEL_1;
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            DiscordEmbed embed;

            eRole newRole = (eRole)userRoleId;
            Program.SendMessage(Program.BOT_ERROR_TEXT_CHANNEL, $"[ROLE CHANGE] {discordMember.Mention} - Change to their database role from '{currentRole.GetDescription()}' to '{newRole.GetDescription()}'");

            if (!isDonator)
            {
                    user.RemoveRole();
            }
            else
            {
                user.SetRole((int)userRoleId);
            }

            try
            {
                embedBuilder.Color = isDonator ? DiscordColor.Green : DiscordColor.Orange;
                embedBuilder.Title = "Thank you from Life V";
                if (!isDonator)
                {
                    embedBuilder.Description = "Thank you, we're sad to see your support end. Please let us know what we can do to improve and earn your support again in the future.";
                }
                else
                {
                    embedBuilder.Description = "Thank you for giving us your support.";
                }
                embedBuilder.AddField("User", user.Username, true);
                embedBuilder.AddField("Role", user.Role.GetDescription(), true);
                embedBuilder.WithTimestamp(DateTime.Now);
                embedBuilder.WithFooter("https://lifev.net");

                embed = embedBuilder.Build();

                await discordMember.SendMessageAsync(embed);
            }
            catch (Exception ex)
            {
                Program.SendMessage(Program.BOT_ERROR_TEXT_CHANNEL, $"[ROLE CHANGE] Unable to message {discordMember?.Mention}, {user.Username}.");
            }
        }
    }
}
