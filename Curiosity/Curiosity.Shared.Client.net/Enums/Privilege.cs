using System;

namespace Curiosity.Shared.Client.net.Enums
{
    [Flags]
    public enum Privilege : int
    {
        UNDEFINED = 0,
        USER = 1,
        MODERATOR = 2,
        ADMINISTRATOR = 3,
        DEVELOPER = 4,
        PROJECTMANAGER = 5,
        IsAdmin = PROJECTMANAGER | ADMINISTRATOR | DEVELOPER,
        IsDeveloper = PROJECTMANAGER | DEVELOPER
    }
}
