using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using CitizenFX.Core;
using System;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class Notifications
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Notification:Advanced", new Action<string, int, string, string, string, int>(Advanced));
            client.RegisterEventHandler("curiosity:Client:Notification:Simple", new Action<string>(Simple));
        }

        static void Simple(string message)
        {
            Screen.ShowNotification(message);
        }

        static public void Advanced(string icon, int iconType, string title, string subject, string message, int gtaColorId)
        {
            API.SetNotificationBackgroundColor(gtaColorId);
            API.SetNotificationTextEntry("STRING");
            API.AddTextComponentString(message);
            API.SetNotificationMessage(icon, icon, false, iconType, title, subject);
            API.DrawNotification(false, true);
        }
    }
}
