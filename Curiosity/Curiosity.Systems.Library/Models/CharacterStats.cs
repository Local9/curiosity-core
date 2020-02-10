namespace Curiosity.Systems.Library.Models
{
    public class CharacterStats
    {
        public string MP0_STAMINA { get { return "MP0_STAMINA"; } }
        public string MP0_SHOOTING_ABILITY { get { return "MP0_SHOOTING_ABILITY"; } }
        public string MP0_STRENGTH { get { return "MP0_STRENGTH"; } }
        public string MP0_STEALTH_ABILITY { get { return "MP0_STEALTH_ABILITY"; } }
        public string MP0_FLYING_ABILITY { get { return "MP0_FLYING_ABILITY"; } }
        public string MP0_WHEELIE_ABILITY { get { return "MP0_WHEELIE_ABILITY"; } }
        public string MP0_LUNG_CAPACITY { get { return "MP0_LUNG_CAPACITY"; } }

        public int Stamina { get; set; } = 20;
        public int Shooting { get; set; } = 20;
        public int Strength { get; set; } = 20;
        public int Stealth { get; set; } = 20;
        public int Flying { get; set; } = 20;
        public int Driving { get; set; } = 20;
        public int LungCapacity { get; set; } = 20;
    }
}