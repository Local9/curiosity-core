using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using ProfanityFilterNS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Server.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry.Add("txaLogger:internalChatMessage", new Action<string, string, string>(async (source, user, msg) =>
            {
                string header = $"<center><h3>📣 Announcement 📣</h3></center>";
                EventSystem.GetModule().SendAll("ui:notification", eNotification.NOTIFICATION_ANNOUNCEMENT, $"{header}{msg}<br /> - {user}", "top-center", "snackbar");
                await BaseScript.Delay(500);
                EventSystem.GetModule().SendAll("ui:notification", eNotification.NOTIFICATION_ANNOUNCEMENT, $"{header}{msg}<br /> - {user}", "top-left", "snackbar");
                await BaseScript.Delay(500);
                EventSystem.GetModule().SendAll("ui:notification", eNotification.NOTIFICATION_ANNOUNCEMENT, $"{header}{msg}<br /> - {user}", "top-right", "snackbar");
                await BaseScript.Delay(500);
                EventSystem.GetModule().SendAll("ui:notification", eNotification.NOTIFICATION_ANNOUNCEMENT, $"{header}{msg}<br /> - {user}", "bottom-center", "snackbar");
                await BaseScript.Delay(500);
                EventSystem.GetModule().SendAll("ui:notification", eNotification.NOTIFICATION_ANNOUNCEMENT, $"{header}{msg}<br /> - {user}", "bottom-right", "snackbar");
                await BaseScript.Delay(500);
                EventSystem.GetModule().SendAll("ui:notification", eNotification.NOTIFICATION_ANNOUNCEMENT, $"{header}{msg}<br /> - {user}", "bottom-left", "snackbar");

                Logger.Debug($"{user} Announcement: {header}{msg}");
            }));

            // may need to make this so its :local, :world, :universe, :help, and yes, it would make it easier!
            EventSystem.Attach("chat:message", new AsyncEventCallback(async metadata => {
                try
                {
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                    List<CuriosityUser> playersInSameWorld = PluginManager.ActiveUsers.Select(x => x.Value).Where(x => x.RoutingBucket == curiosityUser.RoutingBucket).ToList();

                    string message = metadata.Find<string>(0);
                    string channel = metadata.Find<string>(1);

                    var filter = new ProfanityFilter();
                    string filteredMessage = filter.CensorString(message);
                    message = filteredMessage.Replace(@"\", "");

                    string latestName = curiosityUser.LatestName;

                    if (!string.IsNullOrEmpty(curiosityUser.JobCallSign))
                        latestName = $"[{curiosityUser.JobCallSign}] {curiosityUser.LatestName}";

                    switch (channel)
                    {
                        case "universe":
                        case "global":
                        case "help":
                            EventSystem.GetModule().SendAll("chat:receive", latestName, $"{curiosityUser.Role}", message, channel, curiosityUser.CurrentJob, curiosityUser.RoutingBucket);
                            break;
                        case "international":
                            playersInSameWorld.ForEach(u =>
                            {
                                u.Send("chat:receive", latestName, $"{curiosityUser.Role}", message, channel, curiosityUser.CurrentJob, curiosityUser.RoutingBucket);
                            });
                            break;
                        case "local":
                            Dictionary<int, Player> players = new();
                            Player currentPlayer = PluginManager.PlayersList[metadata.Sender];

                            foreach (CuriosityUser user in playersInSameWorld)
                            {
                                Player player = PluginManager.PlayersList[user.Handle];
                                if (player is not null)
                                    players.Add(user.Handle, player);
                            }

                            playersInSameWorld.Select(x => x).Where(x => Vector3.Distance(players[x.Handle].Character.Position, currentPlayer.Character.Position) < 100f).ToList().ForEach(p =>
                            {
                                p.Send("chat:receive", latestName, $"{curiosityUser.Role}", message, channel, curiosityUser.CurrentJob, curiosityUser.RoutingBucket);
                            });

                            players.Clear();
                            break;
                    }

                    // NOTE: Store messages in database

                    string discordMessageStart = $"[W: {curiosityUser.RoutingBucket}, SH: {metadata.Sender}, CH: {channel}] {curiosityUser.LatestName}#{curiosityUser.UserId}";
                    string discordMessage = filteredMessage.Trim('"');

                    DiscordClient.GetModule().SendChatMessage(discordMessageStart, discordMessage);
                    await BaseScript.Delay(0);

                    playersInSameWorld.Clear();

                    return null;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "chat:message");

                    string message = $"[{DateTime.Now.ToString("HH: mm")}]\n";
                    message += "ERROR: chat:message\n";
                    message += "Stack Trace:\n";
                    message += $"{ex}";

                    DiscordClient.GetModule().SendDiscordServerEventLogMessage(message);
                    await BaseScript.Delay(0);

                    return null;
                }
            }));

            Instance.ExportDictionary.Add("AddToServerLog", new Func<string, bool>(
                (message) =>
                {
                    OnLogMessage(message);

                    return true;
                }
            ));

            Instance.ExportDictionary.Add("AddToPlayerLog", new Func<int, string, bool>(
                (playerId, message) =>
                {
                    OnPlayerLogMessage(playerId, message);

                    return true;
                }
            ));
        }

        public static void OnChatMessage([FromSource] Player player, string message, string channel = "help")
        {
            int playerHandle = int.Parse(player.Handle);
            CuriosityUser user = PluginManager.ActiveUsers[playerHandle];
            user.Send("chat:receive", "SERVER", "SERVER", message, channel, string.Empty);
        }

        public static void OnServerMessage([FromSource] Player player, string message, string channel = "help")
        {
            int playerHandle = int.Parse(player.Handle);
            CuriosityUser user = PluginManager.ActiveUsers[playerHandle];

            user.Send("chat:receive", "SERVER", "SERVER", message, channel, string.Empty);
        }

        public static void OnPlayerLogMessage(int playerHandle, string message)
        {
            if (!PluginManager.ActiveUsers.ContainsKey(playerHandle)) return;

            CuriosityUser user = PluginManager.ActiveUsers[playerHandle];
            user.Send("chat:receive", "[P-LOG]", "SERVER", message, "log", string.Empty);
        }

        public static void OnLogMessage(string message)
        {
            EventSystem.GetModule().SendAll("chat:receive", "[S-LOG]", "SERVER", message, "log", string.Empty);
        }
    }
}
