using CitizenFX.Core;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Global.Shared.net.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class NuiEventHandler
    {
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterNuiEventHandler("ClosePanel", new Action<dynamic>(ClosePanel));
            client.RegisterNuiEventHandler("GetDataList", new Action<string>(ClosePanel));

            client.RegisterEventHandler("curiosity:Player:Skills:GetListData", new Action<string>(OpenDataList));
        }

        static void ClosePanel(dynamic obj)
        {
            SetNuiFocus(false, false);
            SetTransitionTimecycleModifier("DEFAULT", 5.0f);
        }

        static void OpenDataList(string listdata)
        {
            SendNuiMessage(Newtonsoft.Json.JsonConvert.SerializeObject(listdata));
            SetScreenEffect();
        }

        static void SetScreenEffect()
        {
            SetNuiFocus(true, true);
            SetTransitionTimecycleModifier($"BLACKOUT", 5.0f);
        }
    }
}
