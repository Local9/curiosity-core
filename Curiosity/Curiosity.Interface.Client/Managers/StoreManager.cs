using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Interface.Client.Managers
{
    public class StoreManager : Manager<StoreManager>
    {
        public override void Begin()
        {
            Instance.EventRegistry["curiosity:Client:Vehicle:Shop:Items"] += new Action<string>(OnVehicleShopItems);
            Instance.EventRegistry["curiosity:Client:Vehicle:Shop:Update"] += new Action(OnGetVehicleShopList);

            Instance.AttachNuiHandler("GetVehicleShopItems", new EventCallback(metadata =>
            {
                OnGetVehicleShopList();
                return null;
            }));

            Instance.AttachNuiHandler("VehicleStoreAction", new EventCallback(metadata =>
            {
                BaseScript.TriggerServerEvent("curiosity:Server:Vehicle:Shop:Action", metadata.Find<int>(0));
                return null;
            }));

            // New Shop Methods
            Instance.AttachNuiHandler("GetShopCategories", new AsyncEventCallback(async metadata =>
            {
                CuriosityStore json = await EventSystem.Request<CuriosityStore>("shop:get:categories");

                string jsn = new JsonBuilder()
                    .Add("operation", "SHOP_CATEGORIES")
                    .Add("list", json.Categories)
                    .Build();

                API.SendNuiMessage(jsn);

                return null;
            }));
        }

        private void OnGetVehicleShopList()
        {
            BaseScript.TriggerServerEvent("curiosity:Server:Vehicle:Shop:Get");
        }

        private void OnVehicleShopItems(string shopItems)
        {
            string decoded = Encode.Base64ToString(shopItems);
            List<VehicleShopItem> list = JsonConvert.DeserializeObject<List<VehicleShopItem>>(decoded);

            string jsn = new JsonBuilder()
                    .Add("operation", "VEHICLE_SHOP_ITEMS")
                    .Add("items", list)
                    .Build();

            API.SendNuiMessage(jsn);
        }
    }
}
