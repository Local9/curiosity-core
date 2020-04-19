using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Client.Diagnostics;
using Curiosity.Systems.Client.Interface;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

            Curiosity.AttachNuiHandler("GetPlayerList", new EventCallback(metadata =>
            {
                string jsn = new JsonBuilder().Add("operation", "PLAYER_LIST")
                    .Add("party", Curiosity.PlayerList).Build();

                API.SendNuiMessage(jsn);

                return null;
            }));

            Curiosity.AttachNuiHandler("CreateParty", new AsyncEventCallback(async metadata =>
            {
                Party party = await EventSystem.Request<Party>("party:create", null);

                string jsn = new JsonBuilder().Add("operation", "PLAYER_PARTY")
                    .Add("party", party).Build();

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

        private void SendPanelMessage()
        {
            Panel p = new Panel();
            p.Main = IsCoreOpen;
            string json = JsonConvert.SerializeObject(p);
            API.SendNuiMessage(json);
            API.SetNuiFocus(IsCoreOpen, IsCoreOpen);
        }
    }
}
