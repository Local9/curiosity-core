using CitizenFX.Core;
using Curiosity.Global.Shared.Entity;
using Curiosity.Server.net.Classes;
using System;

namespace Curiosity.Server.net.Helpers
{
    class ChatLog
    {
        private static Server ServerInstance = Server.GetInstance();

        public static void SendLogMessage(string message, Player player = null, bool discord = false)
        {
            ChatMessage chatMessage = new ChatMessage();

            chatMessage.Name = "[SERVER]";
            chatMessage.Role = "SERVER";
            chatMessage.Channel = "log";
            chatMessage.Message = $"[{DateTime.Now.ToString("HH:mm:ss")}] {message}";

            if (player == null)
            {
                ServerInstance.ExportDictionary["curiosity-core"].AddToLog(message);
            }
            else
            {
                int playerId = int.Parse(player.Handle);
                ServerInstance.ExportDictionary["curiosity-core"].AddToPlayerLog(playerId, message);
            }

            if (discord)
                DiscordWrapper.SendDiscordPlayerLogMessage(message);
        }
    }
}
