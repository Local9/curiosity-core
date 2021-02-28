using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Interface.Client.Managers
{
    public class NotificationManger : Manager<NotificationManger>
    {
        public static NotificationManger NotificationInstance;

        public override void Begin()
        {
            NotificationInstance = this;

            Instance.ExportRegistry.Add("Notification", new Func<int, string, string, string, string, bool>(
                (notification, message, position, theme, key) =>
                {
                    SendNui((Notification)notification, message, position, theme, key);
                    return true;
                }));

            EventSystem.Attach("ui:notification", new EventCallback(metadata =>
            {
                Notification notification = (Notification)metadata.Find<int>(0);
                string message = metadata.Find<string>(1);
                string position = metadata.Find<string>(2);
                string theme = metadata.Find<string>(3);
                string key = metadata.Find<string>(4);

                SendNui(notification, message, position, theme, key);

                return true;
            }));
        }

        public void SendNui(Notification notification, string message, string position, string theme, string key)
        {
            JsonBuilder jb = new JsonBuilder()
            .Add("operation", $"{notification}")
            .Add("type", $"{notification}")
            .Add("message", message)
            .Add("position", position)
            .Add("theme", theme)
            .Add("key", key);

            API.SendNuiMessage(jb.Build());
        }
    }
}
