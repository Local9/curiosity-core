using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;

namespace Curiosity.Systems.Server.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        public override void Begin()
        {
            Curiosity.EventRegistry["chat:global"] += new Action<Player, string, string>(OnChatMessage);
        }

        public static void OnChatMessage([FromSource] Player player, string message, string channel)
        {
            int playerHandle = int.Parse(player.Handle);
            CuriosityUser user = CuriosityPlugin.ActiveUsers[playerHandle];

            ChatMessage chatMessage = new ChatMessage();

            string jsonMessage = string.Empty;

            chatMessage = new ChatMessage();
            chatMessage.Name = user.LastName;
            chatMessage.Role = $"{user.Role}";
            chatMessage.Message = message;
            chatMessage.Channel = channel;

            jsonMessage = JsonConvert.SerializeObject(chatMessage);

            CuriosityPlugin.TriggerClientEvent("chat:receive", jsonMessage);
        }
    }
}
