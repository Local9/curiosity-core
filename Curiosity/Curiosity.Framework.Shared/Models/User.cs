#if CLIENT
using Curiosity.Framework.Client.Utils;
#endif

using Lusive.Events.Attributes;

#nullable enable

namespace Curiosity.Framework.Shared.Models
{
    [Serialization]
    public partial class User
    {
        public int Handle { get; set; }
        public int UserID { get; set; }
        public string? Username { get; set; }
        public List<Character> Characters { get; set; } = new List<Character>();

#if CLIENT

        [Ignore]
        [JsonIgnore]
        public CameraViewmodelQueue CameraQueue { get; set; } = new CameraViewmodelQueue();

        [Ignore]
        [JsonIgnore]
        public Character ActiveCharacter { get; set; }

#endif
    }
}
