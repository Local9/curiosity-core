using Curiosity.Global.Shared.Enums;
using System;
using System.Collections.Generic;

namespace Curiosity.Server.net.Entity
{
    public class CuriosityUser
    {
        public int Handle { get; set; }
        public long UserId { get; set; }
        public long LifeExperience { get; set; }
        public ulong DiscordId { get; set; }
        public List<ulong> DiscordRoles { get; set; }
        public string License { get; set; }
        public string LatestName { get; set; }
        public Privilege Role { get; set; } = Privilege.USER;
        public DateTime LatestActivity { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsBanned { get; set; }
        public bool IsBannedPerm { get; set; }
        public DateTime? BannedUntil { get; set; }
        // Queue
        public int QueuePriority { get; set; }

        public bool IsStaff => (Role == Privilege.COMMUNITYMANAGER || Role == Privilege.MODERATOR || Role == Privilege.ADMINISTRATOR || Role == Privilege.SENIORADMIN || Role == Privilege.DEVELOPER || Role == Privilege.PROJECTMANAGER);
        public bool IsAdmin => (Role == Privilege.COMMUNITYMANAGER || Role == Privilege.ADMINISTRATOR || Role == Privilege.SENIORADMIN || Role == Privilege.DEVELOPER || Role == Privilege.PROJECTMANAGER);
        public bool IsTrustedAdmin => (Role == Privilege.COMMUNITYMANAGER || Role == Privilege.DEVELOPER || Role == Privilege.PROJECTMANAGER);
        public bool IsDeveloper => (Role == Privilege.DEVELOPER || Role == Privilege.PROJECTMANAGER);
        public bool IsProjectManager => (Role == Privilege.PROJECTMANAGER);
        public bool IsDonator => (Role == Privilege.DONATOR3 || Role == Privilege.DONATOR2 || Role == Privilege.DONATOR1 || Role == Privilege.DONATOR);
        public bool IsLifeSupporter => (Role == Privilege.DONATOR);
        public bool IsDonatorLevel1 => (Role == Privilege.DONATOR1);
        public bool IsDonatorLevel2 => (Role == Privilege.DONATOR2);
        public bool IsDonatorLevel3 => (Role == Privilege.DONATOR3);
        public bool IsAllowedSupportXp => (IsStaff || IsDonator);
    }
}
