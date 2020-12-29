using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Interface.Client.Diagnostics;
using Curiosity.Library.Client;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.FiveM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Interface.Client.Managers
{
    public class PdaManager : Manager<PdaManager>
    {
        public static PdaManager PdaInstance;

        public class Panel
        {
            public bool Main;
        }

        private Prop TabletProp;

        public bool IsCoreOpen = false;

        public override void Begin()
        {

            PdaInstance = this;

            Instance.AttachNuiHandler("ClosePanel", new EventCallback(metadata =>
            {
                IsCoreOpen = false;
                SendPanelMessage();
                CloseTablet();
                return null;
            }));

            Instance.AttachNuiHandler("PlayerProfile", new AsyncEventCallback(async metadata =>
            {
                string role = "USER";

                CuriosityUser curiosityUser = await EventSystem.Request<CuriosityUser>("user:getProfile");

                switch (curiosityUser.Role)
                {
                    case Role.DONATOR_LIFE:
                        role = "LifeV Early Supporter";
                        break;
                    case Role.DONATOR_LEVEL_1:
                        role = "LifeV Supporter I";
                        break;
                    case Role.DONATOR_LEVEL_2:
                        role = "LifeV Supporter II";
                        break;
                    case Role.DONATOR_LEVEL_3:
                        role = "LifeV Supporter III";
                        break;
                    default:
                        role = $"{curiosityUser.Role}".ToLowerInvariant();
                        break;
                }

                string jsn = new JsonBuilder().Add("operation", "PROFILE")
                        .Add("name", curiosityUser.LatestName)
                        .Add("userId", curiosityUser.UserId)
                        .Add("role", role)
                        .Add("wallet", curiosityUser.Wallet)
                        .Build();

                API.SendNuiMessage(jsn);

                return jsn;
            }));

            Instance.AttachNuiHandler("GetPlayerList", new AsyncEventCallback(async metadata =>
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
        }

        [TickHandler(SessionWait = true)]
        private async Task OnCoreControls()
        {
            if (!IsCoreOpen && (ControlUtility.IsControlJustPressed(Control.SwitchVisor, true) || ControlUtility.IsControlJustPressed(Control.FrontendSocialClubSecondary, true)))
            {
                IsCoreOpen = !IsCoreOpen;
                SendPanelMessage();
                OpenTablet();
            }

            if (IsCoreOpen && (Game.IsControlJustPressed(0, Control.FrontendCancel)
                || Game.IsControlJustPressed(0, Control.CursorCancel)))
            {
                IsCoreOpen = !IsCoreOpen;
                SendPanelMessage();
                CloseTablet();
            }
        }

        public void SendPanelMessage()
        {
            string jsn = new JsonBuilder().Add("operation", "PANEL")
            .Add("panel", "PDA")
            .Add("main", IsCoreOpen)
            .Build();

            API.SendNuiMessage(jsn);
            API.SetNuiFocus(IsCoreOpen, IsCoreOpen);
        }

        private void OpenTablet()
        {
            AddProp();
            string animationDict = "amb@world_human_tourist_map@male@base";
            string animationBase = "base";

            Game.PlayerPed.Task.PlayAnimation(animationDict, animationBase, -8f, -1, AnimationFlags.Loop);
            API.SetTransitionTimecycleModifier($"BLACKOUT", 5.0f);
        }

        public void CloseTablet()
        {
            RemoveProp();
            Game.PlayerPed.Task.ClearAll();
            API.SetTransitionTimecycleModifier("DEFAULT", 5.0f);
        }

        private async void AddProp()
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

        private void RemoveProp()
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
