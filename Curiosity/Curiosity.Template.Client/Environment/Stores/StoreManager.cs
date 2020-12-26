using Curiosity.Template.Client.Environment.Stores.Impl;
using Curiosity.Template.Client.Managers;

namespace Curiosity.Template.Client.Environment.Stores
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
