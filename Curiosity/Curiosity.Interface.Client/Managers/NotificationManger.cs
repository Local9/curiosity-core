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

            Instance.ExportRegistry.Add("Notification", new Func<int, string, string, bool>(
                (notification, title, message) =>
                {
                    SendNui((Notification)notification, title, message);
                    return true;
                }));

            EventSystem.Attach("ui:notification", new EventCallback(metadata =>
            {
                Notification notification = (Notification)metadata.Find<int>(0);
                string title = metadata.Find<string>(1);
                string message = metadata.Find<string>(2);

                SendNui(notification, title, message);

                return true;
            }));
        }

        public void SendNui(Notification notification, string title, string message)
        {
            JsonBuilder jb = new JsonBuilder()
            .Add("operation", $"{notification}")
            .Add("title", title)
            .Add("message", message);

            API.SendNuiMessage(jb.Build());
        }
    }
}
