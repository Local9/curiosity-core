﻿using Curiosity.Global.Shared.net;
using Curiosity.Global.Shared.net.Entity;
using System;
using CitizenFX.Core;

namespace Curiosity.Server.net.Helpers
{
    class ChatLog
    {

        public static void SendLogMessage(string message, Player player = null)
        {
            ChatMessage chatMessage = new ChatMessage();

            chatMessage.list = "log";
            chatMessage.message = $"[{DateTime.Now.ToString("HH:mm:ss")}] {message}";

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
        }
    }
}
