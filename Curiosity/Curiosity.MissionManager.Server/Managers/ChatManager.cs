using CitizenFX.Core;
using Curiosity.MissionManager.Server;
using Curiosity.MissionManager.Server.Managers;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;

namespace Curiosity.Systems.Server.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry["chat:global"] += new Action<Player, string, string>(OnChatMessage);
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

            PluginManager.TriggerClientEvent("chat:receive", jsonMessage);
        }
    }
}
