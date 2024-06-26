﻿using DSharpPlus.Entities;
using Perseverance.Discord.Bot.Entities;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Perseverance.Discord.Bot.AutomateScripts
{
    public class GameServerStatus
    {
        static Timer _timer;
        static DiscordClient _discordClient;
        static List<Server> servers;
        static int serverIndex = 0;
        
        public GameServerStatus(DiscordClient client)
        {
            _timer = new Timer();
            _timer.Elapsed += new ElapsedEventHandler(Timer_Elapsed);
            _timer.Interval = (1000 * 30);
            _timer.Enabled = true;

            _discordClient = client;

            servers = Program.Configuration.Servers;

            Console.WriteLine($"[SERVER STATUS] Monitoring: {servers.Count}");
        }

        private async void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                string serverInformation = string.Empty;
                
                Server server = servers[serverIndex];
                serverIndex++;
                
                DiscordActivity activity = new();
                activity.Name = "Server Offline";
                activity.ActivityType = ActivityType.ListeningTo;
                try
                {
                    serverInformation = await Utils.HttpTools.GetUrlResultAsync($"http://{server.IP}/info.json");
                }
                catch (Exception ex)
                {
                    await _discordClient.UpdateStatusAsync(activity);
                    return;
                }

                string players = await Utils.HttpTools.GetUrlResultAsync($"http://{server.IP}/players.json");

                if (string.IsNullOrEmpty(players))
                {
                    await _discordClient.UpdateStatusAsync(activity);
                    return;
                }

                List<CitizenFxPlayer> lst = JsonConvert.DeserializeObject<List<CitizenFxPlayer>>(players) ?? new();
                CitizenFxInfo info = JsonConvert.DeserializeObject<CitizenFxInfo>(serverInformation);
                activity.Name = $"{lst.Count}/{info.Variables["sv_maxClients"]} players on {server.Label}";

                await _discordClient.UpdateStatusAsync(activity);

            }
            catch (Exception ex)
            {
                Program.SendMessage(Program.BOT_ERROR_TEXT_CHANNEL, $"CRITICAL EXCEPTION [GameServerStatus]\n{ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
