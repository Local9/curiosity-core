using Lusive.Events.Attributes;

namespace Curiosity.Framework.Shared.Models
{
    [Serialization]
    public partial class Character
    {
        public int CharacterId { get; internal set; }
        public ulong Cash { get; internal set; }
        public string CharacterJson { get; internal set; }
    }
}
