using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Client.Interface
{
    class Chat
    {
        internal static void SendLocalMessage(string message)
        {
            JsonBuilder jsonBuilder = new JsonBuilder();
            jsonBuilder.Add("operation", "CHAT");
            jsonBuilder.Add("subOperation", "NEW_MESSAGE");
            jsonBuilder.Add("message", new
            {
                role = $"SERVER",
                channel = "log",
                activeJob = string.Empty,
                timestamp = DateTime.Now.ToString("HH:mm"),
                name = "SERVER",
                message = message
            });

            string nuiMessage = jsonBuilder.Build();

            PluginManager.Instance.ExportDictionary["curiosity-ui"].AddToChat(nuiMessage);
        }
    }
}
