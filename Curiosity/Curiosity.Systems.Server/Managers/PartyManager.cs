using CitizenFX.Core;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Server.Managers
{
    public class PartyManager : Manager<PartyManager>
    {
        static Dictionary<Guid, Party> ActiveParties = new Dictionary<Guid, Party>();

        public override void Begin()
        {
            
        }

        void CreateParty(string leaderHandle)
        {
            if (!CuriosityPlugin.ActiveUsers.ContainsKey(leaderHandle)) return;
            // Create Party
            Party party = new Party(leaderHandle);
            // Add party to the player
            CuriosityPlugin.ActiveUsers[leaderHandle].PartyId = party.PartyId;
            // add party to the active list
            ActiveParties.Add(party.PartyId, party);
        }

        void DeleteParty(string leaderHandle)
        {
            if (!CuriosityPlugin.ActiveUsers.ContainsKey(leaderHandle)) return;
            // get user
            CuriosityUser curiosityUser = CuriosityPlugin.ActiveUsers[leaderHandle];
            // get party id
            Guid partyId = curiosityUser.PartyId;
            // Don't waste time
            if (partyId == Guid.Empty) return;
            // check party contains the key
            if (!ActiveParties.ContainsKey(partyId))
            {
                // if not then reset the user
                curiosityUser.PartyId = Guid.Empty;
                return;
            }
            // get the party
            Party party = ActiveParties[partyId];
            // update other users to remove their party
            foreach(string member in party.Members)
            {
                CuriosityPlugin.ActiveUsers[member].PartyId = Guid.Empty;
                // Send events

            }
            // remove from leader
            curiosityUser.PartyId = Guid.Empty;
            // remove party from the list
            ActiveParties.Remove(partyId);
        }

        void InviteToParty(string leaderHandle, string playerHandle)
        {
            if (!CuriosityPlugin.ActiveUsers.ContainsKey(leaderHandle)) return;

            if (!CuriosityPlugin.ActiveUsers.ContainsKey(playerHandle)) return;

            CuriosityUser leader = CuriosityPlugin.ActiveUsers[leaderHandle];
            CuriosityUser invitee = CuriosityPlugin.ActiveUsers[playerHandle];

            Player playerLeader = CuriosityPlugin.PlayersList[leader.Handle];
            Player playerInvitee = CuriosityPlugin.PlayersList[invitee.Handle];

            playerLeader.TriggerEvent(""); // X invited
            playerInvitee.TriggerEvent(""); // Confirm Invite
        }

        void JoinParty(string playerHandle, Guid partyId)
        {
            Party party = ActiveParties[partyId];
            party.AddMember(playerHandle);

            foreach(string member in party.Members)
            {
                // send joined events
                Player player = CuriosityPlugin.PlayersList[member];
                player.TriggerEvent("");
            }
        }

        void LeaveParty(string playerHandle, Guid partyId)
        {
            Party party = ActiveParties[partyId];
            party.RemoveMember(playerHandle);

            foreach (string member in party.Members)
            {
                // send removed events
                Player player = CuriosityPlugin.PlayersList[member];
                player.TriggerEvent("");
            }
        }

        /*
         * Create Party
         * Delete Party
         * 
         * JoinParty
         * InviteToParty
         * LeaveParty
         * KickFromParty
         * ChatMessage
         * ChangePartyLeader
         * 
         * */


    }
}
