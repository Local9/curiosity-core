using CitizenFX.Core;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Managers
{
    public class ShopManager : Manager<ShopManager>
    {
        /// <summary>
        /// GetCategories
        /// GetCategoryItems
        /// BuyItem
        /// SellItem
        /// </summary>

        public override void Begin()
        {
            EventSystem.GetModule().Attach("shop:get:categories", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    List<Tuple<int, string>> categories = await Database.Store.ShopDatabase.GetCategories();

                    CuriosityStore curiosityStore = new CuriosityStore();
                    curiosityStore.Categories = categories;

                    return curiosityStore;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "shop:get:categories");
                    return null;
                }
            }));

            EventSystem.GetModule().Attach("shop:get:items", new AsyncEventCallback(async metadata =>
            {
                try
                {
                    string result = Instance.ExportDictionary["curiosity-server"].GetUser(metadata.Sender);
                    CuriosityUser curiosityUser = JsonConvert.DeserializeObject<CuriosityUser>($"{result}");

                    int categoryId = metadata.Find<int>(0);

                    return await Database.Store.ShopDatabase.GetCategoryItems(categoryId, curiosityUser.CharacterId);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "shop:get:items");
                    return null;
                }
            }));

            // REVIEW THESE METHODS WHEN MOVING THE CHARACTER SESSIONS TO THE CORE
            EventSystem.GetModule().Attach("shop:item:action", new AsyncEventCallback(async metadata =>
            {
                string result = Instance.ExportDictionary["curiosity-server"].GetUser(metadata.Sender);
                CuriosityUser curiosityUser = JsonConvert.DeserializeObject<CuriosityUser>($"{result}");

                bool action = metadata.Find<bool>(0);
                int itemId = metadata.Find<int>(1);
                // Number of items later when baskets are added

                Logger.Debug($"shop:item:action -> Action: {action} T=B/F=S, ItemId: {itemId}");

                Tuple<bool, int> sqlResult = await Database.Store.ShopDatabase.TradeItem(curiosityUser.CharacterId, itemId, 1, action);

                await BaseScript.Delay(100);

                if (sqlResult.Item1)
                {
                    Instance.ExportDictionary["curiosity-server"].AdjustWallet($"{metadata.Sender}", sqlResult.Item2, !action); // review
                }

                return sqlResult.Item1;
            }));
        }
    }
}
