using Atlas.Roleplay.Client.Package;
using Atlas.Roleplay.Library.Inventory;
using Atlas.Roleplay.Library.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Atlas.Roleplay.Client.Inventory.Impl
{
    public class ProximityInventory : InventoryContainer
    {
        [JsonIgnore]
        public List<InventoryItemDroppedAdapter> DroppedWorldItems { get; set; } =
            new List<InventoryItemDroppedAdapter>();

        public ProximityInventory(InventoryContainerBase inventoryBase) : base(inventoryBase)
        {
            Registration += Register;
        }

        public void Register()
        {
            var package = NetworkPackage.GetModule();
            var manager = InventoryManager.GetModule();

            manager.StateChanged += state =>
            {
                if (state != InventoryStateChange.Show) return;

                Rework();
            };

            Update += (item, type) =>
            {
                if (item == null) return;

                if (type == InventoryUpdateType.Add)
                {
                    var player = Cache.Player;

                    if (item.Seed != "__none")
                        DroppedWorldItems.Add(new InventoryItemDroppedAdapter(Clipboard.Process(item),
                            Clipboard.Process(player.Entity.Position)));
                }
                else if (type == InventoryUpdateType.Remove)
                {
                    DroppedWorldItems.Where(self => self.Item.Seed == item.Seed).ToList()
                        .ForEach(self => self.Destroy());
                    DroppedWorldItems.RemoveAll(self => self.Item.Seed == item.Seed);
                }

                var load = package.GetLoad<List<InventoryItemDroppedAdapter>>("World.Items.Dropped");

                load.Update(DroppedWorldItems);
                load.Commit();
            };

            package.Imports += (__package, index) =>
            {
                if (index != "World.Items.Dropped") return;

                var load = package.GetLoad<List<InventoryItemDroppedAdapter>>("World.Items.Dropped").Get() ??
                           new List<InventoryItemDroppedAdapter>();

                foreach (var item in load.Where(self =>
                    DroppedWorldItems.FirstOrDefault(adapter => self.Item.Seed == adapter.Item.Seed) == null))
                {
                    item.Activate();
                }

                foreach (var item in DroppedWorldItems.Where(self =>
                    load.FirstOrDefault(adapter => self.Item.Seed == adapter.Item.Seed) == null))
                {
                    item.Destroy();
                }

                DroppedWorldItems = load;
                Rework();
            };
        }

        private void Rework()
        {
            Slots = new InventoryItem[SlotAmount];

            foreach (var adapter in DroppedWorldItems)
            {
                if (adapter.IsAccessible) Slots[adapter.Item.Slot] = adapter.Item;
            }

            Commit();
        }
    }
}