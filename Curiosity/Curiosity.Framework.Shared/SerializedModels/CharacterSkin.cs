namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class CharacterSkin
    {
        [JsonProperty("gender")]
        public int Gender { get; internal set; }

        [JsonProperty("model")]
        public uint Model { get; internal set; }

        [JsonProperty("face")]
        public Face Face { get; internal set; } = new();
    }
}
