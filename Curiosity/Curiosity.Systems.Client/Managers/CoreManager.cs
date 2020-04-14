using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Client.Interface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Managers
{
    public class CoreManager : Manager<CoreManager>
    {
        public class Panel
        {
            public bool Main;
        }

        private static bool IsCoreOpen = false;

        public override void Begin()
        {

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
                || Game.IsControlJustPressed(0, Control.PhoneCancel)
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
        }
    }
}
