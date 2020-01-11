using System.Collections.Generic;
using Newtonsoft.Json;

namespace Atlas.Roleplay.Client.Interface
{
    public class MenuMetadata
    {
        public string Header { get; set; }
        [JsonProperty("Selected")] public int ItemIndex { get; set; }
        public List<MenuItem> Items { get; set; } = new List<MenuItem>();
    }
}