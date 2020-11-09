using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
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
        public Role Role { get; set; } = Role.USER;
        public DateTime LatestActivity { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsBanned { get; set; }
        public bool IsBannedPerm { get; set; }
        public DateTime? BannedUntil { get; set; }
        public bool IsNitroBooster { get; set; }
        // Queue
        public int QueuePriority { get; set; }
        public CuriosityCharacter Character { get; set; }
        public Guid PartyId { get; internal set; } = Guid.Empty;
        public string CurrentJob { get; set; }

        public void SetPartyId(Guid partyId)
        {
            this.PartyId = partyId;
        }

        public bool IsStaff => (Role == Role.COMMUNITY_MANAGER || Role == Role.MODERATOR || Role == Role.ADMINISTRATOR || Role == Role.SENIOR_ADMIN || Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER);
        public bool IsAdmin => (Role == Role.COMMUNITY_MANAGER || Role == Role.ADMINISTRATOR || Role == Role.SENIOR_ADMIN || Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER);
        public bool IsTrustedAdmin => (Role == Role.COMMUNITY_MANAGER || Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER);
        public bool IsDeveloper => (Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER);
        public bool IsProjectManager => (Role == Role.PROJECT_MANAGER);
        public bool IsDonator => (Role == Role.DONATOR_LEVEL_1 || Role == Role.DONATOR_LEVEL_2 || Role == Role.DONATOR_LEVEL_3 || Role == Role.DONATOR_LIFE);
        public bool IsDonatorLevel1 => (Role == Role.DONATOR_LEVEL_1);
        public bool IsDonatorLevel2 => (Role == Role.DONATOR_LEVEL_2);
        public bool IsDonatorLevel3 => (Role == Role.DONATOR_LEVEL_3);
        public bool IsDonatorLife => (Role == Role.DONATOR_LIFE);
    }
}