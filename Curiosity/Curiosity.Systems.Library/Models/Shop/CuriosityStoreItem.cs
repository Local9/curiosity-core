namespace Curiosity.Systems.Library.Models.Shop
{
    public class CuriosityStoreItem
    {
        public int ItemId;
        public string Label;
        public string Description;
        public int BuyValue;
        public int BuyBackValue;
        public int NumberInStock;
        public bool ItemPurchased;
        public int NumberOwned;
        public bool IsStockControlled;
        public int MaximumAllowed;
    }
}
