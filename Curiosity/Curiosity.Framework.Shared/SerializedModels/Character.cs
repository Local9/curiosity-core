using FxEvents.Shared.Attributes;

namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serialization]
    public partial class Character
    {
        [JsonProperty("isActive")]
        public bool IsActive { get; internal set; }

        [JsonProperty("isRegistered")]
        public bool IsRegistered { get; internal set; }

        [JsonProperty("characterId")]
        public int CharacterId { get; internal set; }

        [JsonProperty("skin")]
        public CharacterSkin Skin { get; internal set; }

        [JsonProperty("stats")]
        public CharacterStats Stats { get; internal set; }

#if CLIENT
        [Ignore]
        public async Task OnUpdateCharacterAsync()
        {
            
        }
#endif
    }
}
