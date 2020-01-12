using Curiosity.System.Library.Inventory;

namespace Curiosity.System.Client.Inventory.Items
{
    public class BandageItem : InventoryItem
    {
        public BandageItem() : base("bandage", "Bandage",
            "Bandage eller förband för att skydda eller stödja en skadad kroppsdel eller fixera den (till exempel gipsbandage). Syftet är att begränsa ytterligare skada och i viss mån underlätta läkning av skadan.",
            true)
        {
        }
    }
}