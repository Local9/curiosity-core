using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public struct VehicleInfo
    {
        public Dictionary<string, int> colors;
        public bool customWheels;
        public Dictionary<int, bool> extras;
        public int livery;
        public uint model;
        public Dictionary<int, int> mods;
        public string name;
        public bool neonBack;
        public bool neonFront;
        public bool neonLeft;
        public bool neonRight;
        public string plateText;
        public int plateStyle;
        public bool turbo;
        public bool tyreSmoke;
        public int version;
        public int wheelType;
        public int windowTint;
        public bool xenonHeadlights;
        public bool bulletProofTires;
        public int headlightColor;
    };
}
