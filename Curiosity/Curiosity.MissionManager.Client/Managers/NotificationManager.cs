using Curiosity.MissionManager.Client.Diagnostics;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Events;

namespace Curiosity.MissionManager.Client.Managers
{
    public class NotificationManager : Manager<NotificationManager>
    {
        public override void Begin()
        {
            EventSystem.Attach("system:notification:basic", new EventCallback(metadata =>
            {
                Notify.Custom(metadata.Find<string>(0));
                return null;
            }));

            EventSystem.Attach("mission:notification:success", new EventCallback(metadata =>
            {
                string message = metadata.Find<string>(0);
                string position = metadata.Find<string>(1);

                Logger.Debug($"notification:success {message}/{position}");

                Notify.Success(message, position);
                return null;
            }));

            EventSystem.Attach("mission:notification:info", new EventCallback(metadata =>
            {
                string message = metadata.Find<string>(0);
                string position = metadata.Find<string>(1);

                Logger.Debug($"notification:info {message}/{position}");

                Notify.Info(message, position);
                return null;
            }));

            EventSystem.Attach("mission:notification:warning", new EventCallback(metadata =>
            {
                string message = metadata.Find<string>(0);
                string position = metadata.Find<string>(1);

                Logger.Debug($"notification:warning {message}/{position}");

                Notify.Warning(message, position);
                return null;
            }));

            EventSystem.Attach("mission:notification:fail", new EventCallback(metadata =>
            {
                string message = metadata.Find<string>(0);
                string position = metadata.Find<string>(1);

                Logger.Debug($"notification:fail {message}/{position}");

                Notify.Warning(message, position);
                return null;
            }));

            EventSystem.Attach("mission:notification:show", new EventCallback(metadata =>
            {
                string message = metadata.Find<string>(0);
                string position = metadata.Find<string>(1);

                Logger.Debug($"notification:show {message}/{position}");

                Notify.Show(message, position);
                return null;
            }));
        }
    }
}
