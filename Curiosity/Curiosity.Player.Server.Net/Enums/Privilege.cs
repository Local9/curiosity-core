using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Server.net.Enums
{
    public enum Privilege
    {
        undefined = 0,
        user = 1,
        serverOwner = 2,
        administrator = 3,
        moderator = 4
    }
}
