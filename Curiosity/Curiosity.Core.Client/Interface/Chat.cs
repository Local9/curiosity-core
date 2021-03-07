using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Client.Interface
{
    class Chat
    {
        internal static void SendLocalMessage(string message, string channel = "log", string name = "SERVER")
        {
            JsonBuilder jsonBuilder = new JsonBuilder();
            jsonBuilder.Add("operation", "CHAT");
            jsonBuilder.Add("subOperation", "NEW_MESSAGE");
            jsonBuilder.Add("message", new
            {
                role = $"SERVER",
                channel = channel,
                activeJob = string.Empty,
                timestamp = DateTime.Now.ToString("HH:mm"),
                name = name,
                message = message
            });

            string nuiMessage = jsonBuilder.Build();

            API.SendNuiMessage(nuiMessage);
        }
    }
}
