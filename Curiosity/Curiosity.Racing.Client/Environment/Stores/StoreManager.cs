using Curiosity.Racing.Client.Environment.Stores.Impl;
using Curiosity.Racing.Client.Managers;

namespace Curiosity.Racing.Client.Environment.Stores
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
