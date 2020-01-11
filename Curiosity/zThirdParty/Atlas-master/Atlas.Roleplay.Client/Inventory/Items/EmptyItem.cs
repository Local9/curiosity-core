using Atlas.Roleplay.Library.Inventory;

namespace Atlas.Roleplay.Client.Inventory.Items
{
    public class EmptyItem : InventoryItem
    {
        public new string Icon => "";
        
        public EmptyItem()
        {
            Seed = "__none";
            Name = "none";
            Label = "";
            Description = "";
        }
    }
}