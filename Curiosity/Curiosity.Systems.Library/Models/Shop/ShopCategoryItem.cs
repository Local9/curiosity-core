using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models.Shop
{
    [DataContract]
    public class ShopCategoryItem
    {
        [DataMember(Name = "shopCategoryItemId")]
        public int ShopCategoryItemID;

        [DataMember(Name = "shopCategoryItemDescription")]
        public string ShopCategoryItemDescription;
    }
}
