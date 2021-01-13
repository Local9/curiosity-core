using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;

namespace Curiosity.Core.Server.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("chat:global", new EventCallback(metadata => {

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                ChatMessage chatMessage = new ChatMessage();

                chatMessage.Name = curiosityUser.LatestName;
                chatMessage.Role = $"{curiosityUser.Role}";
                chatMessage.Message = metadata.Find<string>(0);
                chatMessage.Channel = metadata.Find<string>(1);

                string jsonMessage = JsonConvert.SerializeObject(chatMessage);

                EventSystem.GetModule().SendAll("chat:receive", jsonMessage);

                string discordMessageStart = $"{DateTime.Now.ToString("HH:mm")} [{metadata.Sender}] {curiosityUser.LatestName}#{curiosityUser.UserId}";
                string discordMessage = chatMessage.Message.Trim('"');

                Instance.ExportDictionary["curiosity-server"].DiscordChatLog(discordMessageStart, discordMessage);

                return null;
            }));

            Instance.ExportDictionary.Add("AddToLog", new Func<string, bool>(
                (message) =>
                {
                    ChatMessage chatMessage = new ChatMessage();

                    chatMessage.Name = "[S-LOG]";
                    chatMessage.Role = $"SERVER";
                    chatMessage.Message = $"[{DateTime.Now.ToString("HH:mm:ss")}] {message}";
                    chatMessage.Channel = "log";

                    string jsonMessage = JsonConvert.SerializeObject(chatMessage);

                    EventSystem.GetModule().SendAll("chat:receive", jsonMessage);

                    return true;
                }
            ));

            Instance.ExportDictionary.Add("AddToPlayerLog", new Func<int, string, bool>(
                (playerId, message) =>
                {
                    Logger.Debug($"Player ID: {playerId}, message: {message}");

                    ChatMessage chatMessage = new ChatMessage();

                    chatMessage.Name = "[P-LOG]";
                    chatMessage.Role = $"SERVER";
                    chatMessage.Message = $"[{DateTime.Now.ToString("HH:mm:ss")}] {message}";
                    chatMessage.Channel = "log";

                    string jsonMessage = JsonConvert.SerializeObject(chatMessage);

                    EventSystem.GetModule().Send("chat:receive", playerId, jsonMessage);

                    return true;
                }
            ));
        }

        public static void OnChatMessage([FromSource] Player player, string message, string channel)
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

        public static void OnLogMessage(string message)
        {
            ChatMessage chatMessage = new ChatMessage();

            chatMessage.Name = "[S-LOG]";
            chatMessage.Role = $"SERVER";
            chatMessage.Message = $"[{DateTime.Now.ToString("HH:mm:ss")}] {message}";
            chatMessage.Channel = "log";

            string jsonMessage = JsonConvert.SerializeObject(chatMessage);

            EventSystem.GetModule().SendAll("chat:receive", jsonMessage);
        }
    }
}
