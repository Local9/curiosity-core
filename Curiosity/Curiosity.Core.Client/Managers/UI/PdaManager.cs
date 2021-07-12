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
            PhoneAnimation("text", false, false);
            currentWeapon = Game.PlayerPed.Weapons.Current.Hash;
            Game.PlayerPed.Weapons.Select(WeaponHash.Unarmed);
            API.SetTransitionTimecycleModifier($"BLACKOUT", 5.0f);
        }

        public void CloseTablet()
        {
            PhoneAnimation("out", false, false);
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
            API.NetworkRequestControlOfEntity(propNetworkId);

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
            if (TabletProp is null) return;

            if (TabletProp.Exists())
            {
                EventSystem.Send("delete:entity", TabletProp.NetworkId);
                TabletProp.Delete();
                TabletProp = null;
            }
        }

        string currentPhoneStatus = string.Empty;
        string lastDict = string.Empty;
        string lastAnim = string.Empty;
        const string PHONE_DICT_NORM = "cellphone@";
        const string PHONE_DICT_CAR = "anim@cellphone@in_car@ps";

        private async void PhoneAnimation(string status, bool freeze, bool force)
        {
            if (currentPhoneStatus == status && !force) return;
            string dict = PHONE_DICT_NORM;

            if (Game.PlayerPed.IsInVehicle())
                dict = PHONE_DICT_CAR;

            await LoadAnimationDict(dict);

            string animation = GetAnimation(dict, currentPhoneStatus, status);

            if (currentPhoneStatus == "out")
                Game.PlayerPed.Task.ClearAnimation(lastDict, lastAnim);

            int flag = freeze ? 14 : 50;

            Game.PlayerPed.Task.PlayAnimation(dict, animation, 3.0f, -1, (AnimationFlags)flag);

            if (status != "out" && currentPhoneStatus == "out")
            {
                await BaseScript.Delay(300);
                // phone prop.....
            }

            lastDict = dict;
            lastAnim = animation;
            currentPhoneStatus = status;

            if (status == "out")
            {
                await BaseScript.Delay(300);
                // delete the prop....
                Game.PlayerPed.Task.ClearAnimation(lastDict, lastAnim);
            }
        }

        async Task LoadAnimationDict(string dict)
        {
            API.RequestAnimDict(dict);
            while (!API.HasAnimDictLoaded(dict)) await BaseScript.Delay(1);
        }

        string GetAnimation(string dict, string currentStatus, string status)
        {
            if (dict == PHONE_DICT_NORM)
            {
                if (currentStatus == "out")
                {
                    if (status == "text") return "cellphone_text_in";
                    if (status == "call") return "cellphone_call_listen_base";
                }
                if (currentStatus == "text")
                {
                    if (status == "out") return "cellphone_text_out";
                    if (status == "text") return "cellphone_text_in";
                    if (status == "call") return "cellphone_text_to_call";
                }
                if (currentStatus == "call")
                {
                    if (status == "out") return "cellphone_call_out";
                    if (status == "text") return "cellphone_call_to_text";
                    if (status == "call") return "cellphone_text_to_call";
                }
            }

            if (dict == PHONE_DICT_CAR)
            {
                if (currentStatus == "out")
                {
                    if (status == "text") return "cellphone_text_in";
                    if (status == "call") return "cellphone_call_listen_base";
                }
                if (currentStatus == "text")
                {
                    if (status == "out") return "cellphone_text_out";
                    if (status == "text") return "cellphone_text_in";
                    if (status == "call") return "cellphone_text_to_call";
                }
                if (currentStatus == "call")
                {
                    if (status == "out") return "cellphone_horizontal_exit";
                    if (status == "text") return "cellphone_call_to_text";
                    if (status == "call") return "cellphone_text_to_call";
                }
            }

            return string.Empty;
        }
    }
}
