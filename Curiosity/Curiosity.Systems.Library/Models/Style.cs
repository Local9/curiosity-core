using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Library.Models
{
    public class Style
    {
        public int Gender { get; set; } = 0; // Default Gender is Male
        public int FatherApperance { get; set; }
        public int MotherApperance { get; set; }
        public float BlendApperance { get; set; }
        public int FatherSkin { get; set; }
        public int MotherSkin { get; set; }
        public float BlendSkin { get; set; }
        public int EyeColor { get; set; }
        public int HairPrimaryColor { get; set; }
        public int HairSecondaryColor { get; set; }

        public void UpdateBlendData(int fatherApperance, int motherApperance, int fatherSkin, int motherSkin, float apperanceBlend, float skinBlend)
        {
            FatherApperance = fatherApperance;
            MotherApperance = motherApperance;
            BlendApperance = apperanceBlend;
            FatherSkin = fatherSkin;
            MotherSkin = motherSkin;
            BlendSkin = skinBlend;
        }

        public void UpdateHairData(int hairPrimaryColor, int hairSecondaryColor)
        {
            HairPrimaryColor = hairPrimaryColor;
            HairSecondaryColor = hairSecondaryColor;
        }
    }
}
