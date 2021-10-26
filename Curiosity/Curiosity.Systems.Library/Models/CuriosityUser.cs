using Curiosity.Systems.Library.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class CuriosityUser
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public int Handle { get; set; }
        public long UserId { get; set; }
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
        [JsonIgnore] public int TimesKilledSelf { get; set; } = 0;
        public CuriosityCharacter Character { get; set; }
        public Guid PartyId { get; internal set; } = Guid.Empty;
        public string CurrentJob { get; set; } = string.Empty;

        public int TotalNumberOfPlayerKills { get; internal set; } = 0;

        public void IncreasePlayerKills()
        {
            TotalNumberOfPlayerKills++;
        }
        public void LowerPlayerKills()
        {
            TotalNumberOfPlayerKills--;

            if (TotalNumberOfPlayerKills == 0)
                TotalNumberOfPlayerKills = 0;
        }

        public void SetPartyId(Guid partyId)
        {
            this.PartyId = partyId;
        }

        public bool IsStaff => Role == Role.COMMUNITY_MANAGER || Role == Role.MODERATOR || Role == Role.ADMINISTRATOR || Role == Role.SENIOR_ADMIN || Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER;
        public bool IsAdmin => Role == Role.COMMUNITY_MANAGER || Role == Role.ADMINISTRATOR || Role == Role.SENIOR_ADMIN || Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER;
        public bool IsTrustedAdmin => Role == Role.COMMUNITY_MANAGER || Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER;
        public bool IsSeniorDeveloper => Role == Role.DEVELOPER;
        public bool IsDeveloper => Role == Role.DEVELOPER || Role == Role.PROJECT_MANAGER;
        public bool IsProjectManager => Role == Role.PROJECT_MANAGER;
        public bool IsDonator => Role == Role.DONATOR_LEVEL_1 || Role == Role.DONATOR_LEVEL_2 || Role == Role.DONATOR_LEVEL_3 || Role == Role.DONATOR_LIFE;
        public bool IsDonatorLevel1 => Role == Role.DONATOR_LEVEL_1;
        public bool IsDonatorLevel2 => Role == Role.DONATOR_LEVEL_2;
        public bool IsDonatorLevel3 => Role == Role.DONATOR_LEVEL_3;
        public bool IsDonatorLife => Role == Role.DONATOR_LIFE;
        public bool IsSupporterAccess => IsStaff || IsDonator;

        [JsonIgnore] public bool NotificationBackup { get; set; } = false;
        [JsonIgnore] public DateTime LastNotificationBackup { get; set; }
        [JsonIgnore] public int PersonalVehicle { get; set; }
        [JsonIgnore] public int PersonalTrailer { get; set; }
        [JsonIgnore] public int PersonalPlane { get; set; }
        [JsonIgnore] public int PersonalBoat { get; set; }
        [JsonIgnore] public int PersonalHelicopter { get; set; }
        [JsonIgnore] public int RoutingBucket { get; set; }
        [JsonIgnore] public bool AllowPublicStats { get; set; }
        [JsonIgnore] public bool Purchasing { get; set; }
        [JsonIgnore] public int StaffVehicle { get; set; }
        [JsonIgnore] public string JobCallSign { get; set; }
        public string DiscordAvatar { get; set; }
    }
}