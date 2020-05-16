using Atlas.Roleplay.Client.Environment.Stores.Impl;
using Atlas.Roleplay.Client.Managers;

namespace Atlas.Roleplay.Client.Environment.Stores
{
    public class StoreManager : Manager<StoreManager>
    {
        public static Store[] Stores { get; set; } = { new Barber(), new Clothing(), new Masks() };

        public override void Begin()
        {
            foreach (var store in Stores)
            {
                store.Load();

                Atlas.AttachTickHandlers(store);
            }
        }
    }
}