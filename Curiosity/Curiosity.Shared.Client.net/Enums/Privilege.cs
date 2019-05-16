using System;

namespace Curiosity.Shared.Client.net.Enums
{
    [Flags]
    public enum Privilege : int
    {
        UNDEFINED = 0,
        USER = 1,
        SERVEROWNER = 2,
        ADMINISTRATOR = 3,
        MODERATOR = 4,
        IsAdmin = SERVEROWNER | ADMINISTRATOR
    }
}
