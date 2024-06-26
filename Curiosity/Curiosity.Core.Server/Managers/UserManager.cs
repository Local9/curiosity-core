﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Core.Server.Extensions;
using Curiosity.Core.Server.Web;
using Curiosity.Core.Server.Web.Discord.Entity;
using Curiosity.Systems.Library.Entity;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {

        public override void Begin()
        {
            EventSystem.Attach("user:is:role", new AsyncEventCallback(async metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];

                if (player == null)
                {
                    return null;
                }

                CuriosityUser curiosityUser = await Database.Store.UserDatabase.Get(player);
                return (int)curiosityUser.Role == metadata.Find<int>(0);
            }));

            EventSystem.Attach("user:get:playerlist", new EventCallback(metadata =>
            {
                List<CuriosityPlayerListItem> lst = new List<CuriosityPlayerListItem>();

                foreach (KeyValuePair<int, CuriosityUser> kv in PluginManager.ActiveUsers)
                {
                    CuriosityUser curiosityUser = kv.Value;

                    CuriosityPlayerListItem cpl = new CuriosityPlayerListItem();
                    cpl.UserId = curiosityUser.UserId;
                    cpl.ServerHandle = kv.Key;
                    cpl.Name = curiosityUser.LatestName;

                    if (!string.IsNullOrEmpty(curiosityUser.JobCallSign))
                        cpl.Name = $"[{curiosityUser.JobCallSign}] {curiosityUser.LatestName}";

                    cpl.Ping = PluginManager.PlayersList[kv.Key].Ping;
                    cpl.Job = curiosityUser.CurrentJob;
                    cpl.Role = curiosityUser.Role.GetStringValue();
                    cpl.RoutingBucket = (int)curiosityUser.RoutingBucket;
                    cpl.DiscordId = curiosityUser.DiscordId;

                    lst.Add(cpl);
                }

                return lst;
            }));

            EventSystem.Attach("user:login", new AsyncEventCallback(async metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];

                if (player == null)
                {
                    return null;
                }

                CuriosityUser curiosityUser = await Database.Store.UserDatabase.Get(player);

                await BaseScript.Delay(0);

                if (curiosityUser is null)
                    return null;

                curiosityUser.Handle = metadata.Sender;

                PluginManager.ActiveUsers.TryAdd(metadata.Sender, curiosityUser);

                if (curiosityUser.IsStaff)
                {
                    player.State.Set($"{StateBagKey.STAFF_MEMBER}", curiosityUser.IsStaff, true);
                    if (curiosityUser.IsDeveloper)
                    {
                        player.State.Set($"{StateBagKey.PLAYER_DEBUG_NPC}", false, true);
                        player.State.Set($"{StateBagKey.PLAYER_DEBUG_VEH}", false, true);
                        player.State.Set($"{StateBagKey.PLAYER_DEBUG_UI}", false, true);
                    }
                }

                player.State.Set($"{StateBagKey.PLAYER_MENU}", false, true);
                player.State.Set($"{StateBagKey.PLAYER_ASSISTING}", false, true);

                string discordIdStr = player.Identifiers["discord"];

                if (!string.IsNullOrWhiteSpace(discordIdStr))
                {
                    ulong discordId = 0;

                    if (ulong.TryParse(discordIdStr, out discordId))
                    {
                        curiosityUser.DiscordId = discordId;
                    }
                }

                Logger.Debug($"[User] [{metadata.Sender}] [{curiosityUser.LatestName}#{curiosityUser.UserId}|{curiosityUser.Role}] Has successfully connected to the server");

                return curiosityUser;
            }));

            EventSystem.Attach("user:login:module", new AsyncEventCallback(async metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];

                if (player == null)
                {
                    return null;
                }

                CuriosityUser curiosityUser = await Database.Store.UserDatabase.Get(player);

                if (curiosityUser is null)
                    return null;

                curiosityUser.Handle = metadata.Sender;

                return curiosityUser;
            }));

            EventSystem.Attach("user:kick:afk", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                player.Drop($"You were kicked from the server for idling too long.");

                return null;
            }));

            EventSystem.Attach("user:log:exception", new AsyncEventCallback(async metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                Logger.Debug($"--- CLIENT EXCEPTION ---");
                Logger.Debug($"Player: {player.Name}");
                Logger.Debug($"Message: {metadata.Find<string>(0)}");
                Logger.Debug($"StackTrace:");
                Logger.Debug($"{metadata.Find<string>(1)}");

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"Client Exception: {DateTime.UtcNow}\n");
                stringBuilder.Append($"Player Name: {player.Name}\n");
                stringBuilder.Append($"Message: {metadata.Find<string>(0)}\n");
                stringBuilder.Append($"Stack:\n{metadata.Find<string>(1)}");

                DiscordClient.GetModule().SendDiscordServerEventLogMessage($"{stringBuilder}");
                await BaseScript.Delay(0);

                return null;
            }));

            Instance.EventRegistry.Add("user:log:exception", new Action<Player, string, string>(OnUserLogException));

            // TODO: Character
            //EventSystem.Attach("user:job", new EventCallback(metadata =>
            //{
            //    return SetUserJobText(metadata.Sender, metadata.Find<string>(0));
            //}));


            // TODO: Character
            EventSystem.Attach("user:personal:vehicle", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                PluginManager.ActiveUsers[metadata.Sender].PersonalVehicle = metadata.Find<int>(0);

                return null;
            }));

            // TODO: Character
            EventSystem.Attach("user:job:notification:backup", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.NotificationBackup = metadata.Find<bool>(0);

                return curiosityUser.NotificationBackup;
            }));

            #region PDA Lists and methods

            EventSystem.Attach("user:report:list", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;
                List<LogItem> lst = await Database.Store.ServerDatabase.GetList(LogGroup.Report);
                return lst;
            }));

            EventSystem.Attach("user:report:submit", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;

                try
                {
                    CuriosityUser reporter = PluginManager.ActiveUsers[metadata.Sender];
                    int playerBeingReportedHandle = metadata.Find<int>(0);
                    string playerBeingReported = metadata.Find<string>(1);
                    string reason = metadata.Find<string>(2);

                    DiscordClient discordClient = DiscordClient.GetModule();
                    DiscordWebhook discordWebhook = discordClient.Webhooks[WebhookChannel.Report];
                    Webhook webhook = new Webhook(discordWebhook.Url);

                    webhook.AvatarUrl = discordWebhook.Avatar;
                    webhook.Content = $"`{DateTime.Now.ToString("yyyy-MM-dd HH:mm")}`";
                    string cleanServerName = discordClient.StripUnicodeCharactersFromString(PluginManager.Hostname);
                    webhook.Username = cleanServerName;

                    AddTestEmbed(discordWebhook, webhook);

                    Embed embed = new Embed();
                    embed.Author = new EmbedAuthor { Name = $"Report" };
                    embed.Title = $"Created by {reporter.LatestName}";
                    embed.Description = $"Reported Player: {playerBeingReported}\n{reason}";
                    embed.Color = (int)DiscordColor.Green;

                    webhook.Embeds.Add(embed);

                    Embed additionalData = new Embed();
                    additionalData.Author = new EmbedAuthor { Name = $"Additional Data on '{playerBeingReported}'" };
                    additionalData.Thumbnail = new EmbedThumbnail { Url = discordWebhook.Avatar };

                    if (PluginManager.ActiveUsers.ContainsKey(playerBeingReportedHandle))
                    {
                        CuriosityUser user = PluginManager.ActiveUsers[playerBeingReportedHandle];
                        additionalData.Title = $"";
                        additionalData.Description = $"This is additional information we have on the player";

                        NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
                        nfi.CurrencyPositivePattern = 0;

                        int playerPedId = API.GetPlayerPed($"{metadata.Sender}");

                        if (API.DoesEntityExist(playerPedId))
                        {
                            user.Character.Health = API.GetEntityHealth(playerPedId);
                        }

                        additionalData.fields.Add(new Field("Life V ID", $"{user.UserId}", true));
                        additionalData.fields.Add(new Field("DiscordID", $"{user.DiscordId}", true));
                        additionalData.fields.Add(new Field("CharacterID", $"{user.Character.CharacterId}", true));
                        additionalData.fields.Add(new Field("Joined", $"{user.DateCreated.ToString("yyyy-MM-dd HH:mm")}"));
                        additionalData.fields.Add(new Field("Health", $"{user.Character.Health}", true));
                        additionalData.fields.Add(new Field("Cash", $"{string.Format(nfi, "{0:C0}", user.Character.Cash)}", true));
                        additionalData.fields.Add(new Field("Ping", $"{API.GetPlayerPing($"{metadata.Sender}")}ms", true));

                        additionalData.Color = (int)DiscordColor.Blue;
                    }
                    else
                    {
                        additionalData.Title = $"Error";
                        additionalData.Description = "Player has left the server as they were being reported";
                        additionalData.Color = (int)DiscordColor.Red;
                    }

                    webhook.Embeds.Add(additionalData);
                    await BaseScript.Delay(0);
                    await webhook.Send();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }));

            #endregion

            // Native Events
            Instance.EventRegistry["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

            // Exports
            Instance.ExportDictionary.Add("GetUser", new Func<string, string>((playerHandle) =>
            {
                int handle = int.Parse(playerHandle);

                if (!PluginManager.ActiveUsers.ContainsKey(handle))
                    return null;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[handle];
                return JsonConvert.SerializeObject(curiosityUser);
            }));

            //Instance.ExportDictionary.Add("SetJob", new Func<string, string, Task<bool>>(
            //    async (playerHandle, jobText) =>
            //    {
            //        int handle = int.Parse(playerHandle);

            //        return (await SetUserJobText(handle, jobText));
            //    }));

            Instance.ExportDictionary.Add("SetPlayerBucket", new Func<string, int, string>((playerHandle, bucket) =>
            {
                ExportMessage exportMessage = new ExportMessage();

                int handle = int.Parse(playerHandle);

                if (!PluginManager.ActiveUsers.ContainsKey(handle))
                {
                    exportMessage.error = "Player not found";
                }
                else
                {
                    PluginManager.ActiveUsers[handle].RoutingBucket = bucket;
                    API.SetPlayerRoutingBucket(playerHandle, bucket);
                }

                return $"{exportMessage}";
            }));

            Instance.ExportDictionary.Add("UserRole", new Func<string, string>((playerHandle) =>
            {
                ExportMessage exportMessage = new ExportMessage();

                int handle = int.Parse(playerHandle);

                if (!PluginManager.ActiveUsers.ContainsKey(handle))
                {
                    exportMessage.error = "Player not found";
                }
                else
                {
                    exportMessage.roleId = (int)PluginManager.ActiveUsers[handle].Role;
                }

                return $"{exportMessage}";
            }));
        }

        private async void OnUserLogException([FromSource] Player player, string message, string stack)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"Client Exception: {DateTime.UtcNow}\n");
            stringBuilder.Append($"Player Name: {player.Name}\n");
            stringBuilder.Append($"Message: {message}\n");
            stringBuilder.Append($"Stack:\n{stack}");

            DiscordClient.GetModule().SendDiscordServerEventLogMessage($"{stringBuilder}");
            await BaseScript.Delay(0);
        }

        private void AddTestEmbed(DiscordWebhook discordWebhook, Webhook webhook)
        {
            if (!PluginManager.IsLive)
            {
                Embed testEmbed = new Embed();
                testEmbed.Author = new EmbedAuthor { Name = $"TEST", IconUrl = discordWebhook.Avatar };
                testEmbed.Title = $"This is a test";
                testEmbed.Color = (int)DiscordColor.Orange;

                webhook.Embeds.Add(testEmbed);
            }
        }

        async void OnPlayerDropped([FromSource] Player player, string reason)
        {
            try
            {
                DiscordClient discordClient = DiscordClient.GetModule();

                int playerHandle = int.Parse(player.Handle);
                if (PluginManager.ActiveUsers.ContainsKey(playerHandle))
                {
                    StateBag stateBag = player.State;

                    CuriosityUser curUser = PluginManager.ActiveUsers[playerHandle];

                    int playerPed = API.GetPlayerPed(player.Handle);

                    try
                    {
                        bool isJailed = stateBag.Get(StateBagKey.IS_JAILED) ?? false;
                        curUser.Character.IsWanted = isJailed;

                        if (curUser.Character.IsWanted && reason == "Exiting")
                        {
                            long moneyToTake = (long)(curUser.Character.Cash * 0.05f);

                            if ((curUser.Character.Cash - (ulong)moneyToTake) < 0)
                                moneyToTake = (long)curUser.Character.Cash;

                            if (moneyToTake > 0)
                            {
                                await Database.Store.BankDatabase.Adjust(curUser.Character.CharacterId, moneyToTake * -1);
                                string msg = $"Player '{curUser.LatestName}' has Disconnected while wanted and has been fined ${moneyToTake:N0}.";
                                Logger.Debug(msg);
                                ChatManager.OnLogMessage(msg);
                                discordClient.SendDiscordPlayerLogMessage(msg);
                            }
                            
                        }

                        if (API.DoesEntityExist(playerPed))
                        {
                            Vector3 pos = API.GetEntityCoords(playerPed);

                            curUser.Character.LastPosition = new Position(pos.X, pos.Y, pos.Z);

                            int playerPedHealth = API.GetEntityHealth(playerPed);
                            curUser.Character.IsDead = playerPedHealth == 0;
                            curUser.Character.Health = playerPedHealth;
                            curUser.Character.Armor = API.GetPedArmour(playerPed);
                            Logger.Debug($"Player: '{curUser.LatestName}' position saved, health {playerPedHealth}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Player doesn't exist, not saving location or details.");
                    }

                    await curUser.Character.Save();

                    bool userRemoved = PluginManager.ActiveUsers.TryRemove(playerHandle, out CuriosityUser curiosityUserOld);

                    if (curUser.PersonalVehicle > 0) EntityManager.EntityInstance.NetworkDeleteEntity(curUser.PersonalVehicle);
                    await BaseScript.Delay(100);
                    if (curUser.PersonalBoat > 0) EntityManager.EntityInstance.NetworkDeleteEntity(curUser.PersonalBoat);
                    await BaseScript.Delay(100);
                    if (curUser.PersonalPlane > 0) EntityManager.EntityInstance.NetworkDeleteEntity(curUser.PersonalPlane);
                    await BaseScript.Delay(100);
                    if (curUser.PersonalTrailer > 0) EntityManager.EntityInstance.NetworkDeleteEntity(curUser.PersonalTrailer);
                    await BaseScript.Delay(100);
                    if (curUser.PersonalHelicopter > 0) EntityManager.EntityInstance.NetworkDeleteEntity(curUser.PersonalHelicopter);
                    await BaseScript.Delay(100);
                    if (curUser.StaffVehicle > 0) EntityManager.EntityInstance.NetworkDeleteEntity(curUser.StaffVehicle);

                    if (API.GetResourceState("npwd") == "started")
                    {
                        if (Instance.ExportDictionary["npwd"] is not null)
                        {
                            var npwd = Instance.ExportDictionary["npwd"];
                            npwd.unloadPlayer(playerHandle);
                        }
                    }

                    QueueManager.GetModule().OnPlayerDropped(player, reason);

                    ChatManager.OnLogMessage($"Player '{curUser.LatestName}' has Disconnected: '{reason}'");
                    Logger.Debug($"Player: {curUser.LatestName} disconnected ({reason}), UR: {userRemoved}");
                    discordClient.SendDiscordPlayerLogMessage($"Player '{curUser.LatestName}' has Disconnected: '{reason}'");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "OnPlayerDropped");
            }
        }
    }
}
