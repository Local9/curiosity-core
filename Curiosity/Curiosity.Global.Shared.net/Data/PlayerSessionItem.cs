using Curiosity.Global.Shared.Enums;

namespace Curiosity.Global.Shared.Data
{
    public class PlayerSessionItem
    {
        public string ServerId;
        public string Username;
        public long UserId;
        public string Job;
        public Privilege Privilege;
        public bool Disconnected;

        public bool IsDonator => (Privilege == Privilege.DONATOR || Privilege == Privilege.DONATOR1 || Privilege == Privilege.DONATOR2 || Privilege == Privilege.DONATOR3);
        public bool IsDeveloper => (Privilege == Privilege.DEVELOPER || Privilege == Privilege.PROJECTMANAGER);
        public bool IsManager => (Privilege == Privilege.DEVELOPER || Privilege == Privilege.PROJECTMANAGER || Privilege == Privilege.COMMUNITYMANAGER);
        public bool IsAdmin => (Privilege == Privilege.COMMUNITYMANAGER || Privilege == Privilege.ADMINISTRATOR || Privilege == Privilege.SENIORADMIN || Privilege == Privilege.HEADADMIN || Privilege == Privilege.DEVELOPER || Privilege == Privilege.PROJECTMANAGER);
        public bool IsStaff => (Privilege == Privilege.COMMUNITYMANAGER || Privilege == Privilege.MODERATOR || Privilege == Privilege.ADMINISTRATOR || Privilege == Privilege.SENIORADMIN || Privilege == Privilege.HEADADMIN || Privilege == Privilege.DEVELOPER || Privilege == Privilege.PROJECTMANAGER);
    }
}
