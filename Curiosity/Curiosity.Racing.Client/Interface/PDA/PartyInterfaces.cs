using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Racing.Client.Diagnostics;
using Curiosity.Racing.Client.Managers;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.FiveM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Racing.Client.Interface.PDA
{
    public class PartyInterfaces : Manager<PartyInterfaces>
    {
        public static Party Party;

        public override void Begin()
        {
            Curiosity.AttachNuiHandler("CreateParty", new AsyncEventCallback(async metadata =>
            {
                Party = await EventSystem.Request<Party>("party:create", null);

                string jsn = new JsonBuilder().Add("operation", "PARTY_DETAILS")
                    .Add("party", Party).Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Curiosity.AttachNuiHandler("GetPartyDetails", new EventCallback(metadata =>
            {
                if (Party != null)
                {
                    string jsn = new JsonBuilder().Add("operation", "PARTY_DETAILS")
                    .Add("party", Party).Build();

                    API.SendNuiMessage(jsn);
                }

                return null;
            }));

            Curiosity.AttachNuiHandler("PartyInvite", new AsyncEventCallback(async metadata =>
            {
                int playerToInvite = metadata.Find<int>(0);

                bool inviteSent = await EventSystem.Request<bool>("party:invite", playerToInvite);

                string jsn;

                if (inviteSent)
                {
                    jsn = new JsonBuilder().Add("operation", "NOTIFICATION_SUCCESS")
                    .Add("title", $"Party Invite")
                    .Add("message", $"Invite Sent")
                    .Build();
                }
                else
                {
                    jsn = new JsonBuilder().Add("operation", "NOTIFICATION_ERROR")
                    .Add("title", $"Party: Error")
                    .Add("message", $"Player not found, invite not sent.")
                    .Build();
                }

                API.SendNuiMessage(jsn);

                return null;
            }));

            Curiosity.AttachNuiHandler("PartyAcceptInvite", new EventCallback(metadata =>
            {
                string guid = metadata.Find<string>(0);

                EventSystem.Send("party:invite:accept", guid);

                return null;
            }));

            Curiosity.AttachNuiHandler("PartyDeclineInvite", new EventCallback(metadata =>
            {
                string guid = metadata.Find<string>(0);
                EventSystem.Send("party:invite:decline", guid);

                return null;
            }));

            Curiosity.AttachNuiHandler("PartyKick", new AsyncEventCallback(async metadata =>
            {
                Logger.Debug($"{metadata}");

                return null;
            }));

            Curiosity.AttachNuiHandler("PartyPromote", new AsyncEventCallback(async metadata =>
            {
                Logger.Debug($"{metadata}");

                return null;
            }));

            EventSystem.Attach("party:invite:request", new AsyncEventCallback(async metadata =>
            {
                string jsn = new JsonBuilder().Add("operation", "NOTIFICATION_INFO")
                    .Add("title", $"Party Invite")
                    .Add("message", $"{metadata.Find<string>(1)} invites you to a party.")
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            EventSystem.Attach("party:details:join", new EventCallback(metadata =>
            {
                Party = metadata.Find<Party>(0);

                string jsn = new JsonBuilder().Add("operation", "PARTY_DETAILS")
                    .Add("notification", $"{Party.NewestMember} Joined")
                    .Add("party", Party).Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            EventSystem.Attach("party:invite:declined", new EventCallback(metadata =>
            {
                string jsn = new JsonBuilder().Add("operation", "NOTIFICATION_INFO")
                    .Add("title", $"Party Invite Declined")
                    .Add("message", $"{metadata.Find<string>(0)} declined your invite.")
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));
        }

        [TickHandler(SessionWait = true)]
        private async Task OnPartyUpdate()
        {
            if (Party != null)
            {
                foreach (PartyMember partyMember in Party.Members)
                {
                    int playerLocalHandle = API.GetPlayerFromServerId(partyMember.Handle);
                    int pedHandle = API.GetPlayerPed(playerLocalHandle);
                    partyMember.Health = API.GetEntityHealth(pedHandle);
                    partyMember.Armor = API.GetPedArmour(pedHandle);
                }

                string jsn = new JsonBuilder().Add("operation", "PARTY_DETAILS")
                    .Add("party", Party).Build();

                API.SendNuiMessage(jsn);

                long timer = API.GetGameTimer();

                while ((API.GetGameTimer() - timer) < 3000)
                {
                    await BaseScript.Delay(10);
                }
            }
        }
    }
}
