namespace Atlas.Roleplay.Library.Inventory
{
    public class InventoryContainerBase
    {
        public string Seed { get; set; }
        public string Name { get; set; }
        public int SlotAmount { get; set; }
        public InventoryItem[] Slots { get; set; }
    }
}