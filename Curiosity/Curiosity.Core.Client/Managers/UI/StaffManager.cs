using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.PDA;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

                string jsn = new JsonBuilder().Add("operation", "BAN_REASONS")
                .Add("list", lst)
                .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Instance.AttachNuiHandler("BanPlayer", new AsyncEventCallback(async metadata =>
            {
                if (!Cache.Player.User.IsAdmin)
                {
                    NotificationManger.GetModule().Warn("You do not have the permission to use this.");
                    return null;
                }

                int playerToKick = metadata.Find<int>(0);
                int reasonId = metadata.Find<int>(1);
                string reasonText = metadata.Find<string>(2);
                bool perm = metadata.Find<bool>(3);
                int duration = metadata.Find<int>(4);
                int durationType = metadata.Find<int>(5);

                bool success = await EventSystem.Request<bool>("user:ban:submit", playerToKick, reasonId, reasonText, perm, duration, durationType);

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

                string jsn = new JsonBuilder().Add("operation", "KICK_REASONS")
                .Add("list", lst)
                .Build();

                API.SendNuiMessage(jsn);

                return null;
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
        }
    }
}
