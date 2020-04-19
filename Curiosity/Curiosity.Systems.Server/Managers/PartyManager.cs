using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Server.Events;
using System;
using System.Collections.Generic;
namespace Curiosity.Systems.Server.Managers
{
    public class PartyManager : Manager<PartyManager>
    {
        static Dictionary<Guid, Party> ActiveParties = new Dictionary<Guid, Party>();

        public override void Begin()
        {
            EventSystem.GetModule().Attach("party:create", new AsyncEventCallback(async metadata =>
            {
                CuriosityUser curiosityUser = CuriosityPlugin.ActiveUsers[metadata.Sender];
                // Create Party
                Party party = new Party();
                party.LeaderHandle = metadata.Sender;
                // Add party to the player
                curiosityUser.SetPartyId(party.PartyId);
                // add party to the active list
                ActiveParties.Add(party.PartyId, party);
                // Send back the party....
                return party;
            }));
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
