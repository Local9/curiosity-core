﻿using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;

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
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                List<LogItem> lst = await EventSystem.Request<List<LogItem>>("user:ban:list");

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

            Instance.AttachNuiHandler("BanPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                int playerToBan = metadata.Find<int>(0);
                int reasonId = metadata.Find<int>(1);
                string reasonText = metadata.Find<string>(2);
                int duration = metadata.Find<int>(3);

                bool success = await EventSystem.Request<bool>("user:ban:submit", playerToBan, reasonId, reasonText, duration);

                if (success)
                {
                    NotificationManager.GetModule().Success("Ban Logged.");
                }
                else
                {
                    NotificationManager.GetModule().Warn("User was not banned.");
                }

                return new { success = success };
            }));

            Instance.AttachNuiHandler("KickReasons", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
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
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                int playerToKick = metadata.Find<int>(0);
                int reasonId = metadata.Find<int>(1);
                string reasonText = metadata.Find<string>(2);

                Logger.Debug($"Kick: {playerToKick}, {reasonId}, {reasonText}");

                bool success = await EventSystem.Request<bool>("user:kick:submit", playerToKick, reasonId, reasonText);

                if (success)
                {
                    NotificationManager.GetModule().Success("Kick Logged.");
                }
                else
                {
                    NotificationManager.GetModule().Warn("User was not kicked.");
                }

                return new { success = success };
            }));

            Instance.AttachNuiHandler("FreezePlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                int playerToFreeze = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:freeze:submit", playerToFreeze);

                if (success)
                {
                    NotificationManager.GetModule().Success("User frozen state changed.");
                }
                else
                {
                    NotificationManager.GetModule().Warn("User was not frozen.");
                }

                return new { success = success };
            }));

            Instance.AttachNuiHandler("WarnPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                int playerToWarn = metadata.Find<int>(0);
                string message = metadata.Find<string>(1);

                Logger.Debug($"ptw: {playerToWarn}, msg: {message}");

                bool success = await EventSystem.Request<bool>("user:warn:submit", playerToWarn, message);

                if (success)
                {
                    NotificationManager.GetModule().Success("User warned.");
                }
                else
                {
                    NotificationManager.GetModule().Warn("User was not warned.");
                }

                return new { success = success };
            }));

            Instance.AttachNuiHandler("RevivePlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                int playerToRevive = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:revive:submit", playerToRevive);

                if (success)
                {
                    NotificationManager.GetModule().Success("User revived.");
                }
                else
                {
                    NotificationManager.GetModule().Warn("User was not revived.");
                }

                return new { success = success };
            }));

            Instance.AttachNuiHandler("BringPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                int playerToRevive = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:bring:submit", playerToRevive);

                if (success)
                {
                    NotificationManager.GetModule().Success("User teleported.");
                }
                else
                {
                    NotificationManager.GetModule().Warn("User was not teleported.");
                }

                return new { success = success };
            }));

            Instance.AttachNuiHandler("GotoPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                int playerToRevive = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:bring:submit", playerToRevive);

                if (success)
                {
                    NotificationManager.GetModule().Success("Teleported to User.");
                }
                else
                {
                    NotificationManager.GetModule().Warn("Was not teleported.");
                }

                return new { success = success };
            }));

            Instance.AttachNuiHandler("SpectatePlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                int playerHandle = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:spectate:submit", playerHandle);

                if (success)
                {
                    NotificationManager.GetModule().Success("Spectating Player.");
                }
                else
                {
                    NotificationManager.GetModule().Warn("Unable to spectate player.");
                }

                return new { success = success };
            }));

            Instance.AttachNuiHandler("SpectateStop", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManager.GetModule().Warn("You do not have the permission to use this.");
                    return new { success = false };
                }

                int playerToRevive = metadata.Find<int>(0);

                bool success = await EventSystem.Request<bool>("user:spectate:stop", playerToRevive);

                if (success)
                {
                    NotificationManager.GetModule().Success("Stopped Spectating Player.");
                }
                else
                {
                    NotificationManager.GetModule().Warn("Issue when trying to cancel spectate.");
                }

                return new { success = success };
            }));
        }
    }
}
