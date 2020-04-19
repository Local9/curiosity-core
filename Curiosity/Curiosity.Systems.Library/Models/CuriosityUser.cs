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
        public Guid PartyId { get; internal set; } = Guid.Empty;

        public void SetPartyId(Guid partyId)
        {
            this.PartyId = partyId;
        }

        public bool IsStaff => (Role == Role.COMMUNITYMANAGER || Role == Role.MODERATOR || Role == Role.ADMINISTRATOR || Role == Role.SENIOR_ADMIN || Role == Role.HEAD_ADMIN || Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER);
        public bool IsAdmin => (Role == Role.COMMUNITYMANAGER || Role == Role.ADMINISTRATOR || Role == Role.SENIOR_ADMIN || Role == Role.HEAD_ADMIN || Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER);
        public bool IsTrustedAdmin => (Role == Role.COMMUNITYMANAGER || Role == Role.HEAD_ADMIN || Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER);
        public bool IsDeveloper => (Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER);
        public bool IsProjectManager => (Role == Role.PROJECT_MANAGER);
        public bool IsDonator => (Role == Role.DONATOR_LEVEL_1 || Role == Role.DONATOR_LEVEL_2 || Role == Role.DONATOR_LEVEL_3);
        public bool IsDonatorLevel1 => (Role == Role.DONATOR_LEVEL_1);
        public bool IsDonatorLevel2 => (Role == Role.DONATOR_LEVEL_2);
        public bool IsDonatorLevel3 => (Role == Role.DONATOR_LEVEL_3);
    }
}