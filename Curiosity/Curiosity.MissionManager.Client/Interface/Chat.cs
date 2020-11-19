using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;

namespace Curiosity.MissionManager.Client.Interface
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
            BaseScript.TriggerEvent("curiosity:Client:Chat:Message", json);
        }
    }
}
