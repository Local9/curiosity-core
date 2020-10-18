using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.Player;
using Curiosity.Client.net.Helpers;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.PDA
{
    class PdaEvents
    {
        static Client client = Client.GetInstance();

        static bool IsCoreOpen;
        private static Prop TabletProp;

        static public void Init()
        {
            client.RegisterNuiEventHandler("PlayerProfile", new Action<IDictionary<string, object>, CallbackDelegate>(OnPlayerProfile));
            client.RegisterNuiEventHandler("ClosePanel", new Action<IDictionary<string, object>, CallbackDelegate>(OnClosePda));



            client.RegisterTickHandler(OnPdaCoreControls);
        }

        private static void OnPlayerProfile(IDictionary<string, object> data, CallbackDelegate cb)
        {
            string jsn = new JsonBuilder().Add("operation", "PLAYER_PROFILE")
                    .Add("profile", PlayerInformation.playerInfo).Build();
            API.SendNuiMessage(jsn);

            cb(new { ok = true });
        }

        private static void OnClosePda(IDictionary<string, object> data, CallbackDelegate cb)
        {
            IsCoreOpen = false;
            SendPanelMessage();
            CloseTablet();

            cb(new { ok = true });
        }

        private static async Task OnPdaCoreControls()
        {
            if (!Client.isSessionActive) return;
            if (!IsCoreOpen && (ControlHelper.IsControlJustPressed(Control.SwitchVisor, true) || ControlHelper.IsControlJustPressed(Control.FrontendSocialClubSecondary, true)))
            {
                IsCoreOpen = !IsCoreOpen;
                SendPanelMessage();
                OpenTablet();
            }

            if (IsCoreOpen && (Game.IsControlJustPressed(0, Control.FrontendCancel)
                // || Game.IsControlJustPressed(0, Control.PhoneCancel)
                || Game.IsControlJustPressed(0, Control.CursorCancel)))
            {
                IsCoreOpen = !IsCoreOpen;
                SendPanelMessage();
                CloseTablet();
            }
        }

        private static void SendPanelMessage()
        {
            string jsn = new JsonBuilder().Add("operation", "PANEL")
            .Add("panel", "PDA")
            .Add("main", IsCoreOpen)
            .Build();

            API.SendNuiMessage(jsn);
            API.SetNuiFocus(IsCoreOpen, IsCoreOpen);
        }

        private static void OpenTablet()
        {
            AddProp();
            string animationDict = "amb@world_human_tourist_map@male@base";
            string animationBase = "base";

            Game.PlayerPed.Task.PlayAnimation(animationDict, animationBase, -8f, -1, AnimationFlags.Loop);
        }

        private static void CloseTablet()
        {
            RemoveProp();
            Game.PlayerPed.Task.ClearAll();
        }

        private static async void AddProp()
        {
            Model model = "prop_cs_tablet";

            Vector3 pos = Game.PlayerPed.Position;
            pos.Z += .2f;

            TabletProp = await World.CreateProp(model, pos, true, false);

            if (TabletProp == null) return;

            Vector3 offset = new Vector3(0.0f, -0.03f, 0.0f);
            Vector3 rotation = new Vector3(20.0f, -90.0f, 0.0f);

            TabletProp.AttachTo(Game.PlayerPed.Bones[Bone.PH_R_Hand], offset, rotation);

            model.MarkAsNoLongerNeeded();
        }

        private static void RemoveProp()
        {
            if (TabletProp == null) return;

            if (TabletProp.Exists())
            {
                TabletProp.Delete();
                TabletProp = null;
            }
        }
    }
}
