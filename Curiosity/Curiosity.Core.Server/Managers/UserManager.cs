using CitizenFX.Core;
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
            EventSystem.GetModule().Attach("user:get:playerlist", new EventCallback(metadata =>
            {
                List<CuriosityPlayerList> lst = new List<CuriosityPlayerList>();

                foreach(KeyValuePair<int, CuriosityUser> kv in PluginManager.ActiveUsers)
                {
                    CuriosityUser curiosityUser = kv.Value;
                    
                    CuriosityPlayerList cpl = new CuriosityPlayerList();
                    cpl.UserId = curiosityUser.UserId;
                    cpl.ServerHandle = kv.Key;
                    cpl.Name = curiosityUser.LatestName;
                    cpl.Ping = PluginManager.PlayersList[kv.Key].Ping;
                    cpl.Job = curiosityUser.CurrentJob;

                    lst.Add(cpl);
                }

                return lst;
            }));

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

            EventSystem.GetModule().Attach("user:kick:afk", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];

                player.Drop($"You were kicked from the server for idling too long.");

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

                EntityManager.EntityInstance.NetworkDeleteEntity(curUser.PersonalVehicle);
                bool failuresRemoved = MissionManager.FailureTracker.TryRemove(curUser.UserId, out int numFailed);
                bool missionRemoved = MissionManager.ActiveMissions.TryRemove(playerHandle, out MissionData old);

                ChatManager.OnLogMessage($"Player '{player.Name}' has Disconneded: '{reason}'");

                Logger.Debug($"Player: {player.Name} disconnected ({reason}), UR: {userRemoved}, HM: {userHadMission}, MR: {missionRemoved}, FR: {failuresRemoved}");
            }
        }
    }
}
