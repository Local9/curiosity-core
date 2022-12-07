﻿#if CLIENT
using Curiosity.Framework.Client.Utils;
#endif

using Curiosity.Framework.Shared.Enums;

#nullable enable

namespace Curiosity.Framework.Shared.SerializedModels
{
    public partial class User
    {
        public int Handle { get; internal set; }
        public int UserID { get; internal set; }
        public string? Username { get; internal set; }
        public eRole Role { get; internal set; }
        public List<Character> Characters { get; internal set; } = new();

#if CLIENT
        
        [JsonIgnore]
        public Character ActiveCharacter { get; internal set; }

#endif
    }
}
