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
            EventSystem.GetModule().Attach("chat:send", new AsyncEventCallback(metadata =>
            {
                Player player = CuriosityPlugin.PlayersList[metadata.Sender];
                CuriosityUser user = CuriosityPlugin.ActiveUsers[player.Handle];

                ChatMessage chatMessage = metadata.Find<ChatMessage>(0);

                string jsonMessage = string.Empty;

                chatMessage = new ChatMessage();
                chatMessage.Name = user.LastName;
                chatMessage.Role = user.Role;

                jsonMessage = JsonConvert.SerializeObject(chatMessage);

                CuriosityPlugin.TriggerClientEvent("chat:receive", jsonMessage);

                return null;
            }));
        }
    }
}
