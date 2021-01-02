using Curiosity.Systems.Library.Events;
using Curiosity.Core.Server.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                return null;
            }));

            EventSystem.GetModule().Attach("shop:get:items", new AsyncEventCallback(async metadata =>
            {
                return null;
            }));

            EventSystem.GetModule().Attach("shop:item:buy", new AsyncEventCallback(async metadata =>
            {
                return null;
            }));

            EventSystem.GetModule().Attach("shop:item:sell", new AsyncEventCallback(async metadata =>
            {
                return null;
            }));
        }
    }
}
