using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace Curiosity.Interface.Client.net.Environment.UI
{
    class DutyMessage
    {
        public bool duty;
        public bool dutyActive;
        public string job;
    }

    class HudMessage
    {
        public bool hideHud;
    }

    class DutyIcons
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterTickHandler(HideHtmlHud);
            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(SetDutyIcon));
        }

        static void SetDutyIcon(bool onDuty, bool active, string job)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new DutyMessage { duty = onDuty, dutyActive = active, job = job }));
        }

        static async Task HideHtmlHud()
        {
            if (!Client.clientSpawned)
            {
                API.SendNuiMessage(JsonConvert.SerializeObject(new HudMessage { hideHud = true }));
            }
            else if (API.IsPauseMenuActive())
            {
                API.SendNuiMessage(JsonConvert.SerializeObject(new HudMessage { hideHud = true }));
            }
            else
            {
                API.SendNuiMessage(JsonConvert.SerializeObject(new HudMessage { hideHud = Client.hideHud }));
            }
            await Task.FromResult(0);
        }
    }
}
