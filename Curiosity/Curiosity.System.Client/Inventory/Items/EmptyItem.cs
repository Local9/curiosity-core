using Curiosity.System.Library.Inventory;

namespace Curiosity.System.Client.Inventory.Items
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