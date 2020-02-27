using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Library.Models
{
    public class CharacterHeritage
    {
        public int FatherId { get; set; } = 0;
        public int MotherId { get; set; } = 0;
        public float BlendApperance { get; set; } = 0;
        public float BlendSkin { get; set; } = 0;

        public void UpdateBlendData(int fartherId, int motherId, float apperanceBlend, float skinBlend)
        {
            FatherId = fartherId;
            MotherId = motherId;
            BlendApperance = apperanceBlend;
            BlendSkin = skinBlend;
        }
    }
}
