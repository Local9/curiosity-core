using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;
using Newtonsoft.Json;

namespace Curiosity.Systems.Server.Managers
{
    public class ChatManager : Manager<ChatManager>
    {
        public override void Begin()
        {
            Curiosity.EventRegistry["chat:global"] += new Action<Player, string, string>(OnChatMessage);
        }

        private void OnChatMessage([FromSource] Player player, string message, string channel)
        {
            CuriosityUser user = CuriosityPlugin.ActiveUsers[player.Handle];

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
