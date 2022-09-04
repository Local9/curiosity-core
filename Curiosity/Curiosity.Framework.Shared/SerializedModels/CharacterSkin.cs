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

        [JsonProperty("age")]
        public A2 Age;

        [JsonProperty("hair")]
        public Hair Hair;

        [JsonProperty("ears")]
        public Ears Ears;
    }
}
