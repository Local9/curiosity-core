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
        public Role UserRole { get; set; } = Role.USER;
        public DateTime LatestActivity { get; set; }
        public DateTime DateCreated { get; set; }
        public bool Banned { get; set; }
        public bool BannedPerm { get; set; }
        public DateTime? BannedUntil { get; set; }

        // Queue
        public int QueuePriority { get; set; }
        public int QueueLevel { get; set; }

        public CuriosityCharacter Character { get; set; }

        public bool IsStaff => (UserRole == Role.COMMUNITYMANAGER || UserRole == Role.MODERATOR || UserRole == Role.ADMINISTRATOR || UserRole == Role.SENIORADMIN || UserRole == Role.HEADADMIN || UserRole == Role.DEVELOPER || UserRole == Role.PROJECTMANAGER);
        public bool IsAdmin => (UserRole == Role.COMMUNITYMANAGER || UserRole == Role.ADMINISTRATOR || UserRole == Role.SENIORADMIN || UserRole == Role.HEADADMIN || UserRole == Role.DEVELOPER || UserRole == Role.PROJECTMANAGER);
        public bool IsTrustedAdmin => (UserRole == Role.COMMUNITYMANAGER || UserRole == Role.HEADADMIN || UserRole == Role.DEVELOPER || UserRole == Role.PROJECTMANAGER);
        public bool IsDeveloper => (UserRole == Role.DEVELOPER || UserRole == Role.PROJECTMANAGER);
        public bool IsProjectManager => (UserRole == Role.PROJECTMANAGER);
    }
}