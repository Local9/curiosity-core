﻿using CitizenFX.Core;
using Curiosity.MissionManager.Server.Diagnostics;
using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Shared.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.MissionManager.Server.Managers
{
    public class MissionManager : Manager<MissionManager>
    {
        public static ConcurrentDictionary<int, MissionData> ActiveMissions = new ConcurrentDictionary<int, MissionData>();
        public static ConcurrentDictionary<long, int> FailureTracker = new ConcurrentDictionary<long, int>();

        public override void Begin()
        {
            EventSystem.GetModule().Attach("mission:isActive", new EventCallback(metadata =>
            {
                int senderHandle = metadata.Sender;
                string missionId = metadata.Find<string>(0);

                List<MissionData> uniqueMissions = ActiveMissions.Values.Select(x => x).Where(x => x.IsMissionUnique).ToList();

                foreach (MissionData missionData in uniqueMissions)
                {
                    if (missionData.ID == missionId)
                        return true;
                }

                return false;
            }));

            EventSystem.GetModule().Attach("mission:activate", new EventCallback(metadata =>
            {

                int senderHandle = metadata.Sender;
                string missionId = metadata.Find<string>(0);
                bool missionUnique = metadata.Find<bool>(1);
                string displayName = metadata.Find<string>(2);

                try
                {
                    Logger.Debug($"mission:activate > {missionId}");

                    MissionData missionData = new MissionData();

                    missionData.ID = missionId;
                    missionData.DisplayName = displayName;
                    missionData.OwnerHandleId = senderHandle;
                    missionData.IsMissionUnique = missionUnique;

                    bool missionCreated = ActiveMissions.TryAdd(senderHandle, missionData);

                    Logger.Debug($"mission:activate Success? > {missionCreated}:{ActiveMissions.Count}");

                    return missionCreated;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to activate mission");
                    return false;
                }
            }));

            EventSystem.GetModule().Attach("mission:deactivate", new EventCallback(metadata =>
            {
                int senderHandle = metadata.Sender;

                try
                {
                    bool removed = ActiveMissions.TryRemove(senderHandle, out MissionData old);

                    Logger.Debug($"mission:deactivate > {removed}:{old?.ID}:{ActiveMissions.Count}");

                    return removed;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to deactivate mission");
                    return false;
                }
            }));

            EventSystem.GetModule().Attach("mission:add:ped", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return false;

                if (missionData.OwnerHandleId != metadata.Sender) return false;

                int networkId = metadata.Find<int>(0);
                bool isSuspect = metadata.Find<bool>(1);
                bool isHandcuffed = metadata.Find<bool>(2);
                bool attachBlip = metadata.Find<bool>(3);
                int gender = metadata.Find<int>(4);

                // Logger.Debug($"NetworkID: {networkId}, Suspect: {isSuspect}, HandCuffed: {isHandcuffed}, Blip: {attachBlip}");

                return missionData.AddNetworkPed(networkId, isSuspect, isHandcuffed, attachBlip, gender);
            }));

            EventSystem.GetModule().Attach("mission:add:vehicle", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return false;

                if (missionData.OwnerHandleId != metadata.Sender) return false;

                int networkId = metadata.Find<int>(0);
                bool isTowable = metadata.Find<bool>(1);
                bool attachBlip = metadata.Find<bool>(2);

                // Logger.Debug($"NetworkID: {networkId}, Towable: {isTowable}, Blip: {attachBlip}");

                return missionData.AddNetworkVehicle(networkId, isTowable, attachBlip);
            }));

            EventSystem.GetModule().Attach("mission:remove:ped", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return false;

                int networkId = metadata.Find<int>(0);

                return missionData.RemoveNetworkPed(networkId);
            }));

            EventSystem.GetModule().Attach("mission:remove:vehicle", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return false;

                int networkId = metadata.Find<int>(0);

                return missionData.RemoveNetworkVehicle(networkId);
            }));

            EventSystem.GetModule().Attach("mission:assistance:request", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return false;

                Player player = PluginManager.PlayersList[metadata.Sender];
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                if (DateTime.Now.Subtract(curiosityUser.LastNotificationBackup).TotalMinutes > 2)
                {
                    curiosityUser.LastNotificationBackup = DateTime.Now;

                    List<CuriosityUser> users = PluginManager.ActiveUsers.Where(x => x.Value.CurrentJob == "police" && x.Value.NotificationBackup).Select(y => y.Value).ToList();

                    users.ForEach(u =>
                    {
                        EventSystem.GetModule().Send("mission:notification", u.Handle, "Dispatch A.I.", "Back up request", $"Player {player.Name} has requested back up.");
                    });
                }
                else
                {
                    EventSystem.GetModule().Send("mission:notification", curiosityUser.Handle, "Dispatch A.I.", "Back up request", $"Sorry, you cannot request backup currently.");
                }

                missionData.AssistanceRequested = true;

                return true;
            }));

            EventSystem.GetModule().Attach("mission:assistance:accept", new EventCallback(metadata =>
            {
                int missionOwnerId = metadata.Find<int>(0);

                if (!ActiveMissions.ContainsKey(missionOwnerId)) return false;

                MissionData missionData = ActiveMissions[missionOwnerId];
                missionData.AddMember(metadata.Sender);

                return missionData;
            }));

            EventSystem.GetModule().Attach("mission:assistance:list", new EventCallback(metadata =>
            {
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                if (curiosityUser.IsDeveloper)
                    return ActiveMissions.Where(x => x.Value.AssistanceRequested).Select(x => x.Value).ToList();

                return ActiveMissions.Where(x => x.Value.AssistanceRequested && x.Key != metadata.Sender).Select(x => x.Value).ToList();
            }));

            EventSystem.GetModule().Attach("mission:assistance:leave", new EventCallback(metadata =>
            {
                int missionOwnerId = metadata.Find<int>(0);

                if (!ActiveMissions.ContainsKey(missionOwnerId)) return false;

                return ActiveMissions[missionOwnerId].RemoveMember(metadata.Sender);
            }));

            EventSystem.GetModule().Attach("mission:get:data", new EventCallback(metadata =>
            {
                int senderHandle = metadata.Find<int>(0);



                return GetMissionData(metadata.Sender);
            }));

            EventSystem.GetModule().Attach("mission:completed", new AsyncEventCallback(async metadata =>
            {
                if (!ActiveMissions.ContainsKey(metadata.Sender)) return null;

                var player = PluginManager.PlayersList[metadata.Sender];

                if (player == null) return false;

                var curUser = PluginManager.ActiveUsers[metadata.Sender];

                MissionData missionData = ActiveMissions[metadata.Sender];
                missionData.IsCompleted = true;

                await BaseScript.Delay(5000);

                string missionId = missionData.ID;
                bool passed = metadata.Find<bool>(0);
                int numberTransportArrested = metadata.Find<int>(1);

                int numberOfFailures = 0;

                if (!passed)
                {
                    numberOfFailures = FailureTracker.AddOrUpdate(curUser.UserId, 1, (key, oldValue) => oldValue + 1);
                }
                else
                {
                    numberOfFailures = FailureTracker.AddOrUpdate(curUser.UserId, 0, (key, oldValue) => oldValue > 0 ? oldValue - 1 : 0);
                }

                Logger.Debug($"{curUser.LatestName} : NumFail: {numberOfFailures}");

                bool res = Instance.ExportDictionary["curiosity-server"].MissionComplete(player.Handle, missionId, passed, numberTransportArrested, numberOfFailures);

                missionData.PartyMembers.ForEach(async serverHandle =>
                {
                    await BaseScript.Delay(500);
                    if (numberOfFailures >= 3)
                    {
                        Instance.ExportDictionary["curiosity-server"].MissionComplete(serverHandle, missionId, passed, 1, 0);
                        EventSystem.GetModule().Send("mission:backup:completed", serverHandle);
                    }
                    else
                    {
                        EventSystem.GetModule().Send("mission:notification", serverHandle, "No earnings", "The player you've assisted has failed too many times.");
                    }
                });

                return res;
            }));

            EventSystem.GetModule().Attach("mission:ped:identification", new EventCallback(metadata =>
            {

                int senderHandle = metadata.Sender;
                int ownerHandle = metadata.Find<int>(0);
                int netId = metadata.Find<int>(1);

                try
                {
                    MissionData missionData = GetMissionData(ownerHandle);

                    if (missionData == null) return null;

                    if (missionData.NetworkPeds.ContainsKey(netId))
                    {
                        return missionData.NetworkPeds[netId];
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }));
        }

        MissionData GetMissionData(int senderHandle)
        {
            if (ActiveMissions.ContainsKey(senderHandle))
                return ActiveMissions[senderHandle];

            foreach(KeyValuePair<int, MissionData> keyValuePair in ActiveMissions)
            {
                foreach(int playerId in keyValuePair.Value.PartyMembers)
                {
                    if (playerId == senderHandle)
                        return keyValuePair.Value;
                }
            }

            return null;
        }
    }
}
