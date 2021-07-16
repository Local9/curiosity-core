using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Entity;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
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

                MissionData missionData = ActiveMissions[metadata.Sender];
                missionData.IsCompleted = true;

                await BaseScript.Delay(5000);

                string missionId = missionData.ID;
                bool passed = metadata.Find<bool>(0);
                int numberTransportArrested = metadata.Find<int>(1);

                int numberOfFailures = 0;

                if (!passed)
                {
                    numberOfFailures = FailureTracker.AddOrUpdate(metadata.Sender, 1, (key, oldValue) => oldValue + 1);
                }
                else
                {
                    numberOfFailures = FailureTracker.AddOrUpdate(metadata.Sender, 0, (key, oldValue) => oldValue > 0 ? oldValue - 1 : 0);
                }

                Logger.Debug($"{player.Name} : NumFail: {numberOfFailures}");

                Mission res = await MissionCompleted(metadata.Sender, missionId, passed, numberTransportArrested, numberOfFailures);

                missionData.PartyMembers.ForEach(async serverHandle =>
                {
                    Player assistingPlayer = PluginManager.PlayersList[serverHandle];
                    assistingPlayer.State.Set(StateBagKey.PLAYER_ASSISTING, false, true);

                    await BaseScript.Delay(500);
                    if (numberOfFailures >= 3)
                    {
                        EventSystem.GetModule().Send("mission:notification", serverHandle, "No earnings", "The player you've assisted has failed too many times.");
                    }
                    else
                    {
                        await RecordBackup(serverHandle);
                        await BaseScript.Delay(10);
                        await MissionCompleted(serverHandle, missionId, passed, 1, 0);
                    }
                    EventSystem.GetModule().Send("mission:backup:completed", serverHandle);

                    if (!passed)
                    {
                        FailureTracker.AddOrUpdate(serverHandle, 1, (key, oldValue) => oldValue + 1);
                    }
                    else
                    {
                        FailureTracker.AddOrUpdate(serverHandle, 0, (key, oldValue) => oldValue > 0 ? oldValue - 1 : 0);
                    }
                });

                return res;
            }));

            #endregion

            #region Mission Ped Events

            EventSystem.GetModule().Attach("mission:add:ped", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);
                Player player = PluginManager.PlayersList[metadata.Sender];

                if (missionData == null) return null;

                if (missionData.OwnerHandleId != metadata.Sender) return null;

                int networkId = metadata.Find<int>(0);
                int gender = metadata.Find<int>(1);
                bool isDriver = metadata.Find<bool>(2);

                MissionDataPed missionDataPed = missionData.AddNetworkPed(networkId, gender, isDriver);

                int pedId = API.NetworkGetEntityFromNetworkId(networkId);
                Ped ped = new Ped(pedId);

                ped.State.Set(StateBagKey.PLAYER_OWNER, metadata.Sender, true);
                ped.State.Set(StateBagKey.PLAYER_NAME, player.Name, true);
                ped.State.Set(StateBagKey.PED_SPAWNED, true, true);
                ped.State.Set(StateBagKey.PED_FLEE, false, true);
                ped.State.Set(StateBagKey.PED_SHOOT, false, true);
                ped.State.Set(StateBagKey.PED_FRIENDLY, false, true);
                ped.State.Set(StateBagKey.PED_ARREST, false, true);
                ped.State.Set(StateBagKey.PED_ARRESTED, false, true);
                ped.State.Set(StateBagKey.PED_ARRESTABLE, false, true);
                ped.State.Set(StateBagKey.PED_SUSPECT, false, true);
                ped.State.Set(StateBagKey.PED_MISSION, true, true);
                ped.State.Set(StateBagKey.PED_IMPORTANT, false, true);
                ped.State.Set(StateBagKey.PED_HOSTAGE, false, true);
                ped.State.Set(StateBagKey.PED_RELEASED, false, true);
                ped.State.Set(StateBagKey.PED_HANDCUFFED, false, true);
                ped.State.Set(StateBagKey.PED_DIALOGUE, false, true);
                ped.State.Set(StateBagKey.PED_SETUP, false, true);
                ped.State.Set(StateBagKey.PED_IS_DRIVER, false, true);
                // menu options
                ped.State.Set(StateBagKey.MENU_RANDOM_RESPONSE, 0, true);
                ped.State.Set(StateBagKey.MENU_WELCOME, false, true);
                ped.State.Set(StateBagKey.MENU_IDENTIFICATION, false, true);
                ped.State.Set(StateBagKey.MENU_WHAT_YOU_DOING, false, true);
                ped.State.Set(StateBagKey.MENU_RAN_RED_LIGHT, false, true);
                ped.State.Set(StateBagKey.MENU_SPEEDING, false, true);
                ped.State.Set(StateBagKey.MENU_LANE_CHANGE, false, true);
                ped.State.Set(StateBagKey.MENU_TAILGATING, false, true);

                API.SetEntityDistanceCullingRadius(pedId, 15000f);
                API.SetEntityRoutingBucket(pedId, API.GetPlayerRoutingBucket(player.Handle));

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

                        if (!mpd.IsDriver)
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

            EventSystem.GetModule().Attach("mission:update:ped:arrest", new AsyncEventCallback(async metadata =>
            {
                MissionDataPed missionDataPed = GetMissionPed(metadata.Sender, metadata.Find<int>(0));

                if (missionDataPed == null) return null;

                bool successfulArrest = false;

                int experienceEarned = 10;

                if (missionDataPed.StoleVehicle)
                {
                    experienceEarned += 100;
                    successfulArrest = true;
                }

                if (missionDataPed.HasBeenBreathalysed && missionDataPed.BloodAlcoholLimit >= 8)
                {
                    experienceEarned += 50;
                    successfulArrest = true;
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
                    successfulArrest = true;
                }

                if (!successfulArrest)
                    experienceEarned = 10;

                missionDataPed.IsArrested = true;

                bool res = await RecordArrest(metadata.Sender, experienceEarned);

                if (res)
                    _numberOfSuspectArrested++;

                return res;
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

            #endregion

            #endregion

            #region Mission Vehicle Events

            EventSystem.GetModule().Attach("mission:add:vehicle", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);
                Player player = PluginManager.PlayersList[metadata.Sender];

                if (missionData == null) return null;

                if (missionData.OwnerHandleId != metadata.Sender) return null;

                int networkId = metadata.Find<int>(0);

                int vehicleId = API.NetworkGetEntityFromNetworkId(networkId);

                Vehicle veh = new Vehicle(vehicleId);
                veh.State.Set(StateBagKey.VEH_SPAWNED, true, true);
                veh.State.Set(StateBagKey.PLAYER_OWNER, metadata.Sender, true);
                veh.State.Set(StateBagKey.PLAYER_NAME, player.Name, true);
                veh.State.Set(StateBagKey.VEHICLE_MISSION, true, true);
                veh.State.Set(StateBagKey.VEHICLE_STOLEN, false, true);
                veh.State.Set(StateBagKey.VEHICLE_FLEE, false, true);
                veh.State.Set(StateBagKey.VEHICLE_SEARCH, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TOW, false, true);
                veh.State.Set(StateBagKey.VEHICLE_IMPORTANT, false, true);
                veh.State.Set(StateBagKey.VEHICLE_SETUP, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_HANDLE, 0, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_MARKED, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_PULLOVER, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_IGNORED, false, true);
                veh.State.Set(StateBagKey.VEHICLE_TRAFFIC_STOP_COMPLETED, false, true);

                API.SetEntityDistanceCullingRadius(vehicleId, 15000f);
                API.SetEntityRoutingBucket(vehicleId, API.GetPlayerRoutingBucket(player.Handle));

                return missionData.AddNetworkVehicle(networkId);
            }));

            EventSystem.GetModule().Attach("mission:remove:vehicle", new EventCallback(metadata =>
            {
                MissionData missionData = GetMissionData(metadata.Sender);

                if (missionData == null) return false;

                int networkId = metadata.Find<int>(0);

                return missionData.RemoveNetworkVehicle(networkId);
            }));

            EventSystem.GetModule().Attach("mission:update:vehicle:towed", new AsyncEventCallback(async metadata =>
            {
                MissionDataVehicle missionDataVehicle = GetMissionVehicle(metadata.Sender, metadata.Find<int>(0));

                if (missionDataVehicle == null) return null;

                int experienceEarned = 10;

                if (missionDataVehicle.Stolen)
                {
                    experienceEarned += 100;
                }

                if (missionDataVehicle.HasBeenSearched)
                {
                    foreach (KeyValuePair<string, bool> kvp in missionDataVehicle.Items)
                    {
                        if (kvp.Value)
                            experienceEarned += 25;
                    }
                }

                if (!missionDataVehicle.InsuranceValid)
                {
                    experienceEarned += 50;
                }

                bool res = await RecordVehicleTowed(metadata.Sender, experienceEarned);

                return res;
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

                MissionDataVehicle missionDataVehicle = missionData.NetworkVehicles[metadata.Find<int>(0)];

                if (missionDataVehicle == null) return null;

                missionDataVehicle.LicensePlate = metadata.Find<string>(1);
                missionDataVehicle.DisplayName = r.Replace(metadata.Find<string>(2).ToLowerInvariant(), " ");
                missionDataVehicle.PrimaryColor = r.Replace(metadata.Find<string>(3), " ");
                missionDataVehicle.SecondaryColor = r.Replace(metadata.Find<string>(4), " ");

                KeyValuePair<int, MissionDataPed> mdp = missionData.NetworkPeds.Where(x => x.Value.IsDriver).FirstOrDefault();

                if (mdp.Value != null)
                    missionDataVehicle.OwnerName = mdp.Value.FullName;

                if (Utility.RANDOM.Bool(0.15f))
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

                    if (mdp.Value != null)
                        mdp.Value.StoleVehicle = true;
                }

                if (mdp.Value != null)
                {
                    missionData.NetworkPeds[mdp.Key] = mdp.Value;
                }

                missionDataVehicle.InsuranceValid = Utility.RANDOM.Bool(0.90f);

                missionDataVehicle.RecordedLicensePlate = true;

                Logger.Debug($"{missionData}");

                return missionDataVehicle;
            }));

            EventSystem.GetModule().Attach("mission:update:vehicle:search", new EventCallback(metadata =>
            {
                MissionDataVehicle missionDataVehicle = GetMissionVehicle(metadata.Sender, metadata.Find<int>(0));
                
                MissionData missionData = GetMissionData(metadata.Sender);

                MissionDataPed missionDataPed = missionData.NetworkPeds.Where(x => x.Value.IsDriver).Select(x => x.Value).FirstOrDefault();

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
                    EventSystem.GetModule().Send("mission:notification", metadata.Sender, "Dispatch A.I.", "Back up request", $"Sorry, you cannot request backup currently.");
                }

                missionData.AssistanceRequested = true;

                return true;
            }));

            EventSystem.GetModule().Attach("mission:assistance:accept", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];
                player.State.Set(StateBagKey.PLAYER_ASSISTING, true, true);

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

            #region export
            //Instance.ExportDictionary.Add("JobEvent", new Func<int, string, bool>(
            //    (playerHandle, eventName) =>
            //    {
            //        CuriosityUser curiosityUser = PluginManager.ActiveUsers[playerHandle];

            //        EventSystem.GetModule().Send(eventName, playerHandle);

            //        return true;
            //    }
            //));
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

        async Task<bool> RecordVehicleTowed(int serverHandle, int xpEarned)
        {
            if (!PluginManager.ActiveUsers.ContainsKey(serverHandle)) return false;

            CuriosityUser user = PluginManager.ActiveUsers[serverHandle];
            int characterId = user.Character.CharacterId;

            await Database.Store.SkillDatabase.Adjust(characterId, (int)Skill.KNOWLEDGE, 2);
            await BaseScript.Delay(10);
            user.Character.Cash = await Database.Store.BankDatabase.Adjust(characterId, 100);
            await BaseScript.Delay(10);
            await Database.Store.SkillDatabase.Adjust(characterId, (int)Skill.POLICE, xpEarned);
            await BaseScript.Delay(10);
            await Database.Store.StatDatabase.Adjust(characterId, Stat.POLICE_REPUATATION, 5);
            await BaseScript.Delay(100);
            EventSystem.GetModule().Send("mission:notification:impound", serverHandle, "Los Santos Impound", "Vehicle Logged", $"~b~XP Gained~w~: {xpEarned:d0}~n~~b~Cash~w~: ${100:c0}");

            return true;
        }

        async Task<bool> RecordBackup(int serverHandle)
        {
            if (!PluginManager.ActiveUsers.ContainsKey(serverHandle)) return false;

            CuriosityUser user = PluginManager.ActiveUsers[serverHandle];
            int characterId = user.Character.CharacterId;

            await Database.Store.StatDatabase.Adjust(characterId, Stat.MISSION_BACKUP, 1);

            return true;
        }

        async Task<bool> RecordArrest(int serverHandle, int xpEarned)
        {
            if (!PluginManager.ActiveUsers.ContainsKey(serverHandle)) return false;

            CuriosityUser user = PluginManager.ActiveUsers[serverHandle];
            int characterId = user.Character.CharacterId;

            xpEarned = XpEarned(user, xpEarned);

            await Database.Store.SkillDatabase.Adjust(characterId, (int)Skill.KNOWLEDGE, 2);
            await BaseScript.Delay(10);
            user.Character.Cash = await Database.Store.BankDatabase.Adjust(characterId, 100);
            await BaseScript.Delay(10);
            await Database.Store.SkillDatabase.Adjust(characterId, (int)Skill.POLICE, xpEarned);
            await BaseScript.Delay(10);
            await Database.Store.StatDatabase.Adjust(characterId, Stat.POLICE_REPUATATION, 5);
            await BaseScript.Delay(10);
            await Database.Store.StatDatabase.Adjust(characterId, Stat.MISSION_ARRESTS, 1);
            await BaseScript.Delay(100);
            EventSystem.GetModule().Send("mission:notification", serverHandle, "Dispatch A.I.", "Arrest Booked", $"~b~XP Gained~w~: {xpEarned:d0}~n~~b~Cash~w~: ${100:c0}");

            return true;
        }

        async Task<Mission> MissionCompleted(int serverHandle, string missionId, bool passed, int numTransportArrested, int numberOfFailures)
        {
            if (!PluginManager.ActiveUsers.ContainsKey(serverHandle)) return null;
            CuriosityUser curUser = PluginManager.ActiveUsers[serverHandle];
            int characterId = curUser.Character.CharacterId;

            Mission mission = await Database.Store.MissionDatabase.Get(missionId);

            if (mission == null)
            {
                Logger.Error($"No mission returned from the database matching the ID `{missionId}`");
                return null;
            }

            bool usedTransport = numTransportArrested > 0;

            int xpReward = mission.XpReward;
            int repReward = mission.RepReward;
            int repFailure = mission.RepFailure;
            int cashMin = mission.CashMin;
            int cashMax = mission.CashMax;

            await Database.Store.StatDatabase.Adjust(characterId, Stat.MISSION_TAKEN, 1);

            if (passed)
            {
                if (usedTransport)
                {
                    xpReward = (int)(xpReward * .5f);
                    repReward = (int)(repReward * .5f);
                    cashMin = (int)(cashMin * .5f);
                    cashMax = (int)(cashMax * .5f);
                }

                if (numberOfFailures >= 3)
                {
                    xpReward = (int)(xpReward * .1f);
                    repReward = 0;
                    cashMin = (int)(cashMin * .1f);
                    cashMax = (int)(cashMax * .1f);
                }

                int money = Utility.RANDOM.Next(cashMin, cashMax);

                await BaseScript.Delay(100);

                await Database.Store.SkillDatabase.Adjust(characterId, (int)Skill.KNOWLEDGE, 2);
                await BaseScript.Delay(10);
                curUser.Character.Cash = await Database.Store.BankDatabase.Adjust(characterId, money);
                await BaseScript.Delay(10);
                await Database.Store.SkillDatabase.Adjust(characterId, (int)Skill.POLICE, xpReward);
                await BaseScript.Delay(10);
                await Database.Store.StatDatabase.Adjust(characterId, Stat.POLICE_REPUATATION, repReward);
                await BaseScript.Delay(10);
                await Database.Store.StatDatabase.Adjust(characterId, Stat.MISSION_COMPLETED, 1);

                mission.RepFailure = 0;

                xpReward = XpEarned(curUser, xpReward);

                if (numberOfFailures >= 3)
                {
                    EventSystem.GetModule().Send("mission:notification", serverHandle, "Dispatch A.I.", "Lowered Rewards", $"~b~XP Gained~w~: {xpReward:d0}~n~~b~Rep Gained~w~: {repReward:d0}~n~~b~Cash~w~: ${money:c0}");
                    await BaseScript.Delay(500);
                    EventSystem.GetModule().Send("mission:notification", serverHandle, "Dispatch A.I.", "Reason", $"~w~You require ~y~{numberOfFailures - 3:d0} ~w~or more successful callout(s) to earn full rewards.");
                }
                else
                {
                    EventSystem.GetModule().Send("mission:notification", serverHandle, "Dispatch A.I.", "Completed", $"~b~XP Gained~w~: {xpReward:d0}~n~~b~Rep Gained~w~: {repReward:d0}~n~~b~Cash~w~: ${money:c0}");
                }
            }
            else
            {
                mission.XpReward = 0;
                mission.RepReward = 0;
                mission.CashMax = 0;
                mission.CashMin = 0;

                await BaseScript.Delay(10);
                await Database.Store.SkillDatabase.Adjust(characterId, (int)Skill.KNOWLEDGE, -4);
                await BaseScript.Delay(10);
                await Database.Store.StatDatabase.Adjust(characterId, Stat.POLICE_REPUATATION, repFailure * -1);
                await BaseScript.Delay(10);
                await Database.Store.StatDatabase.Adjust(characterId, Stat.MISSION_FAILED, 1);

                // send failure notification
                EventSystem.GetModule().Send("mission:notification", serverHandle, "Dispatch A.I.", "Failed", $"~b~Rep Lost~w~: {repFailure:d0}");

                if (numberOfFailures >= 3)
                {
                    await BaseScript.Delay(500);
                    EventSystem.GetModule().Send("mission:notification", serverHandle, "Dispatch A.I.", "Lowered Rewards", $"~w~You have failed too many missions in a row and will now get lower rewards.");
                }
            }

            return mission;
        }

        int XpEarned(CuriosityUser user, int xpReward)
        {
            float experienceModifier = float.Parse(API.GetConvar("experience_modifier", $"1.0"));

            if (experienceModifier > 1.0f && (user.IsStaff || user.IsDonator))
            {
                experienceModifier += 0.1f;
            }

            if (user.IsStaff || user.IsDonator)
            {
                experienceModifier += CharacterManager.GetModule().ExperienceModifier(user.Role);
            }

            return (int)(xpReward * experienceModifier);
        }
    }
}
