using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Chat.Server.net
{
    public class CuriosityChat : BaseScript
    {
        public CuriosityChat()
        {
            EventHandlers["curiosity:Server:Chat:Message"] += new Action<Player, string, string, string>(ChatMessage);
            EventHandlers["curiosity:Server:Chat:RconCommand"] += new Action<Player, string, List<object>>(RconCommand);
        }

        async void ChatMessage([FromSource]Player player, string message, string color, string scope)
        {
            if (string.IsNullOrWhiteSpace(color) || string.IsNullOrWhiteSpace(message))
            {
                // TODO: Use Log instead
                Debug.WriteLine($"[CHAT] Invalid chat message '{message}' entered by '{player.Name}' (#{player.Handle})");
                return;
            }

            switch (scope)
            {
                default:
                    TriggerClientEvent("curiosity:Client:Chat:Message", player.Name, color, message);
                    break;
            }
            await Delay(0);
        }

        internal void RconCommand([FromSource]Player player, string commandName, List<object> objargs)
        {
            try
            {
                List<string> args = objargs.Cast<string>().ToList();
                if (commandName == "say" && args.Count > 0)
                {
                    string message = String.Join(" ", args);
                    TriggerClientEvent("curiosity:Client:Chat:Message", "console", "#FF0000", message);
                    Function.Call(Hash.CANCEL_EVENT);
                }
                else if (commandName == "tell" && args.Count > 1)
                {
                    int target = Int32.Parse(args[1]);
                    string message = String.Join(" ", args.Skip(1).Take(args.Count - 1));
                    PlayerList playerList = new PlayerList();
                    Player targetPlayer = playerList[target];
                    TriggerClientEvent(targetPlayer, "curiosity:Client:Chat:Message", "console", "#FF0000", message);
                    Function.Call(Hash.CANCEL_EVENT);
                }
                else
                {
                    Function.Call(Hash.CANCEL_EVENT);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RconCommand ERROR: {ex.Message}");
            }
        }
    }
}
