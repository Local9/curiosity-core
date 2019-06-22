using CitizenFX.Core;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.Native;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    class NuiEventHandler
    {
        static Client client = Client.GetInstance();

        static bool panelIsActive = false;

        public static void Init()
        {
            client.RegisterNuiEventHandler("ClosePanel", new Action<dynamic>(ClosePanel));
            client.RegisterEventHandler("curiosity:Player:Skills:GetListData", new Action<string>(OpenDataList));
        }

        static void ClosePanel(dynamic obj)
        {
            SetNuiFocus(false, false);
            SetTransitionTimecycleModifier("DEFAULT", 5.0f);

            CinematicMode.DoHideHud = false;
            DisplayRadar(true);
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

            CinematicMode.DoHideHud = true;
            DisplayRadar(false);
        }
    }
}
