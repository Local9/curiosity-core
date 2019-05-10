using System;

namespace Curiosity.Client.net.Enums
{
    [Flags]
    public enum BlipCategory
    {
        Unknown = 0,
        Shop = 1 << 0,
        Vehicle = 1 << 1,
        People = 1 << 2
    }
}
