using Lusive.Events.Attributes;

namespace Curiosity.Framework.Shared.Models
{
    [Serialization]
    public partial class Character
    {
        public bool IsActive { get; internal set; }
        public bool IsRegistered { get; internal set; }
        public int CharacterId { get; internal set; }
        public ulong Cash { get; internal set; }
        public string CharacterJson { get; internal set; }
    }
}
