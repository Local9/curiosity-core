using System;

namespace Curiosity.Global.Shared.net.Entity
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
        public int UserId;
        public int LifeExperience;
        public DateTime DateCreated;
        public bool Banned;
        public bool BannedPerm;
        public string BannedUntil;
        public int QueuePriority;
        public int QueueLevel;
        // CHARACTER
        public int CharacterId;
        public int LocationId;
        public int RoleId;
        public string Role;
        public int StatId;
        public int BankId;
        public int Wallet = 0;
        public int BankAccount = 0;
        public float PosX;
        public float PosY;
        public float PosZ;
        public int ServerId;
        public PlayerCharacter skin;
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
