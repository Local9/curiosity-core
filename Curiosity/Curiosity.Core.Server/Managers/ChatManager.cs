using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Server.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry.Add("txaLogger:internalChatMessage", new Action<string, string, string>((source, user, msg) =>
            {
                EventSystem.GetModule().SendAll("ui:notification", Notification.NOTIFICATION_SHOW, $"Announcement: {msg}", "top-center", "snackbar");
            }));

            EventSystem.GetModule().Attach("chat:message", new AsyncEventCallback(async metadata => {

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                List<CuriosityUser> playersInSameWorld = PluginManager.ActiveUsers.Select(x => x.Value).Where(x => x.RoutingBucket == curiosityUser.RoutingBucket).ToList();

                string message = metadata.Find<string>(0);
                string channel = metadata.Find<string>(1);

                switch (channel)
                {
                    case "universe":
                    case "help":
                        EventSystem.GetModule().SendAll("chat:receive", curiosityUser.LatestName, $"{curiosityUser.Role}", message, channel, curiosityUser.CurrentJob, curiosityUser.RoutingBucket);
                        break;
                    case "global":
                        playersInSameWorld.ForEach(u =>
                        {
                            u.Send("chat:receive", curiosityUser.LatestName, $"{curiosityUser.Role}", message, channel, curiosityUser.CurrentJob, curiosityUser.RoutingBucket);
                        });
                        break;
                    case "local":
                        playersInSameWorld.Select(x => x).Where(x => Vector3.Distance(x.Character.LastPosition.AsVector(), curiosityUser.Character.LastPosition.AsVector()) < 100f).ToList().ForEach(p =>
                        {
                            p.Send("chat:receive", curiosityUser.LatestName, $"{curiosityUser.Role}", message, channel, curiosityUser.CurrentJob, curiosityUser.RoutingBucket);
                        });
                        break;
                }

                // NOTE: Store messages in database

                string discordMessageStart = $"[{DateTime.Now.ToString("HH:mm")}] [W: {curiosityUser.RoutingBucket}, SH: {metadata.Sender}, CH: {channel}] {curiosityUser.LatestName}#{curiosityUser.UserId}";
                string discordMessage = message.Trim('"');

                DiscordClient.GetModule().SendChatMessage(discordMessageStart, discordMessage);

                return null;
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
