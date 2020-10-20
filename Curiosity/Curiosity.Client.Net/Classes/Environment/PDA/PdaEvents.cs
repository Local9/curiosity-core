using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.Player;
using Curiosity.Client.net.Helpers;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Enums;
using Curiosity.Global.Shared.Utils;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Helper;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.PDA
{
    class PdaEvents
    {
        static Client client = Client.GetInstance();

        static bool IsCoreOpen;
        private static Prop TabletProp;
        static bool _PlayerSpawned;

        static public void Init()
        {
            // PDA Basics
            client.RegisterNuiEventHandler("PlayerProfile", new Action<IDictionary<string, object>, CallbackDelegate>(OnPlayerProfile));
            client.RegisterNuiEventHandler("PlayerExperience", new Action<IDictionary<string, object>, CallbackDelegate>(OnPlayerExperience));
            client.RegisterNuiEventHandler("ClosePanel", new Action<IDictionary<string, object>, CallbackDelegate>(OnClosePda));

            // LEGACY
            client.RegisterEventHandler("curiosity:Client:Interface:Duty", new Action<bool, bool, string>(OnDutyState));

            // VehicleShop
            client.RegisterNuiEventHandler("GetVehicleShopItems", new Action<IDictionary<string, object>, CallbackDelegate>(OnNuiEventGetVehicleShopItems));
            client.RegisterNuiEventHandler("VehicleStoreAction", new Action<IDictionary<string, object>, CallbackDelegate>(OnNuiEventVehicleStoreAction));
            client.RegisterEventHandler("curiosity:Client:Vehicle:Shop:Items", new Action<string>(OnGotVehicleShopItems));
            client.RegisterEventHandler("curiosity:Client:Vehicle:Shop:Update", new Action(OnGotVehicleShopItemsUpdate));

            client.RegisterTickHandler(OnPdaCoreControls);

            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));

        }

        private static void OnGotVehicleShopItemsUpdate()
        {
            BaseScript.TriggerServerEvent("curiosity:Server:Vehicle:Shop:Get");
        }

        private static void OnNuiEventVehicleStoreAction(IDictionary<string, object> args, CallbackDelegate cb)
        {
            int vehicleStoreId = $"{args["0"]}".ToInt();
            BaseScript.TriggerServerEvent("curiosity:Server:Vehicle:Shop:Action", vehicleStoreId);

            cb(new { ok = true });
        }

        private static void OnPlayerSpawned()
        {
            _PlayerSpawned = true;
        }

        private static void OnNuiEventGetVehicleShopItems(IDictionary<string, object> args, CallbackDelegate cb)
        {
            BaseScript.TriggerServerEvent("curiosity:Server:Vehicle:Shop:Get");

            cb(new { ok = true });
        }

        private static void OnGotVehicleShopItems(string shopItems)
        {
            List<VehicleShopItem> list = JsonConvert.DeserializeObject<List<VehicleShopItem>>(Encode.Base64ToString(shopItems));
            
            string jsn = new JsonBuilder()
                .Add("operation", "VEHICLE_SHOP_ITEMS")
                .Add("items", list)
                .Build();

            API.SendNuiMessage(jsn);
        }

        private static void OnPlayerExperience(IDictionary<string, object> data, CallbackDelegate cb)
        {
            string jsn = new JsonBuilder().Add("operation", "EXPERIENCE")
                .Add("skills", PlayerInformation.playerInfo.Skills)
                .Build();

            API.SendNuiMessage(jsn);

            cb(new { ok = true });
        }

        private static void OnDutyState(bool active, bool onduty, string job)
        {
            string jsn = new JsonBuilder().Add("operation", "DUTY")
                    .Add("isActive", active)
                    .Add("isDutyActive", onduty)
                    .Add("job", job.ToTitleCase())
                    .Build();

            API.SendNuiMessage(jsn);
        }

        private static void OnPlayerProfile(IDictionary<string, object> data, CallbackDelegate cb)
        {
            string role = "USER";

            switch(PlayerInformation.privilege)
            {
                case Privilege.DONATOR:
                    role = "LifeV Early Supporter";
                    break;
                case Privilege.DONATOR1:
                    role = "LifeV Supporter I";
                    break;
                case Privilege.DONATOR2:
                    role = "LifeV Supporter II";
                    break;
                case Privilege.DONATOR3:
                    role = "LifeV Supporter III";
                    break;
                default:
                    role = PlayerInformation.playerInfo.Role;
                    break;
            }

            string jsn = new JsonBuilder().Add("operation", "PROFILE")
                    .Add("name", PlayerInformation.playerInfo.Name)
                    .Add("userId", PlayerInformation.playerInfo.UserId)
                    .Add("role", PlayerInformation.playerInfo.Role)
                    .Add("wallet", PlayerInformation.playerInfo.Wallet)
                    .Build();
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

            if (!_PlayerSpawned) return;

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
