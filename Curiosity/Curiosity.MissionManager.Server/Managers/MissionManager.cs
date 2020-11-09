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
            EventSystem.GetModule().Attach("mission:isActive", new AsyncEventCallback(async metadata =>
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

            EventSystem.GetModule().Attach("mission:activate", new AsyncEventCallback(async metadata =>
            {

                int senderHandle = metadata.Sender;
                string missionId = metadata.Find<string>(0);
                bool missionUnique = metadata.Find<bool>(1);

                try
                {
                    MissionData missionData = new MissionData(missionId, senderHandle, missionUnique);
                    return ActiveMissions.TryAdd(senderHandle, missionData);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to activate mission");
                    return false;
                }
            }));

            EventSystem.GetModule().Attach("mission:deactivate", new AsyncEventCallback(async metadata =>
            {

                int senderHandle = metadata.Sender;

                try
                {
                    return ActiveMissions.TryRemove(senderHandle, out MissionData old);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Failed to activate mission");
                    return false;
                }
            }));

            EventSystem.GetModule().Attach("mission:assistance:request", new AsyncEventCallback(async metadata =>
            {
                return false;
            }));

            EventSystem.GetModule().Attach("mission:assistance:accept", new AsyncEventCallback(async metadata =>
            {
                return false;
            }));

            EventSystem.GetModule().Attach("mission:completed", new AsyncEventCallback(async metadata =>
            {
                return false;
            }));
        }
    }
}
