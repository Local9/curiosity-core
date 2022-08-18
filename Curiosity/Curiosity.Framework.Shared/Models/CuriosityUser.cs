using Lusive.Events.Attributes;

#nullable enable

namespace Curiosity.Framework.Shared.Models
{
    [Serialization]
    public partial class CuriosityUser
    {
        public int Handle { get; set; }
        public int UserID { get; set; }
        public string? Username { get; set; }
        public IEnumerable<Character> Characters { get; set; } = Enumerable.Empty<Character>();

        [Ignore]
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
