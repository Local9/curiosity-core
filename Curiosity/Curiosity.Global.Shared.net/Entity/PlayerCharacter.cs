using System;
using System.Collections.Generic;

namespace Curiosity.Global.Shared.Entity
{
    public class PlayerCharacter
    {
        Random rnd = new Random();

        int _fathersApperance = -1;
        int _mothersApperance = -1;
        int _fathersSkin = -1;
        int _mothersSkin = -1;
        int _hairColor = -1;
        int _hairSecondaryColor = -1;

        public string Model = "mp_m_freemode_01";
        // PARENTS
        public int FatherAppearance
        {
            get
            {
                return _fathersApperance != -1 ? _fathersApperance : rnd.Next(45);
            }
            set
            {
                _fathersApperance = value;
            }
        }

        public int MotherAppearance
        {
            get
            {
                return _mothersApperance != -1 ? _mothersApperance : rnd.Next(45);
            }
            set
            {
                _mothersApperance = value;
            }
        }

        public int FatherMotherAppearanceGene = 25;
        public int FatherSkin
        {
            get
            {
                return _fathersSkin != -1 ? _fathersSkin : rnd.Next(45);
            }
            set
            {
                _fathersSkin = value;
            }
        }

        public int MotherSkin
        {
            get
            {
                return _mothersSkin != -1 ? _mothersSkin : rnd.Next(45);
            }
            set
            {
                _mothersSkin = value;
            }
        }

        public int FatherMotherSkinGene = 25;
        // FACE
        public int EyeColor = 0;
        // Hair
        public int HairColor
        {
            get
            {
                return _hairColor != -1 ? _hairColor : rnd.Next(63);
            }
            set
            {
                _hairColor = value;
            }
        }

        public int HairSecondaryColor
        {
            get
            {
                return _hairSecondaryColor != -1 ? _hairSecondaryColor : rnd.Next(63);
            }
            set
            {
                _hairSecondaryColor = value;
            }
        }

        // OverLays
        public Dictionary<int, int> PedHeadOverlay = new Dictionary<int, int>();
        public Dictionary<int, Tuple<int, int>> PedHeadOverlayColor = new Dictionary<int, Tuple<int, int>>();
        // Additional Variations
        public Dictionary<int, Tuple<int, int>> Components = new Dictionary<int, Tuple<int, int>>();
        public Dictionary<int, Tuple<int, int>> Props = new Dictionary<int, Tuple<int, int>>();
    }
}
