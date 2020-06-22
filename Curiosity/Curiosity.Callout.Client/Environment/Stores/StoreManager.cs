// using Curiosity.Callout.Client.Environment.Stores.Impl;
using Curiosity.Callout.Client.Managers;

namespace Curiosity.Callout.Client.Environment.Stores
{
    public class StoreManager : Manager<StoreManager>
    {
        public static Store[] Stores { get; set; } = {  };

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
