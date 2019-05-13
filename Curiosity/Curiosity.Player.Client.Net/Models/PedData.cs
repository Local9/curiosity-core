using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Models
{
    public class PedData
    {
        public Dictionary<byte, HeadOverlay> HeadOverlays = new Dictionary<byte, HeadOverlay>();

        public byte FatherHead;
        public byte MotherHead;
        public byte FatherSkin;
        public byte MotherSkin;
        public float HeadWeight;
        public float SkinWeight;

        public Dictionary<int, float> FacialFeatures = new Dictionary<int, float>();

        public byte PrimaryHairColor;
        public byte SecondaryHairColor;

        public byte EyeColor;
    }

    public class HeadOverlay
    {
        public byte PrimaryColor;
        public byte SecondaryColor;
        public byte Variant;
        public float Opacity;
    }
}
