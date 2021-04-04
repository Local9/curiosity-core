using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;

namespace Curiosity.Core.Client.Managers
{
    public class StaffManager : Manager<StaffManager>
    {
        public override void Begin()
        {
            Instance.AttachNuiHandler("BanReasons", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                List<LogItem> lst = await EventSystem.Request<List<LogItem>>("user:ban:list");

                var reasons = new List<dynamic>();

                foreach(LogItem item in lst)
                {
                    var r = new
                    {
                        logTypeId = item.LogTypeId,
                        group = item.Group,
                        description = item.Description,
                        playerHandle = item.PlayerHandle
                    };
                    reasons.Add(r);
                }

                return reasons;
            }));

            Instance.AttachNuiHandler("BanPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerToBan = metadata.Find<int>(0);
                int reasonId = metadata.Find<int>(1);
                string reasonText = metadata.Find<string>(2);
                int duration = metadata.Find<int>(3);

                bool success = await EventSystem.Request<bool>("user:ban:submit", playerToBan, reasonId, reasonText, duration);

                if (success)
                {
                    NotificationManger.GetModule().Success("Ban Logged.");
                }
                else
                {
                    NotificationManger.GetModule().Warn("User was not banned.");
                }

                return null;
            }));

            Instance.AttachNuiHandler("KickReasons", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                List<LogItem> lst = await EventSystem.Request<List<LogItem>>("user:kick:list");

                var reasons = new List<dynamic>();

                foreach (LogItem item in lst)
                {
                    var r = new
                    {
                        logTypeId = item.LogTypeId,
                        group = item.Group,
                        description = item.Description,
                        playerHandle = item.PlayerHandle
                    };
                    reasons.Add(r);
                }

                return reasons;
            }));

            Instance.AttachNuiHandler("KickPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerToKick = metadata.Find<int>(0);
                int reasonId = metadata.Find<int>(1);
                string reasonText = metadata.Find<string>(2);

                Logger.Debug($"Kick: {playerToKick}, {reasonId}, {reasonText}");

                bool success = await EventSystem.Request<bool>("user:kick:submit", playerToKick, reasonId, reasonText);

                if (success)
                {
                    NotificationManger.GetModule().Success("Kick Logged.");
                }
                else
                {
                    NotificationManger.GetModule().Warn("User was not kicked.");
                }

                return null;
            }));

            Instance.AttachNuiHandler("FreezePlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerToFreeze = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:freeze:submit", playerToFreeze);

                if (success)
                {
                    NotificationManger.GetModule().Success("User frozen state changed.");
                }
                else
                {
                    NotificationManger.GetModule().Warn("User was not frozen.");
                }

                return null;
            }));

            Instance.AttachNuiHandler("WarnPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerToWarn = metadata.Find<int>(0);
                string message = metadata.Find<string>(1);

                Logger.Debug($"ptw: {playerToWarn}, msg: {message}");

                bool success = await EventSystem.Request<bool>("user:warn:submit", playerToWarn, message);

                if (success)
                {
                    NotificationManger.GetModule().Success("User warned.");
                }
                else
                {
                    NotificationManger.GetModule().Warn("User was not warned.");
                }

                return null;
            }));

            Instance.AttachNuiHandler("RevivePlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerToRevive = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:revive:submit", playerToRevive);

                if (success)
                {
                    NotificationManger.GetModule().Success("User revived.");
                }
                else
                {
                    NotificationManger.GetModule().Warn("User was not revived.");
                }

                return null;
            }));

            Instance.AttachNuiHandler("BringPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerToRevive = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:bring:submit", playerToRevive);

                if (success)
                {
                    NotificationManger.GetModule().Success("User teleported.");
                }
                else
                {
                    NotificationManger.GetModule().Warn("User was not teleported.");
                }

                return null;
            }));

            Instance.AttachNuiHandler("GotoPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerToRevive = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:bring:submit", playerToRevive);

                if (success)
                {
                    NotificationManger.GetModule().Success("Teleported to User.");
                }
                else
                {
                    NotificationManger.GetModule().Warn("Was not teleported.");
                }

                return null;
            }));

            Instance.AttachNuiHandler("SpectatePlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerHandle = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:spectate:submit", playerHandle);

                if (success)
                {
                    NotificationManger.GetModule().Success("Spectating Player.");
                }
                else
                {
                    NotificationManger.GetModule().Warn("Unable to spectate player.");
                }

                return null;
            }));

            Instance.AttachNuiHandler("SpectateStop", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerToRevive = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:spectate:stop", playerToRevive);

                if (success)
                {
                    NotificationManger.GetModule().Success("Stopped Spectating Player.");
                }
                else
                {
                    NotificationManger.GetModule().Warn("Issue when trying to cancel spectate.");
                }

                return null;
            }));
        }
    }
}
