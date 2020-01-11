using System.Collections.Generic;
using System.Drawing;
using CitizenFX.Core;

namespace Atlas.Roleplay.Client.Interface
{
    public class Chat
    {
        public static void SendLocalMessage(string title, string message, Color color)
        {
            BaseScript.TriggerEvent("chat:addMessage", new Dictionary<string, object>
            {
                ["color"] = new[] {color.R, color.G, color.B},
                ["args"] = new[] {title, message}
            });
        }

        public static void SendLocalMessage(string message, Color color)
        {
            BaseScript.TriggerEvent("chat:addMessage", new Dictionary<string, object>
            {
                ["color"] = new[] {color.R, color.G, color.B},
                ["args"] = new[] {message}
            });
        }

        public static void SendGlobalMessage(string title, string message, Color color)
        {
            BaseScript.TriggerServerEvent("chat:global", new Dictionary<string, object>
            {
                ["color"] = new[] {color.R, color.G, color.B},
                ["args"] = new[] {title, message}
            });
        }
        
        public static void SendGlobalMessage(string message, Color color)
        {
            BaseScript.TriggerServerEvent("chat:global", new Dictionary<string, object>
            {
                ["color"] = new[] {color.R, color.G, color.B},
                ["args"] = new[] {message}
            });
        }
    }
}