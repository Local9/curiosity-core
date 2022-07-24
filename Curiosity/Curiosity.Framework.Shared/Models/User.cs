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

        [Ignore]
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
