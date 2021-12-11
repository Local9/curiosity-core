using CitizenFX.Core;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class PlayerGroupManager : Manager<PlayerGroupManager>
    {
        // Groups do not have a leader

        Dictionary<int, List<int>> _activeGroups = new Dictionary<int, List<int>>();

        public override void Begin()
        {
            EventSystem.Attach("group:create", new EventCallback(metadata =>
            {
                ExportMessage exportMessage = new ExportMessage();

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                {
                    exportMessage.error = "Player not found. Cannot create group.";
                    goto RETURN_MESSAGE;
                }

                Player player = PluginManager.PlayersList[metadata.Sender];

                if (player is null)
                {
                    exportMessage.error = "Player not found. Cannot create group.";
                    goto RETURN_MESSAGE;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                int currentGroup = curiosityUser.CurrentGroup;

                if (currentGroup == -1)
                {
                    curiosityUser.CurrentGroup = CreateGroup(metadata.Sender);
                    player.State.Set(StateBagKey.PLAYER_GROUP, curiosityUser.CurrentGroup, true);
                    goto RETURN_MESSAGE;
                }
                else
                {
                    // check if it exists, if not then create
                    if (!_activeGroups.ContainsKey(currentGroup))
                    {
                        curiosityUser.CurrentGroup = CreateGroup(metadata.Sender);
                        player.State.Set(StateBagKey.PLAYER_GROUP, curiosityUser.CurrentGroup, true);
                        goto RETURN_MESSAGE;
                    }

                    if (IsUserInGroup(metadata.Sender, out curiosityUser.CurrentGroup))
                    {
                        player.State.Set(StateBagKey.PLAYER_GROUP, curiosityUser.CurrentGroup, true);
                        goto RETURN_MESSAGE;
                    }
                }

            RETURN_MESSAGE:
                return exportMessage;
            }));

            EventSystem.Attach("group:leave", new EventCallback(metadata =>
            {
                ExportMessage exportMessage = new ExportMessage();

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                {
                    exportMessage.error = "Player not found. Cannot create group.";
                    goto RETURN_MESSAGE;
                }

                CuriosityUser curiosityUser = PluginManager.ActiveUsers[metadata.Sender];
                Player player = PluginManager.PlayersList[metadata.Sender];

                if (!RemoveUserFromGroup(metadata.Sender, curiosityUser.CurrentGroup))
                {
                    exportMessage.error = $"Failed to remove user from group.";
                    goto RETURN_MESSAGE;
                }

                curiosityUser.CurrentGroup = -1;
                player.State.Set(StateBagKey.PLAYER_GROUP, curiosityUser.CurrentGroup, true);

            RETURN_MESSAGE:
                return exportMessage;
            }));

            EventSystem.Attach("group:invite", new EventCallback(metadata =>
            {
                return false;
            }));

            EventSystem.Attach("group:invite:accept", new EventCallback(metadata =>
            {
                ExportMessage exportMessage = new ExportMessage();

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                {
                    exportMessage.error = "Player not found. Cannot create group.";
                    goto RETURN_MESSAGE;
                }

            RETURN_MESSAGE:
                return exportMessage;
            }));

            EventSystem.Attach("group:invite:decline", new EventCallback(metadata =>
            {
                ExportMessage exportMessage = new ExportMessage();

                if (!PluginManager.ActiveUsers.ContainsKey(metadata.Sender))
                {
                    exportMessage.error = "Player not found. Cannot create group.";
                    goto RETURN_MESSAGE;
                }

            RETURN_MESSAGE:
                return exportMessage;
            }));
        }

        int CreateGroup(int serverIdToAdd)
        {
            int maxGroupId = _activeGroups.Count + 1;
            _activeGroups.Add(maxGroupId, new List<int> { serverIdToAdd });
            return maxGroupId;
        }

        bool IsUserInGroup(int serverId, out int groupId)
        {
            foreach(var group in _activeGroups)
            {
                foreach(var member in group.Value)
                {
                    if (member == serverId)
                    {
                        groupId = group.Key;
                        return true;
                    }
                }
            }

            groupId = -1;
            return false;
        }

        bool RemoveUserFromGroup(int serverId, int groupId)
        {
            Dictionary<int, List<int>> activeGroups = _activeGroups;

            if (activeGroups.ContainsKey(groupId)) return false;

            if (!activeGroups[groupId].Contains(serverId)) return false;

            activeGroups[groupId].Remove(serverId);
            _activeGroups[groupId] = activeGroups[groupId];

            if (activeGroups[groupId].Count == 0)
                _activeGroups.Remove(groupId);

            return true;
        }
    }
}
