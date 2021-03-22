using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models.Shop
{
    [DataContract]
    public class ShopCategory
    {
        [DataMember(Name = "shopCategoryId")]
        public int ShopCategoryID;
        
        [DataMember(Name = "shopCategoryDescription")]
        public string ShopCategoryDescription;

        [DataMember(Name = "categories")]
        public List<ShopCategoryItem> Categories = new List<ShopCategoryItem>();
    }
}
