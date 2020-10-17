using Curiosity.Global.Shared.Enums;

namespace Curiosity.Global.Shared.Data
{
    static public class Permissions
    {
        public static bool IsStaff(Privilege privilege)
        {
            return (privilege == Privilege.COMMUNITYMANAGER || privilege == Privilege.MODERATOR || privilege == Privilege.ADMINISTRATOR || privilege == Privilege.SENIORADMIN || privilege == Privilege.HEADADMIN || privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        }

        public static bool IsAdmin(Privilege privilege)
        {
            return (privilege == Privilege.COMMUNITYMANAGER || privilege == Privilege.ADMINISTRATOR || privilege == Privilege.SENIORADMIN || privilege == Privilege.HEADADMIN || privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        }

        public static bool IsTrustedAdmin(Privilege privilege)
        {
            return (privilege == Privilege.COMMUNITYMANAGER || privilege == Privilege.HEADADMIN || privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        }

        public static bool IsDeveloper(Privilege privilege)
        {
            return (privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        }

        public static bool IsDeveloperOnly(Privilege privilege)
        {
            return (privilege == Privilege.DEVELOPER);
        }

        public static bool IsProjectManager(Privilege privilege)
        {
            return (privilege == Privilege.PROJECTMANAGER);
        }
    }
}
