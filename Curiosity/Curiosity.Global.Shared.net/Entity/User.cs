using Curiosity.Global.Shared.Enums;
using System;

namespace Curiosity.Global.Shared.Entity
{
    public class User
    {
        private int _stamina;
        private int _strength;
        private int _stealth;
        private int _flying;
        private int _driving;
        private int _lungCapacity;
        private int _mentalState;

        // USER
        public long UserId;
        public int LifeExperience;
        public DateTime DateCreated;
        public bool Banned;
        public bool BannedPerm;
        public string BannedUntil;
        public int QueuePriority;
        public int QueueLevel;
        // CHARACTER
        public long CharacterId;
        public int LocationId;
        public int RoleId = 1;
        public string Role;
        
        public Privilege privilege
        {
            get
            {
                return (Privilege)RoleId;
            }
        }

        public int StatId;
        public int BankId;
        public int Wallet = 0;
        public int BankAccount = 0;
        public float PosX;
        public float PosY;
        public float PosZ;
        public int ServerId;
        public PlayerCharacter Skin = new PlayerCharacter();

        public bool IsDonator => ((Privilege)RoleId == Privilege.DONATOR_LIFE || privilege == Privilege.DONATOR_LEVEL_1 || privilege == Privilege.DONATOR_LEVEL_2 || privilege == Privilege.DONATOR_LEVEL_3);
        public bool IsStaff => (privilege == Privilege.COMMUNITYMANAGER || privilege == Privilege.MODERATOR || privilege == Privilege.ADMINISTRATOR || privilege == Privilege.SENIORADMIN || privilege == Privilege.HEADADMIN || privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        public bool IsAdmin => (privilege == Privilege.COMMUNITYMANAGER || privilege == Privilege.ADMINISTRATOR || privilege == Privilege.SENIORADMIN || privilege == Privilege.HEADADMIN || privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        public bool IsTrustedAdmin => (privilege == Privilege.COMMUNITYMANAGER || privilege == Privilege.HEADADMIN || privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        public bool IsDeveloper => (privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);
        public bool IsProjectManager => (privilege == Privilege.PROJECTMANAGER);
        public bool IsSupporterAccess => (IsStaff || IsDonator);

        // CHARACTER STATS
        public int Stamina
        {
            get
            {
                return _stamina;
            }
            set
            {
                if (value > 100)
                {
                    _stamina = 100;
                }
                else
                {
                    _stamina = value;
                }
            }
        }

        public int Strength
        {
            get
            {
                return _strength;
            }
            set
            {
                if (value > 100)
                {
                    _strength = 100;
                }
                else
                {
                    _strength = value;
                }
            }
        }

        public int Stealth
        {
            get
            {
                return _stealth;
            }
            set
            {
                if (value > 100)
                {
                    _stealth = 100;
                }
                else
                {
                    _stealth = value;
                }
            }
        }

        public int Flying
        {
            get
            {
                return _flying;
            }
            set
            {
                if (value > 100)
                {
                    _flying = 100;
                }
                else
                {
                    _flying = value;
                }
            }
        }

        public int Driving
        {
            get
            {
                return _driving;
            }
            set
            {
                if (value > 100)
                {
                    _driving = 100;
                }
                else
                {
                    _driving = value;
                }
            }
        }

        public int LungCapacity
        {
            get
            {
                return _lungCapacity;
            }
            set
            {
                if (value > 100)
                {
                    _lungCapacity = 100;
                }
                else
                {
                    _lungCapacity = value;
                }
            }
        }

        public int MentalState
        {
            get
            {
                return _mentalState;
            }
            set
            {
                if (value > 100)
                {
                    _mentalState = 100;
                }
                else
                {
                    _mentalState = value;
                }
            }
        }
    }
}
