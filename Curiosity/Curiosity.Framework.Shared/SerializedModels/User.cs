#if CLIENT
using Curiosity.Framework.Client.Utils;
#endif

using Curiosity.Framework.Shared.Enums;
using FxEvents.Shared.Attributes;

#nullable enable

namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serialization]
    public partial class User
    {
        public int Handle { get; internal set; }
        public int UserID { get; internal set; }
        public string? Username { get; internal set; }
        public eRole Role { get; internal set; }
        public List<Character> Characters { get; internal set; } = new();

#if CLIENT
        
        [Ignore]
        [JsonIgnore]
        public Character ActiveCharacter { get; internal set; }

#endif
    }
}
