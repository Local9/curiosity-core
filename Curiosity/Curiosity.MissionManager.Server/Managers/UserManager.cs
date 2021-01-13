using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.MissionManager.Server.Diagnostics;
using Curiosity.MissionManager.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Shared.Entity;
using Newtonsoft.Json;
using System;

namespace Curiosity.MissionManager.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public override async void Begin()
        {
            EventSystem.GetModule().Attach("user:login", new AsyncEventCallback(async metadata =>
            {
                var player = PluginManager.PlayersList[metadata.Sender];

                string exportResponse = Instance.ExportDictionary["curiosity-core"].GetUser(player.Handle);

                CuriosityUser curiosityUser = JsonConvert.DeserializeObject<CuriosityUser>($"{exportResponse}");

                curiosityUser.Handle = metadata.Sender;

                PluginManager.ActiveUsers.TryAdd(metadata.Sender, curiosityUser);

                return curiosityUser;
            }));

            Instance.EventRegistry["playerDropped"] += new Action<Player, string>(OnPlayerDropped);

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
        }

        static void OnPlayerDropped([FromSource]Player player, string reason)
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
                    foreach(int partyMember in mission.PartyMembers)
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
