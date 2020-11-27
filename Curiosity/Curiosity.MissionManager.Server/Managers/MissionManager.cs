using Curiosity.MissionManager.Server.Diagnostics;
using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Events;
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

                try
                {
                    Logger.Debug($"mission:activate > {missionId}");

                    MissionData missionData = new MissionData(missionId, senderHandle, missionUnique);
                    return ActiveMissions.TryAdd(senderHandle, missionData);
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

                    Logger.Debug($"mission:deactivate > {removed}:{old.ID}");

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

                int networkId = metadata.Find<int>(0);

                return missionData.AddNetworkPed(networkId);
            }));

            EventSystem.GetModule().Attach("mission:add:vehicle", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return false;

                int networkId = metadata.Find<int>(0);

                return missionData.AddNetworkVehicle(networkId);
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

                missionData.AssistanceRequested = true;

                return false;
            }));

            EventSystem.GetModule().Attach("mission:assistance:list", new EventCallback(metadata =>
            {
                return ActiveMissions.Where(x => x.Value.AssistanceRequested && x.Key != metadata.Sender);
            }));

            EventSystem.GetModule().Attach("mission:leave", new EventCallback(metadata =>
            {
                int missionOwnerId = metadata.Find<int>(0);

                if (!ActiveMissions.ContainsKey(missionOwnerId)) return false;

                return ActiveMissions[missionOwnerId].RemoveMember(metadata.Sender);
            }));

            EventSystem.GetModule().Attach("mission:get:data", new EventCallback(metadata =>
            {
                return GetMissionData(metadata.Sender);
            }));

            EventSystem.GetModule().Attach("mission:assistance:accept", new EventCallback(metadata =>
            {
                int missionOwnerId = metadata.Find<int>(0);

                if (!ActiveMissions.ContainsKey(missionOwnerId)) return false;

                return ActiveMissions[missionOwnerId].AddMember(metadata.Sender);
            }));

            EventSystem.GetModule().Attach("mission:completed", new EventCallback(metadata =>
            {
                if (!ActiveMissions.ContainsKey(metadata.Sender)) return null;

                var player = PluginManager.PlayersList[metadata.Sender];

                if (player == null) return false;

                string missionId = ActiveMissions[metadata.Sender].ID;
                bool passed = metadata.Find<bool>(0);
                int numberTransportArrested = metadata.Find<int>(1);

                bool res = Instance.ExportDictionary["curiosity-server"].MissionComplete(player.Handle, missionId, passed, numberTransportArrested);

                ActiveMissions.TryRemove(metadata.Sender, out MissionData old);

                return res;
            }));
        }

        MissionData GetMissionData(int senderHandle)
        {
            if (!ActiveMissions.ContainsKey(senderHandle)) return null;
            return ActiveMissions[senderHandle];
        }
    }
}
