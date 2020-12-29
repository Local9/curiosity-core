using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Client.net.Classes.PlayerClasses;
using Curiosity.Client.net.Helpers;
using Curiosity.Global.Shared;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Enums;
using Curiosity.Global.Shared.Utils;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.PDA
{
    class PdaEvents
    {
        static Client client = Client.GetInstance();

        static public void Init()
        {
            // PDA Basics
            // VehicleShop
            client.RegisterNuiEventHandler("GetVehicleShopItems", new Action<IDictionary<string, object>, CallbackDelegate>(OnNuiEventGetVehicleShopItems));
            client.RegisterNuiEventHandler("VehicleStoreAction", new Action<IDictionary<string, object>, CallbackDelegate>(OnNuiEventVehicleStoreAction));
            client.RegisterEventHandler("curiosity:Client:Vehicle:Shop:Items", new Action<string>(OnGotVehicleShopItems));
            client.RegisterEventHandler("curiosity:Client:Vehicle:Shop:Update", new Action(OnGotVehicleShopItemsUpdate));

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

        private static void OnDutyState(bool active, bool onduty, string job)
        {
            string jsn = new JsonBuilder().Add("operation", "DUTY")
                    .Add("isActive", active)
                    .Add("isDutyActive", onduty)
                    .Add("job", job.ToTitleCase())
                    .Build();

            API.SendNuiMessage(jsn);
        }
    }
}
