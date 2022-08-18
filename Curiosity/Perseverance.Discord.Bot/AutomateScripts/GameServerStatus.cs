using DSharpPlus.Entities;
using Perseverance.Discord.Bot.Entities;
using Timer = System.Timers.Timer;

namespace Perseverance.Discord.Bot.AutomateScripts
{
    public class GameServerStatus
    {
        static Timer _timer;
        static DiscordClient _discordClient;

        public GameServerStatus()
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
                List<Server> servers = Program.Configuration.Servers;
                string serverInformation = string.Empty;
                foreach (Server server in servers)
                {
                    DiscordActivity activity = new();
                    activity.Name = "Server Offline";
                    activity.ActivityType = ActivityType.ListeningTo;
                    try
                    {
                        serverInformation = await Utils.HttpTools.GetUrlResultAsync($"http://{server}/info.json");
                    }
                    catch
                    {
                        await _discordClient.UpdateStatusAsync(activity);
                        return;
                    }

                    string players = await Utils.HttpTools.GetUrlResultAsync($"http://{server}/players.json");

                    List<CitizenFxPlayer> lst = JsonConvert.DeserializeObject<List<CitizenFxPlayer>>(players);
                    CitizenFxInfo info = JsonConvert.DeserializeObject<CitizenFxInfo>(serverInformation);
                    activity.Name = $"{lst.Count}/{info.Variables["sv_maxClients"]} players on {server.Label}";

                    await _discordClient.UpdateStatusAsync(activity);
                }
            }
            catch (Exception ex)
            {
                Program.SendMessage(Program.BOT_TEXT_CHANNEL, $"CRITICAL EXCEPTION [GameServerStatus]\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
