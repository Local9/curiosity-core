using CitizenFX.Core;
using Curiosity.Systems.Client.Interface;
using System.Collections.Generic;

namespace Curiosity.Systems.Client.Managers
{
    public class PartyManager : Manager<PartyManager>
    {
        public static Dictionary<string, Player> Members = new Dictionary<string, Player>();

        public override void Begin()
        {
            // Party Members need to be stored on the server, this also needs to send all members to any new members
        }

        public static void AddMembers(string playerArgument)
        {
            Player player = CuriosityPlugin.Instance.PlayerList[playerArgument];
            Members.Add(playerArgument, player);
        }

        public static void InviteMember(string playerArgument)
        {
            Player player = CuriosityPlugin.Instance.PlayerList[playerArgument];

            if (player != null)
            {
                Chat.SendLocalMessage($"Invited '{player.Name}' to party");
            }
            else
            {
                Chat.SendLocalMessage($"Player not found.");
            }
        }
    }
}
