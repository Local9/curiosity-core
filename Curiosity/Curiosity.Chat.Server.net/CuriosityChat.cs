using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Curiosity.Chat.Server.net
{
    public class CuriosityChat : BaseScript
    {
        Regex regex = new Regex(@"^[\x20-\x7E£]+$");

        public CuriosityChat()
        {
            EventHandlers["curiosity:Server:Chat:Message"] += new Action<Player, string, string, string, string>(ChatMessage);
            EventHandlers["curiosity:Server:Chat:RconCommand"] += new Action<Player, string, List<object>>(RconCommand);
        }

        async void ChatMessage([FromSource]Player player, string role, string message, string color, string scope)
        {
            await Delay(0);
            string originalMessage = message;
            bool profanityFound = false;

            if (string.IsNullOrWhiteSpace(color) || string.IsNullOrWhiteSpace(message))
            {
                // TODO: Use Log instead
                Debug.WriteLine($"[CHAT] Invalid chat message '{message}' entered by '{player.Name}' (#{player.Handle})");
                Function.Call(Hash.CANCEL_EVENT);
                return;
            }

            if (!regex.Match(message).Success)
            {
                player.TriggerEvent("curiosity:Client:Chat:Message", $"SERVER", "red", "Invalid characters detected, message not sent.");
                Debug.WriteLine($"[CHAT] Invalid chat message '{message}' entered by '{player.Name}' (#{player.Handle})");
                return;
            }

            if (message.ContainsProfanity())
            {
                profanityFound = true;
                player.TriggerEvent("curiosity:Server:Chat:Profanity");
                await Delay(0);
                Regex wordFilter = new Regex($"({string.Join("|", ProfanityFilter.ProfanityArray())})");
                message = wordFilter.Replace(message, "#####");
            }

            switch (scope)
            {
                default:
                    TriggerClientEvent("curiosity:Client:Chat:Message", $"{role} {player.Name}", color, message);
                    break;
            }
            await Delay(0);
            if (profanityFound)
                message = string.Format("\n:warning: **PROFANITY REMOVED** :warning:\n **Sent to Players:** {0}\n **Original Message:** {1}", message, originalMessage);

            TriggerEvent("curiosity:Server:Discord:ChatMessage", player.Name, message);
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
