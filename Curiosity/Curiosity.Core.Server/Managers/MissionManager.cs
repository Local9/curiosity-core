using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Server.Managers
{
    public class MissionManager : Manager<MissionManager>
    {
        const long TWO_MINUTES = (1000 * 60) * 2;

        public override void Begin()
        {
            #region Mission Back Up Events

            EventSystem.Attach("mission:assistance:request", new EventCallback(metadata =>
            {
                Player player = PluginManager.PlayersList[metadata.Sender];
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                if ((API.GetGameTimer() - curiosityUser.LastNotificationBackup) > TWO_MINUTES)
                {
                    curiosityUser.LastNotificationBackup = API.GetGameTimer();
                    curiosityUser.AssistanceRequested = true;

                    List<CuriosityUser> users = PluginManager.ActiveUsers.Where(x => x.Value.Job == ePlayerJobs.POLICE_OFFICER && !x.Value.DisableNotifications && x.Key != metadata.Sender).Select(y => y.Value).ToList();

                    users.ForEach(u =>
                    {
                        EventSystem.Send("ui:notification", u.Handle, eNotification.NOTIFICATION_WARNING, $"Dispatch A.I.<br />Back up request<br />Player '{player.Name}' has requested back up. You can find their location in the Police Menu.", "bottom-right", "snackbar", true);
                    });
                }
                else
                {
                    EventSystem.Send("ui:notification", metadata.Sender, eNotification.NOTIFICATION_WARNING, "Dispatch A.I.<br />Back up request<br />Sorry, you cannot request backup currently.", "bottom-right", "snackbar", true);
                    return false;
                }

                return true;
            }));

            EventSystem.Attach("mission:assistance:accept", new EventCallback(metadata =>
            {
                try
                {
                    if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;
                    CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];

                    Player playerToAssist = PluginManager.PlayersList[metadata.Find<int>(0)];
                    if (playerToAssist is null) return null;

                    // Player player = PluginManager.PlayersList[metadata.Sender];
                    // player.State.Set(StateBagKey.PLAYER_ASSISTING, true, true);

                    if (int.TryParse(playerToAssist.Handle, out int handle))
                        EventSystem.Send("ui:notification", handle, eNotification.NOTIFICATION_SUCCESS, $"Dispatch A.I.<br />Back up response<br />{curiosityUser.LatestName} responding.", "bottom-right", "snackbar", true);

                    Vector3 pos = playerToAssist.Character.Position;

                    RecordBackup(curiosityUser.Character.CharacterId);

                    return new { x = pos.X, y = pos.Y, z = pos.Z };
                }
                catch (Exception e)
                {
                    return false;
                }
            }));


            EventSystem.Attach("mission:assistance:codefour", new EventCallback(metadata =>
            {
                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender)) return false;
                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                curiosityUser.AssistanceRequested = false;
                return null;
            }));

            #endregion
        }

        async Task RecordBackup(int characterId)
        {
            await Database.Store.StatDatabase.Adjust(characterId, Stat.MISSION_BACKUP, 1);
        }
    }
}
