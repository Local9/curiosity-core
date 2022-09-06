using FxEvents.Shared.Attributes;

namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class CharacterSkin
    {
        [JsonProperty("gender")]
        public int Gender { get; internal set; }

        [Ignore]
        public bool IsMale => Gender == 0;

        [JsonProperty("model")]
        public uint Model { get; internal set; }

        [JsonProperty("face")]
        public Face Face { get; internal set; } = new();

        [JsonProperty("age")]
        public A2 Age = new A2();

        [JsonProperty("hair")]
        public Hair Hair = new Hair();

        [JsonProperty("ears")]
        public Ears Ears = new Ears();

        [JsonProperty("outfit")]
        public CharacterOutfit CharacterOutfit = new();

#if CLIENT

#endif
    }
}
