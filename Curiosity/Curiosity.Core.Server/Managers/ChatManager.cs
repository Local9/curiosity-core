using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Core.Server.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        public override void Begin()
        {
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

                DiscordClient.DiscordInstance.SendChatMessage(discordMessageStart, discordMessage);

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

        public static void OnChatMessage([FromSource] Player player, string message, string channel = "local")
        {
            int playerHandle = int.Parse(player.Handle);
            CuriosityUser user = PluginManager.ActiveUsers[playerHandle];

            ChatMessage chatMessage = new ChatMessage();

            chatMessage.Name = user.LatestName;
            chatMessage.Role = $"{user.Role}";
            chatMessage.Message = message;
            chatMessage.Channel = channel;

            string jsonMessage = JsonConvert.SerializeObject(chatMessage);

            user.Send("chat:receive", jsonMessage);
        }

        public static void OnServerMessage([FromSource] Player player, string message, string channel = "local")
        {
            int playerHandle = int.Parse(player.Handle);
            CuriosityUser user = PluginManager.ActiveUsers[playerHandle];

            JsonBuilder jsonBuilder = new JsonBuilder();
            jsonBuilder.Add("operation", "CHAT");
            jsonBuilder.Add("subOperation", "NEW_MESSAGE");
            jsonBuilder.Add("message", new
            {
                role = $"SERVER",
                channel = channel,
                activeJob = string.Empty,
                timestamp = DateTime.Now.ToString("HH:mm"),
                name = "SERVER",
                message = message
            });

            string jsonMessage = JsonConvert.SerializeObject(jsonBuilder.Build());

            user.Send("chat:receive", "SERVER", "SERVER", message, channel, string.Empty);
        }

        public static void OnPlayerLogMessage(int playerHandle, string message)
        {
            if (!PluginManager.ActiveUsers.ContainsKey(playerHandle)) return;

            CuriosityUser user = PluginManager.ActiveUsers[playerHandle];
            user.Send("chat:receive", "SERVER", "SERVER", "[P-LOG]", message, "log", string.Empty);
        }

        public static void OnLogMessage(string message)
        {
            JsonBuilder jsonBuilder = new JsonBuilder();
            jsonBuilder.Add("operation", "CHAT");
            jsonBuilder.Add("subOperation", "NEW_MESSAGE");
            jsonBuilder.Add("message", new
            {
                role = $"SERVER",
                channel = "log",
                activeJob = string.Empty,
                timestamp = DateTime.Now.ToString("HH:mm"),
                name = "[S-LOG]",
                message = message
            });

            string jsonMessage = JsonConvert.SerializeObject(jsonBuilder.Build());

            EventSystem.GetModule().SendAll("chat:receive", "SERVER", "SERVER", "[S-LOG]", message, "log", string.Empty);
        }
    }
}
