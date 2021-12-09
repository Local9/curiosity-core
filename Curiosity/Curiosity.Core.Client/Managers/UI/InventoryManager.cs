using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Curiosity.Systems.Library.Models.Shop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers.UI
{
    public class InventoryManager : Manager<InventoryManager>
    {
        bool isProcessing = false;
        DateTime processingDate;

        public override void Begin()
        {
            Instance.AttachNuiHandler("GetPlayerItems", new AsyncEventCallback(async metadata =>
            {
                List<CuriosityShopItem> lst = await EventSystem.Request<List<CuriosityShopItem>>("character:inventory:items");

                return JsonConvert.SerializeObject(lst);
            }));

            Instance.AttachNuiHandler("GetAllItems", new AsyncEventCallback(async metadata =>
            {
                List<CharacterItem> items = await EventSystem.Request<List<CharacterItem>>("character:inventory:items:all");
                return JsonConvert.SerializeObject(items);
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

                if (!int.TryParse(metadata.Find<string>(0), out itemId))
                {
                    Notify.Error($"Item was invalid, please submit a ticket if you can replicate it.");
                    return new { success = false };
                }

                int.TryParse(metadata.Find<string>(1), out numberOfItems);

                ExportMessage result = await EventSystem.Request<ExportMessage>("character:inventory:equip", itemId, numberOfItems);

                if (!result.success)
                {
                    Notify.Error($"{result.error}");
                    return $"{result}";
                }

                return $"{result}";
            }));

            Instance.AttachNuiHandler("UseItem", new AsyncEventCallback(async metadata =>
            {
                int itemId = -1;

                if (!int.TryParse(metadata.Find<string>(0), out itemId))
                {
                    Notify.Error($"Item was invalid, please submit a ticket if you can replicate it.");
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
                    Notify.Error($"Item was invalid, please submit a ticket if you can replicate it.");
                    return new { success = false };
                }

                int.TryParse(metadata.Find<string>(1), out numberOfItems);

                ExportMessage result = await EventSystem.Request<ExportMessage>("character:inventory:remove", itemId, numberOfItems);

                if (!result.success)
                {
                    Notify.Error($"{result.error}");
                    return $"{result}";
                }

                return $"{result}";
            }));
        }

        private async Task OnProcessingReset()
        {
            if (!isProcessing)
            {
                Instance.DetachTickHandler(OnProcessingReset);
            }

            if (DateTime.UtcNow > processingDate)
            {
                isProcessing = false;
                Instance.DetachTickHandler(OnProcessingReset);
                Notify.Info($"Processing has been force completed.");
            }
        }

        public async Task<ExportMessage> UseItem(int itemId, Vehicle veh = null)
        {
            ExportMessage result = new ExportMessage();

            if (isProcessing)
            {
                Notify.Error($"Currently processing request.");
                result.error = $"Currently processing request.";
                return result;
            }
            isProcessing = true;

            processingDate = DateTime.UtcNow.AddSeconds(15);
            Instance.AttachTickHandler(OnProcessingReset);

            int networkId = Cache.PlayerPed.NetworkId;

            if (veh != null)
            {
                networkId = veh.NetworkId;
            }

            result = await EventSystem.Request<ExportMessage>("character:inventory:use", itemId, networkId);

            if (!result.success)
            {
                Notify.Error($"{result.error}");
                return result;
            }

            if (result.item is not null)
            {
                if (result.item.CategoryId == 19)
                {
                    int playerHealth = Cache.PlayerPed.Health;
                    Cache.PlayerPed.Health = (playerHealth + result.item.HealingAmount);
                    Notify.Success($"Healed {result.item.HealingAmount}hp<br />Health: {Game.PlayerPed.Health}hp");
                    await EventSystem.Request<ExportMessage>("character:inventory:success", itemId);
                }

                if (result.item.CategoryId == 21)
                {
                    int playerArmor = Cache.PlayerPed.Armor;
                    Cache.PlayerPed.Armor = (playerArmor + result.item.HealingAmount);
                    Notify.Success($"Armor increased {result.item.HealingAmount}hp<br />Armor: {Game.PlayerPed.Armor}hp");
                    await EventSystem.Request<ExportMessage>("character:inventory:success", itemId);
                }

                if (result.item.CategoryId == 24)
                {
                    if (veh is null)
                    {
                        Notify.Success($"Vehicle Repair Unsuccessful");
                    }
                    else
                    {
                        veh.Repair();
                        Notify.Success($"Vehicle Repaired");
                        await EventSystem.Request<ExportMessage>("character:inventory:success", itemId);
                    }
                }
            }

            isProcessing = false;

            return result;
        }
    }
}
