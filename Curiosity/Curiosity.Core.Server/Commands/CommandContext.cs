using Curiosity.Systems.Library.Enums;
using System.Collections.Generic;

namespace Curiosity.Core.Server.Commands
{
    public abstract class CommandContext
    {
        public abstract string[] Aliases { get; set; }
        public abstract string Title { get; set; }
        public abstract bool IsRestricted { get; set; }
        public abstract List<Role> RequiredRoles { get; set; }
    }
}