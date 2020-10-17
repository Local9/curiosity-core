using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Global.Shared.Entity;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class NuiEventHandler
    {
        static Client client = Client.GetInstance();

        static bool IsPanelActive = false;

        public static void Init()
        {
            client.RegisterNuiEventHandler("ClosePanel", new Action<dynamic>(ClosePanel));
            // client.RegisterEventHandler("curiosity:Player:Skills:GetListData", new Action<string>(OpenDataList));

            client.RegisterTickHandler(OnNuiPanelTick);
        }

        static async Task OnNuiPanelTick()
        {
            if (IsPanelActive)
            {
                API.DisableAllControlActions(0);
                API.DisableAllControlActions(1);
                API.DisableAllControlActions(2);
            }

            if (ControlHelper.IsDisabledControlJustPressed(Control.FrontendCancel, false) && IsPanelActive)
            {
                ClosePanel(null);
            }
            await Task.FromResult(0);
        }

        static async void ClosePanel(dynamic obj)
        {
            await BaseScript.Delay(0);

            IsPanelActive = false;

            API.EnableAllControlActions(0);
            API.EnableAllControlActions(1);
            API.EnableAllControlActions(2);

            CinematicMode.DoHideHud = false;
            DisplayRadar(true);

            await BaseScript.Delay(0);
            SendNuiMessage(Newtonsoft.Json.JsonConvert.SerializeObject(new NuiData() { panel = "close" }));

            SetNuiFocus(false, false);
            SetTransitionTimecycleModifier("DEFAULT", 5.0f);
        }

        static void GetListData(SkillType skillType)
        {
            BaseScript.TriggerServerEvent("curiosity:Server:Skills:GetListData", (int)skillType);
        }

        static void OpenDataList(string listdata)
        {
            SendNuiMessage(listdata);
            SetScreenEffect();
        }

        static void SetScreenEffect()
        {
            SetNuiFocus(true, true);
            SetTransitionTimecycleModifier($"BLACKOUT", 5.0f);
            IsPanelActive = true;
            CinematicMode.DoHideHud = true;
            DisplayRadar(false);
        }
    }
}
