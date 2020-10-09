using CitizenFX.Core;
using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Server.net.Classes;
using System;

namespace Curiosity.Server.net.Helpers
{
    class ChatLog
    {
        public static void SendLogMessage(string message, Player player = null, bool discord = false)
        {
            ChatMessage chatMessage = new ChatMessage();

            chatMessage.Channel = "log";
            chatMessage.Message = $"[{DateTime.Now.ToString("HH:mm:ss")}] {message}";

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(chatMessage);
            string encoded = Encode.StringToBase64(json);

            if (player == null)
            {
                Server.TriggerClientEvent("curiosity:Client:Chat:Message", encoded);
            }
            else
            {
                player.TriggerEvent("curiosity:Client:Chat:Message", encoded);
            }

            if (discord)
                DiscordWrapper.SendDiscordPlayerLogMessage(message);
        }
    }
}
