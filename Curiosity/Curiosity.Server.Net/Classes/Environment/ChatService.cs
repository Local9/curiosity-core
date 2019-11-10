using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Server.net.Extensions;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net;

namespace Curiosity.Server.net.Classes.Environment
{
    class ChatService
    {
        static Server server = Server.GetInstance();
        static Regex regex = new Regex(@"^[\x20-\x7E£]+$");

        public static void Init()
        {
            server.RegisterEventHandler("curiosity:Server:Chat:Message", new Action<CitizenFX.Core.Player, string>(ProcessMessage));
        }

        static void ProcessMessage([FromSource]CitizenFX.Core.Player player, string message)
        {
            if (!SessionManager.PlayerList.ContainsKey(player.Handle)) return;

            Session session = SessionManager.PlayerList[player.Handle];

            if (string.IsNullOrWhiteSpace(message))
            {
                API.CancelEvent();
                return;
            }

            if (!regex.Match(message).Success)
            {
                API.CancelEvent();
                return;
            }

            if (message.Length == 0 || message.Length > 240)
            {
                message = message.Substring(0, 240);
            }

            if (message.ContainsProfanity())
            {
                Regex wordFilter = new Regex($"({string.Join("|", ProfanityFilter.ProfanityArray())})");
                message = wordFilter.Replace(message, "$!\"£^!@");
            }

            if (message.IsAllUpper())
            {
                message.ToLower();
            }

            ChatMessage chatMessage = new ChatMessage();

            chatMessage.color = $"{session.Privilege}".ToLower();
            chatMessage.role = $"{session.Privilege}";
            chatMessage.list = "chat";
            chatMessage.message = message;
            chatMessage.roleClass = $"{session.Privilege}";
            chatMessage.name = session.Player.Name;
            chatMessage.job = $"{session.job}";

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(chatMessage);
            string encoded = Encode.StringToBase64(json);

            Server.TriggerClientEvent("curiosity:Client:Chat:Message", encoded);

            Server.TriggerEvent("curiosity:Server:Discord:ChatMessage", player.Name, message);
        }
    }
}
