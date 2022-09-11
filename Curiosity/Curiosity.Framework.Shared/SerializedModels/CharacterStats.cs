using FxEvents.Shared.Attributes;

namespace Curiosity.Framework.Shared.SerializedModels
{
    [Serializable]
    public partial class CharacterStats
    {
        [Ignore]
        string MP0_FLYING_ABILITY = "MP0_FLYING_ABILITY";
        [Ignore]
        string MP0_LUNG_CAPACITY = "MP0_LUNG_CAPACITY";
        [Ignore]
        string MP0_SHOOTING_ABILITY = "MP0_SHOOTING_ABILITY";
        [Ignore]
        string MP0_STAMINA = "MP0_STAMINA";
        [Ignore]
        string MP0_STEALTH_ABILITY = "MP0_STEALTH_ABILITY";
        [Ignore]
        string MP0_STRENGTH = "MP0_STRENGTH";
        [Ignore]
        string MP0_WHEELIE_ABILITY = "MP0_WHEELIE_ABILITY";

        int _stamina;
        int _strength;
        int _lungCapacity;
        int _stealthAbility;
        int _shootingAbility;
        int _wheelieAbility;
        int _flyingAbility;

        public int STAMINA
        {
            get
            {
                return _stamina;
            }
            private set
            {
                _stamina = value;
                if (value > 100)
                    _stamina = 100;
                if (value < 0)
                    _stamina = 0;
                
                SetStatValue(MP0_STAMINA, _stamina);
            }
        }
        
        public int STRENGTH
        {
            get
            {
                return _strength;
            }
            private set
            {
                _strength = value;
                if (value > 100)
                    _strength = 100;
                if (value < 0)
                    _strength = 0;

                SetStatValue(MP0_STRENGTH, _strength);
            }
        }

        public int LUNG_CAPACITY
        {
            get
            {
                return _lungCapacity;
            }
            private set
            {
                _lungCapacity = value;
                if (value > 100)
                    _lungCapacity = 100;
                if (value < 0)
                    _lungCapacity = 0;

                SetStatValue(MP0_LUNG_CAPACITY, _lungCapacity);
            }
        }
        
        public int STEALTH_ABILITY
        {
            get
            {
                return _stealthAbility;
            }
            private set
            {
                _stealthAbility = value;
                if (value > 100)
                    _stealthAbility = 100;
                if (value < 0)
                    _stealthAbility = 0;

                SetStatValue(MP0_STEALTH_ABILITY, _stealthAbility);
            }
        }
        
        public int SHOOTING_ABILITY
        {
            get
            {
                return _shootingAbility;
            }
            private set
            {
                _shootingAbility = value;
                if (value > 100)
                    _shootingAbility = 100;
                if (value < 0)
                    _shootingAbility = 0;

                SetStatValue(MP0_SHOOTING_ABILITY, _shootingAbility);
            }
        }
        
        public int WHEELIE_ABILITY
        {
            get
            {
                return _wheelieAbility;
            }
            private set
            {
                _wheelieAbility = value;
                if (value > 100)
                    _wheelieAbility = 100;
                if (value < 0)
                    _wheelieAbility = 0;

                SetStatValue(MP0_WHEELIE_ABILITY, _wheelieAbility);
            }
        }
        
        public int FLYING_ABILITY
        {
            get
            {
                return _flyingAbility;
            }
            private set
            {
                _flyingAbility = value;
                if (value > 100)
                    _flyingAbility = 100;
                if (value < 0)
                    _flyingAbility = 0;

                SetStatValue(MP0_FLYING_ABILITY, _flyingAbility);
            }
        }

        public CharacterStats() { }

        public CharacterStats(int stamina, int strength, int lungCapacity, int stealthAbility, int shootingAbility, int wheelieAbility, int flyingAbility)
        {
            STAMINA = stamina;
            STRENGTH = strength;
            LUNG_CAPACITY = lungCapacity;
            STEALTH_ABILITY = stealthAbility;
            SHOOTING_ABILITY = shootingAbility;
            WHEELIE_ABILITY = wheelieAbility;
            FLYING_ABILITY = flyingAbility;
        }

#if SERVER
        [Ignore]
        void SetStatValue(string stat, int value) { }
#endif

#if CLIENT
        [Ignore]
        void SetStatValue(string stat, int value)
        {
            uint hash = (uint)API.GetHashKey(stat);
            StatSetInt(hash, value, true);
        }
#endif
    }
}
