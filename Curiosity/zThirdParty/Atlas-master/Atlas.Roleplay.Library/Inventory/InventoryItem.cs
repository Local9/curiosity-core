using System.Collections.Generic;

namespace Atlas.Roleplay.Library.Inventory
{
    public class InventoryItem
    {
        public string Seed { get; set; }
        public string Name { get; set; }
        public string Icon => $"Icons/{Name.ToLower().Replace("weapon::", "")}.png";
        public string Label { get; set; }
        public string Description { get; set; }
        public int Slot { get; set; }
        public bool Usable { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        public InventoryItem()
        {
        }

        public InventoryItem(string name, string label, string description, bool usable)
        {
            Seed = Library.Seed.Generate();
            Name = name;
            Label = label;
            Description = description;
            Usable = usable;
        }

        public bool IsWeapon()
        {
            return Name.ToUpper().StartsWith("WEAPON::");
        }
    }
}