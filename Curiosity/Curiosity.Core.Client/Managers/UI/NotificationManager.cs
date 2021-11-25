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
            Instance.ExportDictionary.Add("Notification", new Func<int, string, string, string, int, bool, bool, bool, bool>(
                (notification, message, position, theme, duration, autoClose, dismissible, playSound) =>
                {
                    SendNui((eNotification)notification, message, position, theme, duration, autoClose, dismissible, playSound);
                    return true;
                }));

            EventSystem.Attach("ui:notification", new EventCallback(metadata =>
            {
                eNotification notification = (eNotification)metadata.Find<int>(0);
                string message = metadata.Find<string>(1);
                string position = metadata.Find<string>(2);
                string theme = metadata.Find<string>(3);
                bool playSound = metadata.Find<bool>(4);

                int duration = 10000;
                if (notification == eNotification.NOTIFICATION_ANNOUNCEMENT)
                {
                    notification = eNotification.NOTIFICATION_SHOW;
                    duration = 30000;
                }

                SendNui(notification, message, position, theme, duration, playSound);

                return true;
            }));

            EventSystem.Attach("system:notification:basic", new EventCallback(metadata =>
            {
                CustomNUI(metadata.Find<string>(0));
                return null;
            }));
        }

        public void SendNui(eNotification notification, string message, string position = "bottom-right", string theme = "snackbar", int duration = 10000, bool autoClose = true, bool dismissible = false, bool playSound = false)
        {
            if (playSound)
            {
                switch (notification)
                {
                    case eNotification.NOTIFICATION_SUCCESS:
                        API.PlaySoundFrontend(-1, "package_delivered_success", "DLC_GR_Generic_Mission_Sounds", true);
                        break;
                    case eNotification.NOTIFICATION_INFO:
                        API.PlaySoundFrontend(-1, "INFO", "HUD_FRONTEND_DEFAULT_SOUNDSET", true);
                        break;
                    case eNotification.NOTIFICATION_ERROR:
                        API.PlaySoundFrontend(-1, "ERROR", "HUD_FREEMODE_SOUNDSET", true);
                        break;
                    case eNotification.NOTIFICATION_WARNING:
                        API.PlaySoundFrontend(-1, "tyre_health_warning", "DLC_sum20_Open_Wheel_Racing_Sounds", true);
                        break;
                    case eNotification.NOTIFICATION_LOADER:
                        API.PlaySoundFrontend(-1, "SELECT", "HUD_FREEMODE_SOUNDSET", true);
                        break;
                    default:
                        API.PlaySoundFrontend(-1, "SELECT", "HUD_FREEMODE_SOUNDSET", true);
                        break;
                }
            }

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

        internal void Loader(string message, string position = "bottom-right", bool playSound = false)
        {
            SendNui(eNotification.NOTIFICATION_LOADER, message, position, playSound: playSound);
        }

        internal void Success(string message, string position = "bottom-right", bool playSound = false)
        {
            SendNui(eNotification.NOTIFICATION_SUCCESS, message, position, playSound: playSound);
        }

        internal void Warn(string message, string position = "bottom-right", bool playSound = false)
        {
            SendNui(eNotification.NOTIFICATION_WARNING, message, position, playSound: playSound);
        }

        internal void Info(string message, string position = "bottom-right", bool playSound = false)
        {
            SendNui(eNotification.NOTIFICATION_INFO, message, position, playSound: playSound);
        }

        internal void Error(string message, string position = "bottom-right", bool playSound = false)
        {
            SendNui(eNotification.NOTIFICATION_ERROR, message, position, playSound: playSound);
        }

        internal void CustomNUI(string message, bool blink = true, bool saveToBrief = true, int bgColor = 2)
        {
            API.SetNotificationTextEntry("CELL_EMAIL_BCON"); // 10x ~a~
            foreach (string s in CitizenFX.Core.UI.Screen.StringToArray(message))
            {
                API.AddTextComponentSubstringPlayerName(s);
            }
            API.SetNotificationBackgroundColor(bgColor);
            API.DrawNotification(blink, saveToBrief);
        }
    }
}
