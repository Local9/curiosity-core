using CitizenFX.Core;
using Curiosity.Server.net.Classes;

namespace Curiosity.Server.net.Helpers
{
    class ChatLog
    {
        private static Server ServerInstance = Server.GetInstance();

        public static void SendLogMessage(string message, Player player = null, bool discord = false)
        {
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
