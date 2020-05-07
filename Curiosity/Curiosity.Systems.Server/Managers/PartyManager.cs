using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Discord;
using Curiosity.Systems.Server.Diagnostics;
using Curiosity.Systems.Server.Events;
using Curiosity.Systems.Server.Extensions;
using System;
using System.Collections.Generic;
namespace Curiosity.Systems.Server.Managers
{
    public class PartyManager : Manager<PartyManager>
    {
        public static Dictionary<Guid, Party> ActiveParties = new Dictionary<Guid, Party>();

        public override void Begin()
        {
            EventSystem.GetModule().Attach("party:create", new EventCallback(metadata =>
            {
                CuriosityUser curiosityUser = CuriosityPlugin.ActiveUsers[metadata.Sender];
                // Create Party
                Party party = new Party();
                party.LeaderHandle = metadata.Sender;

                PartyMember partyMember = new PartyMember();
                partyMember.Handle = party.LeaderHandle;
                partyMember.Name = curiosityUser.LastName;
                partyMember.Leader = true;

                party.Members.Add(partyMember);
                // Add party to the player
                curiosityUser.SetPartyId(party.PartyId);
                // add party to the active list
                ActiveParties.Add(party.PartyId, party);
                // Send back the party....
                return party;
            }));

            EventSystem.GetModule().Attach("party:invite", new EventCallback(metadata =>
            {
                int invitee = metadata.Find<int>(0);
                int sender = metadata.Sender;

                CuriosityUser curiosityUser = CuriosityPlugin.ActiveUsers[sender];
                CuriosityUser userToInvite = CuriosityPlugin.ActiveUsers[invitee];

                Logger.Debug($"Handle of Invitee: {userToInvite.LastName}, Sender: {curiosityUser.LastName}");

                if (userToInvite.Handle == 0)
                {
                    Logger.Debug($"Invitee not found.");
                    Logger.Debug($"Active User Count: {CuriosityPlugin.ActiveUsers.Count}");
                    return false;
                }

                Logger.Debug($"[PARTY][INVITE] PID: {curiosityUser.PartyId}, Inviter: {curiosityUser.LastName}, Invitee: [{userToInvite.Handle}] {userToInvite.LastName}");

                userToInvite.Send("party:invite:request", curiosityUser.PartyId, curiosityUser.LastName);

                return true;

                // bool accept = await EventSystem.Request<bool>("party:invite:request", userToInvite.Handle);

                //if (accept)
                //{
                //    Party party = ActiveParties[curiosityUser.PartyId];
                //    party.AddMember(userToInvite.Handle, userToInvite.LastName);
                //    return party;
                //}
                //else
                //{
                //    return null;
                //}

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
