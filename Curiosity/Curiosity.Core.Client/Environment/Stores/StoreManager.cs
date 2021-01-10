using Curiosity.Core.Client.Environment.Stores.Impl;
using Curiosity.Core.Client.Managers;

namespace Curiosity.Core.Client.Environment.Stores
{
    public class StoreManager : Manager<StoreManager>
    {
        public static Store[] Stores { get; set; } = { new Convenience() };

        public override void Begin()
        {
            foreach (var store in Stores)
            {
                store.Load();
                Instance.AttachTickHandlers(store);
            }
        }
    }
}
