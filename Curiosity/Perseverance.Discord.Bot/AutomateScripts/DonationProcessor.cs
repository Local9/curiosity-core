using DSharpPlus.Entities;
using Perseverance.Discord.Bot.Database.Store;
using Perseverance.Discord.Bot.Logic;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Perseverance.Discord.Bot.AutomateScripts
{
    internal class DonationProcessor
    {
        static Timer _timer;
        static DiscordClient _discordClient;

        public DonationProcessor(DiscordClient discordClient)
        {
            _timer = new Timer();
            _timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            _timer.Interval = (1000 * 60) * 60; // 60 Minutes
            _timer.Enabled = true;

            _discordClient = discordClient;
        }

        private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                List<DatabaseUser> users = await DatabaseUser.GetDonatorsAsync();
                DiscordGuild discordGuild = await _discordClient.GetGuildAsync(Program.BOT_GUILD_ID);

                Program.SendMessage(Program.BOT_ERROR_TEXT_CHANNEL, $"[DonationProcessor] Running");

                foreach (DatabaseUser databaseUser in users)
                {
                    ulong discordId = databaseUser.DiscordId;
                    try
                    {
                        DiscordMember member = await discordGuild.GetMemberAsync(discordId);
                        await DiscordMemberLogic.UpdateDonationRole(member);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains($"404"))
                        {
                            await databaseUser.RemoveRole();
                        }
                    }
                }

                Program.SendMessage(Program.BOT_ERROR_TEXT_CHANNEL, $"[DonationProcessor] Completed");
            }
            catch (Exception ex)
            {
                Program.SendMessage(Program.BOT_ERROR_TEXT_CHANNEL, $"CRITICAL EXCEPTION [DonationProcessor]\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
