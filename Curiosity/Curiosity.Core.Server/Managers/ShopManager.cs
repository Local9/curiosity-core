﻿using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Events;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
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

            EventSystem.GetModule().Attach("shop:get:items", new EventCallback(metadata =>
            {
                return Instance.ExportDictionary["curiosity-server"].GetShopCategoryItems(metadata.Sender, metadata.Find<int>(0));
            }));

            EventSystem.GetModule().Attach("shop:item:buy", new EventCallback(metadata =>
            {
                return Instance.ExportDictionary["curiosity-server"].ItemBuy(metadata.Sender, metadata.Find<int>(0));
            }));

            EventSystem.GetModule().Attach("shop:item:sell", new EventCallback(metadata =>
            {
                return Instance.ExportDictionary["curiosity-server"].ItemSell(metadata.Sender, metadata.Find<int>(0));
            }));
        }
    }
}
