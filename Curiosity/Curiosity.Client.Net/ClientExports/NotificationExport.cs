using CitizenFX.Core.Native;
using Curiosity.Client.net.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.ClientExports
{
    internal class NotificationExport
    {
        static Client Instance = Client.GetInstance();

        public static void Init()
        {
            Instance.RegisterExport("NotificationSuccess", new Func<string, string, bool>(OnNotificationSuccess));
            Instance.RegisterExport("NotificationError", new Func<string, string, bool>(OnNotificationError));
            Instance.RegisterExport("NotificationInfo", new Func<string, string, bool>(OnNotificationInfo));
            Instance.RegisterExport("NotificationWarning", new Func<string, string, bool>(OnNotificationWarning));
            Instance.RegisterExport("NotificationShow", new Func<string, string, bool>(OnNotificationShow));
            Instance.RegisterExport("NotificationClear", new Func<bool>(OnNotificationClear));
        }

        private static bool OnNotificationClear()
        {
            SendNuiMessage("NOTIFICATION_CLEAR", string.Empty, string.Empty);
            return true;
        }

        private static bool OnNotificationShow(string title, string message)
        {
            SendNuiMessage("NOTIFICATION_SHOW", title, message);
            return true;
        }

        private static bool OnNotificationWarning(string title, string message)
        {
            SendNuiMessage("NOTIFICATION_WARNING", title, message);
            return true;
        }

        private static bool OnNotificationInfo(string title, string message)
        {
            SendNuiMessage("NOTIFICATION_INFO", title, message);
            return true;
        }

        private static bool OnNotificationError(string title, string message)
        {
            SendNuiMessage("NOTIFICATION_ERROR", title, message);
            return true;
        }

        private static bool OnNotificationSuccess(string title, string message)
        {
            SendNuiMessage("NOTIFICATION_SUCCESS", title, message);
            return true;
        }

        private static void SendNuiMessage(string type, string title, string message)
        {
            JsonBuilder jb = new JsonBuilder()
            .Add("operation", type)
            .Add("title", title)
            .Add("message", message);

            API.SendNuiMessage(jb.Build());
        }
    }
}
