using Curiosity.Systems.Client.Environment.Stores.Impl;
using Curiosity.Systems.Client.Managers;

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
