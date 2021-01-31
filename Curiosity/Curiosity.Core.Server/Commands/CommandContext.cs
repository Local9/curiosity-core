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

        public static List<Role> STAFF_ROLES = new List<Role>() { Role.ADMINISTRATOR, Role.COMMUNITY_MANAGER, Role.DEVELOPER, Role.HEAD_ADMIN, Role.HELPER, Role.MODERATOR, Role.PROJECT_MANAGER, Role.SENIOR_ADMIN };
    }
}