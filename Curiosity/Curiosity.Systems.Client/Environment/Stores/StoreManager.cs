using Curiosity.Systems.Client.Environment.Stores.Impl;
using Curiosity.Systems.Client.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Environment.Stores
{
    public class StoreManager : Manager<StoreManager>
    {
        public static Store[] Stores { get; set; } = { new Convenience() };

        public override void Begin()
        {
            foreach (var store in Stores)
            {
                store.Load();
                Curiosity.AttachTickHandlers(store);
            }
        }
    }
}
