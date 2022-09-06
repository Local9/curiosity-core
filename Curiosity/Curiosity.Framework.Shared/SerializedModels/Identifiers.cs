#nullable enable

using FxEvents.Shared.Attributes;

namespace Curiosity.Framework.Shared.SerializedModels
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
