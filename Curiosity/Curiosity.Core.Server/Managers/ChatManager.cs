using CitizenFX.Core;
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
                var player = PluginManager.PlayersList[metadata.Sender];

                if (player == null) return null;

                string exportResponse = Instance.ExportDictionary["curiosity-server"].GetUser(player.Handle);

                CuriosityUser curiosityUser = JsonConvert.DeserializeObject<CuriosityUser>($"{exportResponse}");

                ChatMessage chatMessage = new ChatMessage();

                chatMessage.Name = curiosityUser.LatestName;
                chatMessage.Role = $"{curiosityUser.Role}";
                chatMessage.Message = metadata.Find<string>(0);
                chatMessage.Channel = metadata.Find<string>(1);

                string jsonMessage = JsonConvert.SerializeObject(chatMessage);

                EventSystem.GetModule().Send("chat:receive", -1, jsonMessage);

                string discordMessageStart = $"{DateTime.Now.ToString("HH:mm")} [{player.Handle}] {player.Name}#{curiosityUser.UserId}";
                string discordMessage = chatMessage.Message.Trim('"');

                Instance.ExportDictionary["curiosity-server"].DiscordChatLog(discordMessageStart, discordMessage);

                return null;
            }));
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
    }
}
