﻿using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Client.Managers
{
    public class NotificationManger : Manager<NotificationManger>
    {
        public static NotificationManger NotificationInstance;

        public override void Begin()
        {
            NotificationInstance = this;

            Instance.ExportDictionary.Add("Notification", new Func<int, string, string, string, bool>(
                (notification, message, position, theme) =>
                {
                    SendNui((Notification)notification, message, position, theme);
                    return true;
                }));

            EventSystem.Attach("ui:notification", new EventCallback(metadata =>
            {
                Notification notification = (Notification)metadata.Find<int>(0);
                string message = metadata.Find<string>(1);
                string position = metadata.Find<string>(2);
                string theme = metadata.Find<string>(3);

                SendNui(notification, message, position, theme);

                return true;
            }));
        }

        public void SendNui(Notification notification, string message, string position = "bottom-right", string theme = "toast")
        {
            JsonBuilder jb = new JsonBuilder()
            .Add("operation", $"NOTIFICATION")
            .Add("type", $"{notification}")
            .Add("message", message)
            .Add("position", position)
            .Add("theme", theme);

            API.SendNuiMessage(jb.Build());
        }

        public void Success(string message)
        {
            SendNui(Notification.NOTIFICATION_SUCCESS, message);
        }

        internal void Warn(string message)
        {
            SendNui(Notification.NOTIFICATION_WARNING, message);
        }
    }
}