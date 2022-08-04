using DSharpPlus.Entities;
using Perseverance.Discord.Bot.Database.Store;
using Perseverance.Discord.Bot.Entities;
using Timer = System.Timers.Timer;

namespace Perseverance.Discord.Bot.AutomateScripts
{
    internal class DonationProcessor
    {
        static Timer _timer;
        static DiscordClient _discordClient;

        public DonationProcessor()
        {
            _timer = new Timer();
            _timer.Elapsed += Timer_Elapsed;
            _timer.Interval = (1000 * 30);
            _timer.Enabled = true;

            _discordClient = Program.Client;
        }

        private async void Timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                List<DatabaseUser> users = await DatabaseUser.GetDonatorsAsync();
            }
            catch (Exception ex)
            {
                Program.SendMessage(Program.CURIOSITY_BOT_TEXT_CHANNEL, $"CRITICAL EXCEPTION [DonationProcessor]\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
