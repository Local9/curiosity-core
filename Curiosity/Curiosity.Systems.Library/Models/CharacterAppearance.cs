using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class CharacterAppearance
    {
        public int EyeColor { get; set; } = 0;
        public int HairStyle { get; set; } = 0;
        public KeyValuePair<string, string> HairOverlay { get; set; } = new KeyValuePair<string, string>("", "");
        public int HairPrimaryColor { get; set; } = 0;
        public int HairSecondaryColor { get; set; } = 0;
        public int Eyebrow { get; set; } = 0;
        public float EyebrowOpacity { get; set; } = 0f;
        public int EyebrowColor { get; set; } = 0;
        public int FacialHair { get; set; } = 0;
        public float FacialHairOpacity { get; set; } = 0f;
        public int FacialHairColor { get; set; } = 0;
        public int SkinBlemish { get; set; } = 0;
        public float SkinBlemishOpacity { get; set; } = 0f;
        public int SkinAging { get; set; } = 0;
        public float SkinAgingOpacity { get; set; } = 0f;
        public int SkinComplexion { get; set; } = 0;
        public float SkinComplexionOpacity { get; set; } = 0f;
        public int SkinMoles { get; set; } = 0;
        public float SkinMolesOpacity { get; set; } = 0f;
        public int SkinDamage { get; set; } = 0;
        public float SkinDamageOpacity { get; set; } = 0f;
        public int EyeMakeup { get; set; } = 0;
        public float EyeMakeupOpacity { get; set; } = 0f;
        public int EyeMakeupColor { get; set; } = 0;
        public int Blusher { get; set; } = 0;
        public float BlusherOpacity { get; set; } = 0f;
        public int BlusherColor { get; set; } = 0;
        public int Lipstick { get; set; } = 0;
        public float LipstickOpacity { get; set; } = 0f;
        public int LipstickColor { get; set; } = 0;

    }
}