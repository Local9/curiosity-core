using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Client.Diagnostics;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.FiveM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Managers
{
    public class PDAInterface : Manager<PDAInterface>
    {
        public class Panel
        {
            public bool Main;
        }

        private static bool IsCoreOpen = false;
        public static Party Party;

        public override void Begin()
        {
            Curiosity.AttachNuiHandler("ClosePanel", new EventCallback(metadata =>
            {
                IsCoreOpen = false;
                SendPanelMessage();
                return null;
            }));

            Curiosity.AttachNuiHandler("PlayerProfile", new EventCallback(metadata =>
            {
                string jsn = new JsonBuilder().Add("operation", "PLAYER_PROFILE")
                    .Add("profile", Cache.Player).Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Curiosity.AttachNuiHandler("GetPlayerList", new AsyncEventCallback(async metadata =>
            {
                FiveMPlayerList players = await EventSystem.Request<FiveMPlayerList>("server:playerList", null);

                try
                {
                    List<FiveMPlayer> playerList = players.Players.Select(p => p)
                    // .Where(z => z.ServerHandle != $"{Game.Player.ServerId}")
                    .ToList();

                    string jsn = new JsonBuilder().Add("operation", "PLAYER_LIST")
                        .Add("players", playerList).Build();

                    API.SendNuiMessage(jsn);
                }
                catch (Exception ex)
                {
                    Logger.Error($"{ex}");
                }

                return null;
            }));

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
        private async Task OnCoreControls()
        {
            if (Session.CreatingCharacter) return;
            if (!IsCoreOpen && Game.IsControlJustPressed(0, Control.FrontendSocialClubSecondary))
            {
                Cache.Player.Handle = Game.Player.ServerId;
                IsCoreOpen = !IsCoreOpen;
                SendPanelMessage();
            }

            if (IsCoreOpen && (Game.IsControlJustPressed(0, Control.FrontendCancel)
                // || Game.IsControlJustPressed(0, Control.PhoneCancel)
                || Game.IsControlJustPressed(0, Control.CursorCancel)))
            {
                IsCoreOpen = !IsCoreOpen;
                SendPanelMessage();
            }
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

        private void SendPanelMessage()
        {
            string jsn = new JsonBuilder().Add("operation", "PANEL")
            .Add("panel", "PDA")
            .Add("playerHandle", Cache.Player.Handle)
            .Add("main", IsCoreOpen)
            .Build();

            API.SendNuiMessage(jsn);
            API.SetNuiFocus(IsCoreOpen, IsCoreOpen);
        }
    }
}
