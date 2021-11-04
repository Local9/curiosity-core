using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Client.Managers
{
    public class NotificationManager : Manager<NotificationManager>
    {
        public override void Begin()
        {
            Instance.ExportDictionary.Add("Notification", new Func<int, string, string, string, int, bool, bool, bool>(
                (notification, message, position, theme, duration, autoClose, dismissible) =>
                {
                    SendNui((Notification)notification, message, position, theme, duration, autoClose, dismissible);
                    return true;
                }));

            EventSystem.Attach("ui:notification", new EventCallback(metadata =>
            {
                Notification notification = (Notification)metadata.Find<int>(0);
                string message = metadata.Find<string>(1);
                string position = metadata.Find<string>(2);
                string theme = metadata.Find<string>(3);

                int duration = 10000;
                if (notification == Notification.NOTIFICATION_ANNOUNCEMENT)
                {
                    notification = Notification.NOTIFICATION_SHOW;
                    duration = 30000;
                }

                SendNui(notification, message, position, theme, duration);

                return true;
            }));
        }

        public void SendNui(Notification notification, string message, string position = "bottom-right", string theme = "snackbar", int duration = 10000, bool autoClose = true, bool dismissible = false)
        {
            JsonBuilder jb = new JsonBuilder()
            .Add("operation", $"NOTIFICATION")
            .Add("type", $"{notification}")
            .Add("message", message)
            .Add("position", position)
            .Add("theme", theme)
            .Add("autoClose", autoClose)
            .Add("dismissible", dismissible)
            .Add("duration", duration);

            string jsonMessage = jb.Build();

            API.SendNuiMessage(jsonMessage);
        }

        internal void Loader(string message, string position = "bottom-right")
        {
            SendNui(Notification.NOTIFICATION_LOADER, message, position);
        }

        internal void Success(string message, string position = "bottom-right")
        {
            SendNui(Notification.NOTIFICATION_SUCCESS, message, position);
        }

        internal void Warn(string message, string position = "bottom-right")
        {
            SendNui(Notification.NOTIFICATION_WARNING, message, position);
        }

        internal void Info(string message, string position = "bottom-right")
        {
            SendNui(Notification.NOTIFICATION_INFO, message, position);
        }

        internal void Error(string message, string position = "bottom-right")
        {
            SendNui(Notification.NOTIFICATION_ERROR, message, position);
        }
    }
}
