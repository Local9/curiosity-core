using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Perseverance.Discord.Bot.Config;
using Perseverance.Discord.Bot.Database.Store;
using Perseverance.Discord.Bot.Entities;

namespace Perseverance.Discord.Bot.SlashCommands
{
    public enum eServerList
    {
        Main
    }

    [SlashCommandGroup("server", "Server related commands.")]
    public class ServerCommands : ApplicationCommandModule
    {
        [SlashCommand("status", "Get server status")]
        public async Task StatusCommand(InteractionContext ctx, [Option("server", "Server to get information from.")] eServerList serverIndex = eServerList.Main)
        {
            List<Server> servers = ApplicationConfig.Servers;

            if (servers.Count == 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "No servers found in the config."
                });
                return;
            }

            Server server = servers[(int)serverIndex];
            string serverInformation = string.Empty;

            try
            {
                serverInformation = await Utils.HttpTools.GetUrlResultAsync($"http://{server.IP}/info.json");
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "Server failed to respond, possible the server is currently offline."
                });
                return;
            }

            string players = await Utils.HttpTools.GetUrlResultAsync($"http://{server.IP}/players.json");

            List<CitizenFxPlayer> playersList = JsonConvert.DeserializeObject<List<CitizenFxPlayer>>(players);
            CitizenFxInfo info = JsonConvert.DeserializeObject<CitizenFxInfo>(serverInformation);

            // return an embed message
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.Color = DiscordColor.Green;
            embedBuilder.Title = $"{server.Label}";
            embedBuilder.Description = $"Players: {playersList.Count}/{info.Variables["sv_maxClients"]}";
            embedBuilder.AddField("Currently Connected", $"{string.Join(", ", playersList)}", false);
            embedBuilder.WithTimestamp(DateTime.Now);
            embedBuilder.WithFooter("lifev.net");

            await ctx.CreateResponseAsync(embedBuilder);
        }

        [SlashCommand("connect", "Get server connect information")]
        public async Task ConnectCommand(InteractionContext ctx, [Option("server", "Server to get information.")] eServerList serverIndex = eServerList.Main)
        {
            // Get server from config
            List<Server> servers = ApplicationConfig.Servers;

            if (servers.Count == 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "No servers found in the config."
                });
                return;
            }

            // get server player information
            Server server = servers[(int)serverIndex];
            string serverInformation = string.Empty;

            try
            {
                serverInformation = await Utils.HttpTools.GetUrlResultAsync($"http://{server.IP}/info.json");
            }
            catch
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = "Server failed to respond, possible the server is currently offline."
                });
                return;
            }

            string players = await Utils.HttpTools.GetUrlResultAsync($"http://{server.IP}/players.json");

            // return an embed message
            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder();
            embedBuilder.Color = DiscordColor.Green;
            embedBuilder.Title = $"{server.Label}";
            embedBuilder.Description = $"You can connect to the server using the following;";
            embedBuilder.AddField("URL", $"http://connect.lifev.net", false);
            embedBuilder.AddField("Console IP", $"`connect {server.IP}`", false);
            embedBuilder.WithTimestamp(DateTime.Now);
            embedBuilder.WithFooter("lifev.net");

            await ctx.CreateResponseAsync(embedBuilder);
        }

        [SlashCommand("top", "Get top players on the server based on Skill Experience.")]
        public async Task TopCommand(InteractionContext ctx, [Option("Skill", "Skill to look up.")] string skill)
        {
            try
            {
                // return message
                string message = string.Empty;
                List<string> topSkills = await Database.Store.DatabaseSkill.GetSkillsTopAsync();

                if (!topSkills.Contains(skill))
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    {
                        Content = $"Skill {skill} is not in the top list.\n\nExpected: {string.Join(", ", topSkills)}"
                    });
                    return;
                }

                IEnumerable<DatabaseSkill> topPlayers = await Database.Store.DatabaseSkill.GetSkillsTopPlayers(5, skill);

                if (topPlayers.ToList().Count == 0)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    {
                        Content = $"No players found for {skill}."
                    });
                    return;
                }

                string topUsers = "```autohotkey";
                topUsers += "\nRank | Name";
                int count = 1;

                foreach (DatabaseSkill databaseSkill in topPlayers)
                {
                    topUsers += $"\n[{count:00}]    > {databaseSkill.Username}";
                    topUsers += $"\n              Experience: {databaseSkill.Experience:#,###,##0}";
                    count++;
                };

                topUsers += "```";

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                {
                    Content = topUsers
                });
            }
            catch (Exception ex)
            {
                Program.SendMessage(Program.BOT_ERROR_TEXT_CHANNEL, $"CRITICAL EXCEPTION [TopCommand]\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
