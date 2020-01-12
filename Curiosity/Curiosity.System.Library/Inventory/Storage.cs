using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Curiosity.System.Library.Inventory
{
    public class Storage
    {
        [Key] public string Seed { get; set; }
        public string Owner { get; set; }
        public StorageMetadata Metadata { get; set; }

        [Column("Metadata")]
        public string _Metadata
        {
            get => JsonConvert.SerializeObject(Metadata);
            set => Metadata = JsonConvert.DeserializeObject<StorageMetadata>(value);
        }
    }

    public class StorageMetadata
    {
        public List<InventoryItem> Items { get; set; }
    }
}