using System;

namespace Curiosity.Systems.Library.Models
{
    public class CuriosityUser
    {
        public int Handle { get; set; }
        public long UserId { get; set; }
        public long LifeExperience { get; set; }
        public ulong DiscordId { get; set; }
        public string License { get; set; }
        public string LastName { get; set; }
        public Role Role { get; set; } = Role.USER;
        public DateTime LatestActivity { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Banned { get; set; }
        public bool BannedPerm { get; set; }
        public DateTime? BannedUntil { get; set; }
        // Queue
        public int QueuePriority { get; set; }
        public int QueueLevel { get; set; }
        public CuriosityCharacter Character { get; set; }
        public Party Party { get; set; }

        public bool IsStaff => (Role == Role.COMMUNITYMANAGER || Role == Role.MODERATOR || Role == Role.ADMINISTRATOR || Role == Role.SENIORADMIN || Role == Role.HEADADMIN || Role == Role.DEVELOPER || Role == Role.PROJECTMANAGER);
        public bool IsAdmin => (Role == Role.COMMUNITYMANAGER || Role == Role.ADMINISTRATOR || Role == Role.SENIORADMIN || Role == Role.HEADADMIN || Role == Role.DEVELOPER || Role == Role.PROJECTMANAGER);
        public bool IsTrustedAdmin => (Role == Role.COMMUNITYMANAGER || Role == Role.HEADADMIN || Role == Role.DEVELOPER || Role == Role.PROJECTMANAGER);
        public bool IsDeveloper => (Role == Role.DEVELOPER || Role == Role.PROJECTMANAGER);
        public bool IsProjectManager => (Role == Role.PROJECTMANAGER);
    }
}