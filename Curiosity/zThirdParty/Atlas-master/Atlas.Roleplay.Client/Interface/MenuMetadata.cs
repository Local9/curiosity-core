using Newtonsoft.Json;
using System.Collections.Generic;

namespace Atlas.Roleplay.Client.Interface
{
    public class MenuMetadata
    {
        public string Header { get; set; }
        [JsonProperty("Selected")] public int ItemIndex { get; set; }
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }
}