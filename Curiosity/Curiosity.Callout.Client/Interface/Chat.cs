using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;

namespace Curiosity.Callout.Client.Interface
{
    class Chat
    {
        internal static void SendLocalMessage(string message)
        {
            ChatMessage chatMessage = new ChatMessage();
            chatMessage.Channel = "chat";
            chatMessage.Role = $"SERVER";
            chatMessage.Name = "SERVER";
            chatMessage.Message = message;

            string json = JsonConvert.SerializeObject(chatMessage);
            BaseScript.TriggerEvent("chat:receive", json);
        }
    }
}
