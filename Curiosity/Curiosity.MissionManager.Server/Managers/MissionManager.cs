using CitizenFX.Core;
using Curiosity.MissionManager.Server.Diagnostics;
using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using Curiosity.Systems.Shared.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Curiosity.MissionManager.Server.Managers
{
    public class MissionManager : Manager<MissionManager>
    {
        private const string SEARCH_ITEM_GUN = "Gun";
        public static ConcurrentDictionary<int, MissionData> ActiveMissions = new ConcurrentDictionary<int, MissionData>();
        public static ConcurrentDictionary<long, int> FailureTracker = new ConcurrentDictionary<long, int>();

        static int _numberOfSuspectArrested = 0;

        Regex r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

        // Item - Illegal
        Dictionary<string, bool> Items = new Dictionary<string, bool>()
        {
            { "Chewing Gum", false },
            { SEARCH_ITEM_GUN, true },
            { "Knife", true },
            { "Keys", false },
            { "Wallet", false },
            { "Cocaine", true },
            { "Marijuana", true },
            { "Speed", true },
            { "Heroine", true },
            { "Sweet Wrapper", false },
            { "Mobile Phone", false },
            { "Loose Change", false },
            { "Receipt", false },
            { "Gloves", false },
            { "Chapstick", false },
            { "Comb", false },
            { "Headphones", false },
            { "Condom", false },
            { "Camera", false },
            { "Pack of Smokes", false },
            { "Glasses", false },
            { "Torch", false },
            { "Knuckle Duster", true },
            { "Pager", false },
            { "Flick Knife", true },
            { "Watch", false },
        };

        public override void Begin()
        {

            #region Mission Events

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
                    }
                    else
                    {
                        EventSystem.GetModule().Send("mission:notification", serverHandle, "No earnings", "The player you've assisted has failed too many times.");
                    }
                    EventSystem.GetModule().Send("mission:backup:completed", serverHandle);
                });

                return res;
            }));

            #endregion

            #region Mission Ped Events

            EventSystem.GetModule().Attach("mission:add:ped", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return null;

                if (missionData.OwnerHandleId != metadata.Sender) return null;

                int networkId = metadata.Find<int>(0);
                int gender = metadata.Find<int>(1);
                bool isDriver = metadata.Find<bool>(2);

                MissionDataPed missionDataPed = missionData.AddNetworkPed(networkId, gender, isDriver);

                Logger.Debug($"Added ped to mission;\n{missionDataPed}");

                return missionDataPed;
            }));

            EventSystem.GetModule().Attach("mission:remove:ped", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return false;

                int networkId = metadata.Find<int>(0);

                return missionData.RemoveNetworkPed(networkId);
            }));

            EventSystem.GetModule().Attach("mission:ped:identification", new EventCallback(metadata =>
            {

                int senderHandle = metadata.Sender;
                int ownerHandle = metadata.Find<int>(0);
                int netId = metadata.Find<int>(1);
                bool isDriver = metadata.Find<bool>(2);

                try
                {
                    MissionData missionData = GetMissionData(ownerHandle);

                    if (missionData == null)
                    {
                        Logger.Error($"Unable to find mission data");
                        return null;
                    }

                    if (missionData.NetworkPeds.ContainsKey(netId))
                    {
                        MissionDataPed mpd = missionData.NetworkPeds[netId];
                        mpd.IsIdentified = true;
                        mpd.IsDriver = isDriver;
                        return mpd;
                    }

                    Logger.Error($"Unable to find ped in mission");
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }));

            #region Mission Ped Updates

            EventSystem.GetModule().Attach("mission:update:ped:mission", new EventCallback(metadata =>
            {
                MissionDataPed missionDataPed = GetMissionPed(metadata.Sender, metadata.Find<int>(0));

                if (missionDataPed == null) return null;

                missionDataPed.IsMission = metadata.Find<bool>(1);

                return missionDataPed;
            }));

            EventSystem.GetModule().Attach("mission:update:ped:handcuffed", new EventCallback(metadata =>
            {
                MissionDataPed missionDataPed = GetMissionPed(metadata.Sender, metadata.Find<int>(0));

                if (missionDataPed == null) return null;

                missionDataPed.IsHandcuffed = metadata.Find<bool>(1);

                return missionDataPed;
            }));

            EventSystem.GetModule().Attach("mission:update:ped:suspect", new EventCallback(metadata =>
            {
                MissionDataPed missionDataPed = GetMissionPed(metadata.Sender, metadata.Find<int>(0));

                if (missionDataPed == null) return null;

                missionDataPed.IsSuspect = metadata.Find<bool>(1);

                return missionDataPed;
            }));

            EventSystem.GetModule().Attach("mission:update:ped:blip", new EventCallback(metadata =>
            {
                MissionDataPed missionDataPed = GetMissionPed(metadata.Sender, metadata.Find<int>(0));

                if (missionDataPed == null) return null;

                missionDataPed.AttachBlip = metadata.Find<bool>(1);

                return missionDataPed;
            }));

            EventSystem.GetModule().Attach("mission:update:ped:driver", new EventCallback(metadata =>
            {
                MissionDataPed missionDataPed = GetMissionPed(metadata.Sender, metadata.Find<int>(0));

                if (missionDataPed == null) return null;

                missionDataPed.IsDriver = metadata.Find<bool>(1);

                return missionDataPed;
            }));

            EventSystem.GetModule().Attach("mission:update:ped:breathlyser", new EventCallback(metadata =>
            {
                MissionDataPed missionDataPed = GetMissionPed(metadata.Sender, metadata.Find<int>(0));

                if (missionDataPed == null) return null;

                missionDataPed.HasBeenBreathalysed = true;

                return missionDataPed;
            }));

            EventSystem.GetModule().Attach("mission:update:ped:search", new EventCallback(metadata =>
            {
                MissionDataPed missionDataPed = GetMissionPed(metadata.Sender, metadata.Find<int>(0));

                if (missionDataPed == null) return null;

                if (missionDataPed.Items.Count > 0) return missionDataPed;

                Dictionary<string, bool> randomItems = new Dictionary<string, bool>();

                bool illegalItem = false;

                int randomCountOfItems = Utility.RANDOM.Next(1, 6);

                foreach (KeyValuePair<string, bool> kvp in Items)
                {
                    if (randomItems.Count == randomCountOfItems)
                        continue;

                    if (Utility.RANDOM.Bool(.5f))
                    {
                        bool itemLegality = kvp.Value;

                        if (kvp.Key == SEARCH_ITEM_GUN) // if they have a gun and a carry license, cannot flag the gun
                            itemLegality = !missionDataPed.HasCarryLicense;

                        randomItems.Add(kvp.Key, itemLegality);

                        if (!illegalItem)
                            illegalItem = itemLegality;
                    }
                }

                missionDataPed.IsCarryingIllegalItems = illegalItem;
                missionDataPed.Items = randomItems;
                missionDataPed.HasBeenSearched = true;

                return missionDataPed;
            }));

            EventSystem.GetModule().Attach("mission:update:ped:arrest", new EventCallback(metadata =>
            {
                MissionDataPed missionDataPed = GetMissionPed(metadata.Sender, metadata.Find<int>(0));

                if (missionDataPed == null) return null;

                int experienceEarned = 10;

                if (missionDataPed.StoleVehicle)
                {
                    experienceEarned += 100;
                }

                if (missionDataPed.HasBeenBreathalysed && missionDataPed.BloodAlcoholLimit >= 8)
                {
                    experienceEarned += 50;
                }

                if (missionDataPed.IsCarryingIllegalItems && missionDataPed.HasBeenSearched)
                {
                    experienceEarned += 50;
                }

                if (missionDataPed.IsWanted)
                {
                    missionDataPed.Wants.ForEach(s =>
                    {
                        experienceEarned += 25;
                    });
                }

                missionDataPed.IsArrested = true;

                bool res = Instance.ExportDictionary["curiosity-server"].TrafficStopArrest(metadata.Sender, experienceEarned);

                if (res)
                    _numberOfSuspectArrested++;

                return res;
            }));

            #endregion

            #endregion

            #region Mission Vehicle Events

            EventSystem.GetModule().Attach("mission:add:vehicle", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return null;

                if (missionData.OwnerHandleId != metadata.Sender) return null;

                int networkId = metadata.Find<int>(0);

                return missionData.AddNetworkVehicle(networkId);
            }));

            EventSystem.GetModule().Attach("mission:remove:vehicle", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return false;

                int networkId = metadata.Find<int>(0);

                return missionData.RemoveNetworkVehicle(networkId);
            }));

            #region Mission Vehicle Update

            EventSystem.GetModule().Attach("mission:update:vehicle:mission", new EventCallback(metadata =>
            {
                MissionDataVehicle missionDataVehicle = GetMissionVehicle(metadata.Sender, metadata.Find<int>(0));

                if (missionDataVehicle == null) return null;

                missionDataVehicle.AttachBlip = metadata.Find<bool>(1);

                return missionDataVehicle;
            }));

            EventSystem.GetModule().Attach("mission:update:vehicle:blip", new EventCallback(metadata =>
            {
                MissionDataVehicle missionDataVehicle = GetMissionVehicle(metadata.Sender, metadata.Find<int>(0));

                if (missionDataVehicle == null) return null;

                missionDataVehicle.AttachBlip = metadata.Find<bool>(1);

                return missionDataVehicle;
            }));

            EventSystem.GetModule().Attach("mission:update:vehicle:towable", new EventCallback(metadata =>
            {
                MissionDataVehicle missionDataVehicle = GetMissionVehicle(metadata.Sender, metadata.Find<int>(0));

                if (missionDataVehicle == null) return null;

                missionDataVehicle.IsTowable = metadata.Find<bool>(1);

                return missionDataVehicle;
            }));

            EventSystem.GetModule().Attach("mission:update:vehicle:license", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                MissionDataVehicle missionDataVehicle = GetMissionVehicle(metadata.Sender, metadata.Find<int>(0));

                if (missionDataVehicle == null) return null;

                missionDataVehicle.LicensePlate = metadata.Find<string>(1);
                missionDataVehicle.DisplayName = r.Replace(metadata.Find<string>(2).ToLowerInvariant(), " ");
                missionDataVehicle.PrimaryColor = r.Replace(metadata.Find<string>(3), " ");
                missionDataVehicle.SecondaryColor = r.Replace(metadata.Find<string>(4), " ");

                MissionDataPed mdp = missionData.NetworkPeds.Select(x => x.Value).Where(x => x.IsDriver).FirstOrDefault();

                if (mdp != null)
                    missionDataVehicle.OwnerName = mdp.FullName;

                if (Utility.RANDOM.Bool(0.2f))
                {
                    string firstname = "";
                    string surname = "";

                    if (0 == Utility.RANDOM.Next(2))
                    {
                        firstname = PedIdentifcationData.FirstNameMale[Utility.RANDOM.Next(PedIdentifcationData.FirstNameMale.Count)];
                    }
                    else
                    {
                        firstname = PedIdentifcationData.FirstNameFemale[Utility.RANDOM.Next(PedIdentifcationData.FirstNameFemale.Count)];
                    }

                    surname = PedIdentifcationData.Surname[Utility.RANDOM.Next(PedIdentifcationData.Surname.Count)];

                    missionDataVehicle.Stolen = true;
                    missionDataVehicle.OwnerName = $"{firstname} {surname}";

                    if (mdp != null)
                        mdp.StoleVehicle = true;
                }

                missionDataVehicle.InsuranceValid = Utility.RANDOM.Bool(0.90f);

                missionDataVehicle.RecordedLicensePlate = true;

                return missionDataVehicle;
            }));

            EventSystem.GetModule().Attach("mission:update:vehicle:search", new EventCallback(metadata =>
            {
                MissionDataVehicle missionDataVehicle = GetMissionVehicle(metadata.Sender, metadata.Find<int>(0));
                MissionDataPed missionDataPed = GetMissionData(metadata.Sender).NetworkPeds.Where(x => x.Value.IsDriver).Select(x => x.Value).FirstOrDefault();

                if (missionDataPed == null) return null;

                if (missionDataVehicle == null) return null;

                if (missionDataVehicle.Items.Count > 0) return missionDataVehicle;

                Dictionary<string, bool> randomItems = new Dictionary<string, bool>();

                bool illegalItem = false;

                int randomCountOfItems = Utility.RANDOM.Next(1, 6);

                foreach (KeyValuePair<string, bool> kvp in Items)
                {
                    if (randomItems.Count == randomCountOfItems)
                        continue;

                    if (Utility.RANDOM.Bool(.5f))
                    {
                        bool itemLegality = kvp.Value;

                        if (kvp.Key == SEARCH_ITEM_GUN) // if they have a gun and a carry license, cannot flag the gun
                            itemLegality = !missionDataPed.HasCarryLicense;

                        randomItems.Add(kvp.Key, itemLegality);

                        if (!illegalItem)
                            illegalItem = itemLegality;
                    }
                }

                missionDataVehicle.HasBeenSearched = true;
                missionDataPed.IsCarryingIllegalItems = illegalItem;
                missionDataVehicle.Items = randomItems;

                return missionDataVehicle;
            }));

            #endregion

            #endregion

            #region Mission Back Up Events

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

            #endregion
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

        MissionDataPed GetMissionPed(int sender, int networkId)
        {
            MissionData missionData = GetMissionData(sender);

            if (missionData == null) return null;

            if (!missionData.NetworkPeds.ContainsKey(networkId)) return null;

            MissionDataPed missionDataPed = missionData.NetworkPeds[networkId];

            if (missionDataPed == null) return null;

            return missionDataPed;
        }

        MissionDataVehicle GetMissionVehicle(int sender, int networkId)
        {
            MissionData missionData = GetMissionData(sender);

            if (missionData == null) return null;

            if (!missionData.NetworkVehicles.ContainsKey(networkId)) return null;

            MissionDataVehicle missionDataVehicle = missionData.NetworkVehicles[networkId];

            if (missionDataVehicle == null) return null;

            return missionDataVehicle;
        }
    }
}
