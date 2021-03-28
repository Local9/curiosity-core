using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
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

                if (user.IsStaff)
                {
                    return false;
                }

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
                int duration = metadata.Find<int>(3);

                if (!PluginManager.ActiveUsers.ContainsKey(playerToBan))
                {
                    return false;
                }

                Player player = PluginManager.PlayersList[playerToBan];
                CuriosityUser user = PluginManager.ActiveUsers[playerToBan];
                CuriosityUser admin = PluginManager.ActiveUsers[metadata.Sender];

                if (user.IsStaff)
                {
                    return false;
                }

                DateTime banUntil = DateTime.Now.AddYears(10);
                bool perm = false;

                string banDurationMessage = "Permanently";

                switch (duration)
                {
                    case 1:
                        banUntil = DateTime.Now.AddHours(2);
                        banDurationMessage = "2 Hours";
                        break;
                    case 2:
                        banUntil = DateTime.Now.AddHours(8);
                        banDurationMessage = "8 Hours";
                        break;
                    case 3:
                        banUntil = DateTime.Now.AddDays(1);
                        banDurationMessage = "1 Day";
                        break;
                    case 4:
                        banUntil = DateTime.Now.AddDays(2);
                        banDurationMessage = "2 Days";
                        break;
                    case 5:
                        banUntil = DateTime.Now.AddDays(7);
                        banDurationMessage = "1 Week";
                        break;
                    case 6:
                        banUntil = DateTime.Now.AddDays(14);
                        banDurationMessage = "2 Weeks";
                        break;
                    case 7:
                        perm = true;
                        break;
                }

                DateTime bannedUntilTimestamp = banUntil;

                bool success = await Database.Store.UserDatabase.LogBan(user.UserId, admin.UserId, reasonId, user.Character.CharacterId, perm, bannedUntilTimestamp);

                if (success)
                {
                    player.Drop($"Banned by {admin.LatestName}\n{reasonText}\nDuration: {banDurationMessage}");

                    DiscordClient.GetModule().SendDiscordStaffLogMessage(admin.LatestName, user.LatestName, "Banned", reasonText, banDurationMessage);

                    Notify.Send(notification: Notification.NOTIFICATION_INFO, message: $"Player '{user.LatestName}' has been banned.<br />Duration: {banDurationMessage}");

                    QueueManager.GetModule().RemoveFrom(user.License, true, true, true, true, true, true); // Nuke them from the queue

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

            EventSystem.GetModule().Attach("user:revive:submit", new EventCallback(metadata =>
            {
                try
                {
                    if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;

                    CuriosityUser staff = PluginManager.ActiveUsers[metadata.Sender];

                    if (!staff.IsStaff) return false;

                    int pid = metadata.Find<int>(0);
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[pid];
                    curiosityUser.Send("character:respawnNow");

                    Notify.Send(pid, notification: Notification.NOTIFICATION_SUCCESS, message: $"Revived by '{staff.LatestName}'");

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }));

            EventSystem.GetModule().Attach("user:bring:submit", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;

                if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return false;

                Player staff = PluginManager.PlayersList[metadata.Sender];

                int pid = metadata.Find<int>(0);

                int bucket = API.GetPlayerRoutingBucket($"{pid}");
                int staffBucket = API.GetPlayerRoutingBucket($"{metadata.Sender}");

                if (staffBucket != bucket)
                {
                    Notify.Send(metadata.Sender, Notification.NOTIFICATION_WARNING, "Cannot bring player from another world");
                    return false;
                }

                Player player = PluginManager.PlayersList[pid];
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[pid];
                Vector3 pos = staff.Character.Position + new Vector3(0f, 2f, 0f);
                curiosityUser.Send("user:position:move", pos.X, pos.Y, pos.Z);

                return true;
                
                //bool fadedOut = await EventSystem.Request<bool>("user:screen:fadeOut", pid, 2000);
               
                //if (fadedOut)
                //{
                //    player.Character.Position = staff.Character.Position + new Vector3(0f, 2f, 0f);
                //    await BaseScript.Delay(500);
                //    await EventSystem.Request<bool>("user:screen:fadeIn", pid, 2000);
                //    return true;
                //}
                //else
                //{
                //    await EventSystem.Request<bool>("user:screen:fadeIn", pid, 2000);
                //    return false;
                //}
            }));

            EventSystem.GetModule().Attach("user:goto:submit", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;

                if (!PluginManager.ActiveUsers[metadata.Sender].IsStaff) return false;

                Player staff = PluginManager.PlayersList[metadata.Sender];
                CuriosityUser staffUser = PluginManager.ActiveUsers[metadata.Sender];

                int pid = metadata.Find<int>(0);
                Player player = PluginManager.PlayersList[pid];
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[pid];

                int bucket = API.GetPlayerRoutingBucket($"{pid}");
                int staffBucket = API.GetPlayerRoutingBucket($"{metadata.Sender}");

                if (staffBucket != bucket)
                {
                    API.SetPlayerRoutingBucket($"{metadata.Sender}", bucket);
                    Notify.Send(metadata.Sender, Notification.NOTIFICATION_WARNING, "Changing world");
                }

                Vector3 pos = player.Character.Position + new Vector3(0f, 2f, 0f);
                staffUser.Send("user:position:move", pos.X, pos.Y, pos.Z);

                return true;

                //bool fadedOut = await EventSystem.Request<bool>("user:screen:fadeOut", metadata.Sender, 2000);

                //if (fadedOut)
                //{
                //    staff.Character.Position = player.Character.Position + new Vector3(0f, 2f, 0f);
                //    await BaseScript.Delay(500);
                //    await EventSystem.Request<bool>("user:screen:fadeIn", metadata.Sender, 2000);
                //    return true;
                //}
                //else
                //{
                //    await EventSystem.Request<bool>("user:screen:fadeIn", metadata.Sender, 2000);
                //    return false;
                //}
            }));
        }
    }
}
