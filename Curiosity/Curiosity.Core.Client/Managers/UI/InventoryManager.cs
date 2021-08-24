using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers.UI
{
    public class InventoryManager : Manager<InventoryManager>
    {
        bool isProcessing = false;

        public override void Begin()
        {
            Instance.AttachNuiHandler("GetPlayerItems", new AsyncEventCallback(async metadata =>
            {
                List<CuriosityShopItem> lst = await EventSystem.Request<List<CuriosityShopItem>>("character:inventory:items");

                return JsonConvert.SerializeObject(lst);
            }));

            Instance.AttachNuiHandler("GetPlayerInventory", new AsyncEventCallback(async metadata =>
            {
                List<CuriosityShopItem> lst = await EventSystem.Request<List<CuriosityShopItem>>("character:inventory:equipped");

                return JsonConvert.SerializeObject(lst);
            }));

            Instance.AttachNuiHandler("AddToInventory", new AsyncEventCallback(async metadata =>
            {
                int itemId = -1;
                int numberOfItems = 1;

                if(!int.TryParse(metadata.Find<string>(0), out itemId))
                {
                    NotificationManager.GetModule().Error($"Item was invalid, please submit a ticket if you can replicate it.");
                    return new { success = false };
                }

                int.TryParse(metadata.Find<string>(1), out numberOfItems);

                ExportMessage result = await EventSystem.Request<ExportMessage>("character:inventory:equip", itemId, numberOfItems);

                if (!result.Success)
                {
                    NotificationManager.GetModule().Error($"{result.Error}");
                    return $"{result}";
                }

                return $"{result}";
            }));

            Instance.AttachNuiHandler("UseItem", new AsyncEventCallback(async metadata =>
            {
                NotificationManager notificationManager = NotificationManager.GetModule();

                int itemId = -1;

                if (!int.TryParse(metadata.Find<string>(0), out itemId))
                {
                    notificationManager.Error($"Item was invalid, please submit a ticket if you can replicate it.");
                    return new { success = false };
                }

                var result = await UseItem(itemId);

                return $"{result}";
            }));

            Instance.AttachNuiHandler("RemoveFromInventory", new AsyncEventCallback(async metadata =>
            {
                int itemId = -1;
                int numberOfItems = 1;

                if (!int.TryParse(metadata.Find<string>(0), out itemId))
                {
                    NotificationManager.GetModule().Error($"Item was invalid, please submit a ticket if you can replicate it.");
                    return new { success = false };
                }

                int.TryParse(metadata.Find<string>(1), out numberOfItems);

                ExportMessage result = await EventSystem.Request<ExportMessage>("character:inventory:remove", itemId, numberOfItems);

                if (!result.Success)
                {
                    NotificationManager.GetModule().Error($"{result.Error}");
                    return $"{result}";
                }

                return $"{result}";
            }));
        }

        public async Task<dynamic> UseItem(int itemId)
        {
            NotificationManager notificationManager = NotificationManager.GetModule();

            if (isProcessing)
            {
                notificationManager.Error($"Currently processing request.");
                return new { success = false };
            }

            isProcessing = true;

            ExportMessage result = await EventSystem.Request<ExportMessage>("character:inventory:use", itemId);

            if (!result.Success)
            {
                notificationManager.Error($"{result.Error}");
                return $"{result}";
            }

            if (result.Item is not null)
            {
                if (result.Item.CategoryId == 19)
                {
                    int playerHealth = Cache.PlayerPed.Health;
                    Cache.PlayerPed.Health = (playerHealth + result.Item.HealingAmount);
                    notificationManager.Success($"Healed {result.Item.HealingAmount}hp<br />Health: {Game.PlayerPed.Health}hp");
                }

                if (result.Item.CategoryId == 21)
                {
                    int playerArmor = Cache.PlayerPed.Armor;
                    Cache.PlayerPed.Armor = (playerArmor + result.Item.HealingAmount);
                    notificationManager.Success($"Armor increased {result.Item.HealingAmount}hp<br />Armor: {Game.PlayerPed.Armor}hp");
                }
            }

            isProcessing = false;

            return $"{result}";
        }
    }
}
