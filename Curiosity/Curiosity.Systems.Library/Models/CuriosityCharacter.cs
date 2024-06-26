using Newtonsoft.Json;
using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class CuriosityCharacter
    {
        [JsonIgnore] public string MP0_STAMINA { get { return "MP0_STAMINA"; } }
        [JsonIgnore] public string MP0_SHOOTING_ABILITY { get { return "MP0_SHOOTING_ABILITY"; } }
        [JsonIgnore] public string MP0_STRENGTH { get { return "MP0_STRENGTH"; } }
        [JsonIgnore] public string MP0_STEALTH_ABILITY { get { return "MP0_STEALTH_ABILITY"; } }
        [JsonIgnore] public string MP0_FLYING_ABILITY { get { return "MP0_FLYING_ABILITY"; } }
        [JsonIgnore] public string MP0_WHEELIE_ABILITY { get { return "MP0_WHEELIE_ABILITY"; } }
        [JsonIgnore] public string MP0_LUNG_CAPACITY { get { return "MP0_LUNG_CAPACITY"; } }

        public string Version = "2";
        public int CharacterId { get; set; }
        public int Health { get; set; }
        public int Armor { get; set; }
        public bool IsDead { get; set; }
        public bool IsPassive { get; set; }
        public bool IsWanted { get; set; } = false;
        public ulong Cash { get; set; }
        public bool MarkedAsRegistered { get; set; }
        public int Gender { get; set; } = 0; // Default Gender is Male
        public Position LastPosition { get; set; } = new Position(405.9247f, -997.2114f, -99.00401f, 86.36787f);
        public CharacterHeritage Heritage { get; set; } = new CharacterHeritage();
        public CharacterAppearance Appearance { get; set; } = new CharacterAppearance();
        public CharacterInfo CharacterInfo { get; set; } = new CharacterInfo();
        public CharacterTattoos Tattoos { get; set; } = new CharacterTattoos();
        public Dictionary<int, float> Features { get; set; } = new Dictionary<int, float>();
        public bool AllowHelmet { get; set; }
        public bool IsOnIsland { get; set; }

        public bool IsMale => Gender == 0;

        public void ChangeFeature(int feature, float value)
        {
            if (Features.ContainsKey(feature))
            {
                Features[feature] = value;
            }
            else
            {
                Features.Add(feature, value);
            }
        }

        public int RespawnCharge()
        {
            int costOfRespawn = (int)(Cash * 0.1);

            if (costOfRespawn < 100)
                costOfRespawn = 0;

            if (costOfRespawn > 5000)
            {
                costOfRespawn = 5000;
            }

            return costOfRespawn;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}