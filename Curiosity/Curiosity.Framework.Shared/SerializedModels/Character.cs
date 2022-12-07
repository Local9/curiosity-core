using FxEvents;

namespace Curiosity.Framework.Shared.SerializedModels
{
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
        public async Task OnUpdateCharacterAsync()
        {
            
        }

        public async Task<bool> OnSaveCharacterAsync()
        {
            return await EventDispatcher.Get<bool>("character:save", this);
        }
#endif
    }
}
