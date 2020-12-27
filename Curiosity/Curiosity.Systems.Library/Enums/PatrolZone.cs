using Curiosity.Systems.Library.Attributes;

namespace Curiosity.Systems.Library.Enums
{
    public enum PatrolZone
    {
        [StringValue("Anywhere")]
        Anywhere,
        [StringValue("City")]
        City,
        [StringValue("County")]
        County,
        [StringValue("Ocean")]
        Ocean,
        [StringValue("Highway")]
        Highway,
        [StringValue("Rural")]
        Rural
    }
}
