using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client.Managers
{
    public class NotificationManger : Manager<NotificationManger>
    {
        public override void Begin()
        {
            Instance.ExportRegistry.Add("NotificationSuccess", new Func<string, string, bool>(
                (title, message) =>
                {
                    SendNuiMessage("NOTIFICATION_SUCCESS", title, message);
                    return true;
                }));

            Instance.ExportRegistry.Add("NotificationError", new Func<string, string, bool>(
                (title, message) =>
                {
                    SendNuiMessage("NOTIFICATION_ERROR", title, message);
                    return true;
                }));

            Instance.ExportRegistry.Add("NotificationInfo", new Func<string, string, bool>(
                (title, message) =>
                {
                    SendNuiMessage("NOTIFICATION_INFO", title, message);
                    return true;
                }));

            Instance.ExportRegistry.Add("NotificationWarning", new Func<string, string, bool>(
                (title, message) =>
                {
                    SendNuiMessage("NOTIFICATION_WARNING", title, message);
                    return true;
                }));

            Instance.ExportRegistry.Add("NotificationShow", new Func<string, string, bool>(
                (title, message) =>
                {
                    SendNuiMessage("NOTIFICATION_SHOW", title, message);
                    return true;
                }));

            Instance.ExportRegistry.Add("NotificationClear", new Func<bool>(
                () =>
                {
                    SendNuiMessage("NOTIFICATION_CLEAR", string.Empty, string.Empty);
                    return true;
                }));
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
