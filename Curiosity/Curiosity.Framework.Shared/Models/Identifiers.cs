using Lusive.Events.Attributes;

#nullable enable

namespace Curiosity.Framework.Shared.Models
{
    [Serialization]
    public partial class Identifiers
    {
        public string? Steam { get; set; }
        public string? License { get; set; }
        public string? Discord { get; set; }
        public string? Fivem { get; set; }
        public string? Ip { get; set; }

        public string?[] ToArray() => new string?[] { Steam, Discord, License, Fivem, Ip };
    }
}
