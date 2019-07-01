using System;
using System.Collections.Generic;

namespace Curiosity.Global.Shared.net.Entity
{
    public class PlayerCharacter
    {
        public string Model = "mp_m_freemode_01";
        // PARENTS
        public int FatherAppearance = 0;
        public int MotherAppearance = 0;
        public float FatherMotherAppearanceGene = 25.0f;
        public int FatherSkin = 0;
        public int MotherSkin = 0;
        public float FatherMotherSkinGene = 25.0f;
        // FACE
        public int EyeColor = 0;
        // Hair
        public int HairColor = 0;
        public int HairSecondaryColor = 0;
        // OverLays
        public Dictionary<int, int> PedHeadOverlay;
        public Dictionary<int, Tuple<int, int>> PedHeadOverlayColor;
        // Additional Variations
        public Dictionary<int, Tuple<int, int>> Components;
        public Dictionary<int, Tuple<int, int>> Props;
    }
}
