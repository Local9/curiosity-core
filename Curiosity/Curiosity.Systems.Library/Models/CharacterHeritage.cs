using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Library.Models
{
    public class CharacterHeritage
    {
        public int FatherApperance { get; set; } = 0;
        public int MotherApperance { get; set; } = 0;
        public float BlendApperance { get; set; } = 0;
        public int FatherSkin { get; set; } = 0;
        public int MotherSkin { get; set; } = 0;
        public float BlendSkin { get; set; } = 0;

        public void UpdateBlendData(int fatherApperance, int motherApperance, int fatherSkin, int motherSkin, float apperanceBlend, float skinBlend)
        {
            FatherApperance = fatherApperance;
            MotherApperance = motherApperance;
            BlendApperance = apperanceBlend;
            FatherSkin = fatherSkin;
            MotherSkin = motherSkin;
            BlendSkin = skinBlend;
        }
    }
}
