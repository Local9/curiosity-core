﻿using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Entity;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override void Begin()
        {
            EventSystem.GetModule().Attach("user:login", new AsyncEventCallback(async metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];

                if (player == null)
                {
                    return null;
                }

                CuriosityUser curiosityUser = await Database.Store.UserDatabase.Get(player);

                Logger.Debug($"[User] [{metadata.Sender}] [{curiosityUser.LatestName}#{curiosityUser.UserId}|{curiosityUser.Role}] Has successfully connected to the server");

                curiosityUser.Handle = metadata.Sender;

                PluginManager.ActiveUsers.TryAdd(metadata.Sender, curiosityUser);

                return curiosityUser;
            }));

            EventSystem.GetModule().Attach("user:getProfile", new AsyncEventCallback(async metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                    return null;

                return PluginManager.ActiveUsers[metadata.Sender];
            }));

            EventSystem.GetModule().Attach("user:getSkills", new EventCallback(metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];
                string exportResponse = Instance.ExportDictionary["curiosity-server"].GetSkills(player.Handle);
                List<CharacterSkill> returnVal = JsonConvert.DeserializeObject<List<CharacterSkill>>(exportResponse);
                return returnVal;
            }));

            EventSystem.GetModule().Attach("user:getStats", new EventCallback(metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];
                string exportResponse = Instance.ExportDictionary["curiosity-server"].GetStats(player.Handle);
                List<CharacterSkill> returnVal = JsonConvert.DeserializeObject<List<CharacterSkill>>(exportResponse);
                return returnVal;
            }));

            EventSystem.GetModule().Attach("user:license:weapon", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                return null;
            }));

            EventSystem.GetModule().Attach("user:log:exception", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                Logger.Debug($"--- CLIENT EXCEPTION ---");
                Logger.Debug($"Player: {player.Name}");
                Logger.Debug($"Message: {metadata.Find<string>(0)}");
                Logger.Debug($"StackTrace:");
                Logger.Debug($"{metadata.Find<string>(1)}");

                return null;
            }));
            
            EventSystem.GetModule().Attach("user:job", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                PluginManager.ActiveUsers[metadata.Sender].CurrentJob = metadata.Find<string>(0);

                return null;
            }));

            EventSystem.GetModule().Attach("user:personal:vehicle", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                PluginManager.ActiveUsers[metadata.Sender].PersonalVehicle = metadata.Find<int>(0);

                return null;
            }));

            EventSystem.GetModule().Attach("user:job:notification:backup", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return null;

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.NotificationBackup = metadata.Find<bool>(0);

                return curiosityUser.NotificationBackup;
            }));

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
        }

        static void OnPlayerDropped([FromSource] Player player, string reason)
        {
            int playerHandle = int.Parse(player.Handle);
            if (PluginManager.ActiveUsers.ContainsKey(playerHandle))
            {
                CuriosityUser curUser = PluginManager.ActiveUsers[playerHandle];

                bool userRemoved = PluginManager.ActiveUsers.TryRemove(playerHandle, out CuriosityUser curiosityUserOld);
                bool userHadMission = MissionManager.ActiveMissions.ContainsKey(playerHandle);

                if (userHadMission)
                {
                    MissionData mission = MissionManager.ActiveMissions[playerHandle];
                    foreach (int partyMember in mission.PartyMembers)
                    {
                        EventSystem.GetModule().Send("mission:backup:completed", partyMember);
                    }
                }

                bool failuresRemoved = MissionManager.FailureTracker.TryRemove(curUser.UserId, out int numFailed);
                bool missionRemoved = MissionManager.ActiveMissions.TryRemove(playerHandle, out MissionData old);

                Logger.Debug($"Player: {player.Name} disconnected ({reason}), UR: {userRemoved}, HM: {userHadMission}, MR: {missionRemoved}, FR: {failuresRemoved}");
            }
        }
    }
}
