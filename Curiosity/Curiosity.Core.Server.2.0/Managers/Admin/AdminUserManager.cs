using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Util;
using Curiosity.Core.Server.Web;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers.Admin
{
    public class AdminUserManager : Manager<AdminUserManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("user:kick:list", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return null;

                List<LogItem> lst = await Database.Store.ServerDatabase.GetList(LogGroup.Kick);
                return lst;
            }));

            EventSystem.GetModule().Attach("user:kick:submit", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return null;

                int playerToKick = metadata.Find<int>(0);
                int reasonId = metadata.Find<int>(1);
                string reasonText = metadata.Find<string>(2);

                if (!PluginManager.ActiveUsers.ContainsKey(playerToKick))
                {
                    return false;
                }

                Player player = PluginManager.PlayersList[playerToKick];
                CuriosityUser user = PluginManager.ActiveUsers[playerToKick];

                CuriosityUser admin = PluginManager.ActiveUsers[metadata.Sender];

                bool success = await Database.Store.UserDatabase.LogKick(user.UserId, admin.UserId, reasonId, user.Character.CharacterId);

                if (success)
                {
                    player.Drop($"Kicked by {admin.LatestName}\n{reasonText}");

                    DiscordClient.GetModule().SendDiscordStaffLogMessage(admin.LatestName, user.LatestName, "Kick", reasonText);

                    Notify.Send(notification: Notification.NOTIFICATION_INFO, message: $"Player '{user.LatestName}' has been kicked.");
                    
                    return true;
                }
                else
                {
                    return false;
                }
                
            }));

            EventSystem.GetModule().Attach("user:ban:list", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return null;

                List<LogItem> lst = await Database.Store.ServerDatabase.GetList(LogGroup.Ban);
                return lst;
            }));

            EventSystem.GetModule().Attach("user:ban:submit", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return null;

                int playerToBan = metadata.Find<int>(0);
                int reasonId = metadata.Find<int>(1);
                string reasonText = metadata.Find<string>(2);
                bool perm = metadata.Find<bool>(3);
                int duration = metadata.Find<int>(4);
                int durationType = metadata.Find<int>(5);

                // Type: 0 Hours, 1 Days, 2 Weeks, if Perm not required

                if (!PluginManager.ActiveUsers.ContainsKey(playerToBan))
                {
                    return false;
                }

                Player player = PluginManager.PlayersList[playerToBan];
                CuriosityUser user = PluginManager.ActiveUsers[playerToBan];
                CuriosityUser admin = PluginManager.ActiveUsers[metadata.Sender];

                //if (user.IsStaff)
                //{
                //    return false;
                //}

                DateTime bannedUntilTimestamp = DateTime.Now.AddHours(duration);
                string banDurationMessage = $"{duration} hours";

                switch (durationType)
                {
                    case 1: // Days
                        bannedUntilTimestamp = DateTime.Now.AddDays(duration);
                        banDurationMessage = $"{duration} Day(s)";
                        break;
                    case 2: // Weeks
                        bannedUntilTimestamp = DateTime.Now.AddDays(7 * duration);
                        banDurationMessage = $"{duration} Week(s)";
                        break;
                }

                bool success = await Database.Store.UserDatabase.LogBan(user.UserId, admin.UserId, reasonId, user.Character.CharacterId, perm, bannedUntilTimestamp);

                if (success)
                {
                    string banDuration = string.Format("{0} Days", duration);
                    if (perm)
                        banDuration = "Permanently";

                    player.Drop($"Banned by {admin.LatestName}\n{reasonText}\nDuration: {banDurationMessage}");

                    DiscordClient dc = new DiscordClient();
                    dc.SendDiscordStaffLogMessage(admin.LatestName, user.LatestName, "Banned", reasonText, banDurationMessage);

                    Notify.Send(notification: Notification.NOTIFICATION_INFO, message: $"Player '{user.LatestName}' has been banned.<br />Duration: {banDurationMessage}");

                    return true;
                }
                else
                {
                    return false;
                }
            }));

            EventSystem.GetModule().Attach("user:warn:submit", new EventCallback(metadata =>
            {
                try
                {
                    if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;

                    if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return false;

                    Player staff = PluginManager.PlayersList[metadata.Sender];

                    int pid = metadata.Find<int>(0);
                    Player p = PluginManager.PlayersList[pid];

                    string msg = metadata.Find<string>(1);

                    Notify.Send(pid, notification: Notification.NOTIFICATION_WARNING, message: $"{msg}");
                    BaseScript.TriggerClientEvent(player: p, "txAdminClient:warn", staff.Name, msg, "WARNING", "Warned By:", "Hold [SPACE] for 10 seconds to dismiss this message.");

                    DiscordClient.GetModule().SendDiscordStaffLogMessage(staff.Name, p.Name, "WARNING", msg);
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error [USER:WARNING]: {ex.Message}");
                    return false;
                }
            }));

            EventSystem.GetModule().Attach("user:freeze:submit", new EventCallback(metadata =>
            {
                try
                {
                    if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;

                    if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return false;

                    Player player = PluginManager.PlayersList[metadata.Find<int>(0)];

                    bool freezePlayer = player.State.Get($"{StateBagKey.PLAYER_FREEZE}") == null ? true : player.State.Get($"{StateBagKey.PLAYER_FREEZE}");

                    API.FreezeEntityPosition(player.Character.Handle, freezePlayer);

                    player.State.Set($"{StateBagKey.PLAYER_FREEZE}", !freezePlayer, false);

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }));

            EventSystem.GetModule().Attach("user:bring:submit", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return null;

                return null;
            }));

            EventSystem.GetModule().Attach("user:goto:submit", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return null;

                return null;
            }));
        }
    }
}
