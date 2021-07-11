using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.FiveM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class PdaManager : Manager<PdaManager>
    {
        private const string COMMAND_OPEN_PDA_LEGACY = "open_pda_legacy";
        private const string COMMAND_OPEN_PDA_HOME = "open_pda_new";
        public static PdaManager PdaInstance;
        WeaponHash currentWeapon;

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

                API.SetNuiFocusKeepInput(false);

                SendPanelMessage();
                CloseTablet();
                ClosedMenu();
                return null;
            }));

            Instance.AttachNuiHandler("GetPlayerList", new AsyncEventCallback(async metadata =>
            {
                FiveMPlayerList players = await EventSystem.Request<FiveMPlayerList>("server:playerList", null);

                try
                {
                    List<FiveMPlayer> playerList = players.Players.Select(p => p)
                    .Where(z => z.ServerHandle != $"{Game.Player.ServerId}")
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

            API.RegisterKeyMapping(COMMAND_OPEN_PDA_HOME, "Open Curiosity PDA", "keyboard", "HOME");
            API.RegisterCommand(COMMAND_OPEN_PDA_HOME, new Action(OpenPDA), false);
        }

        public async void OpenPDA()
        {
            await Session.Loading();

            if (!IsCoreOpen && Cache.Character.MarkedAsRegistered)
            {
                IsCoreOpen = !IsCoreOpen;

                SendPanelMessage();
                OpenTablet();
                FocusInputs();
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnPdaControls()
        {
            if (IsCoreOpen && (Game.IsControlJustPressed(0, Control.FrontendCancel)
                || Game.IsControlJustPressed(0, Control.CursorCancel)))
            {
                IsCoreOpen = !IsCoreOpen;
                SendPanelMessage();
                ClosedMenu();
                CloseTablet();
            }
        }

        private void FocusInputs()
        {
            API.SetNuiFocus(true, IsCoreOpen);
            API.SetNuiFocusKeepInput(!IsCoreOpen);
        }

        private void ClosedMenu()
        {
            API.SetNuiFocus(false, false);
            API.SetNuiFocusKeepInput(false);
        }

        public void SendPanelMessage()
        {
            string jsn = new JsonBuilder().Add("operation", "PANEL")
            .Add("show", IsCoreOpen)
            .Build();

            API.SendNuiMessage(jsn);
        }

        private void OpenTablet()
        {
            currentWeapon = Game.PlayerPed.Weapons.Current.Hash;
            Game.PlayerPed.Weapons.Select(WeaponHash.Unarmed);

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
            Game.PlayerPed.Weapons.Select(currentWeapon);
        }

        private async void AddProp()
        {
            if (Game.PlayerPed.IsDead) return;

            string modelString = "prop_cs_tablet";
            Model model = modelString;
            await model.Request(10000);

            Vector3 pos = Game.PlayerPed.Position;
            pos.Z += .2f;

            int propNetworkId = await EventSystem.Request<int>("entity:spawn:prop", (uint)model.Hash, pos.X, pos.Y, pos.Z, true, true, true);

            if (propNetworkId == 0) return;

            int entityId = API.NetworkGetEntityFromNetworkId(propNetworkId);

            if (entityId == 0) return;

            Prop TabletProp = new Prop(entityId);

            if (TabletProp == null) return;

            if (!TabletProp.Exists()) return;

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
