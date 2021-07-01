using System.Collections.Generic;

namespace Curiosity.Systems.Library.Models
{
    public class VehicleInfo
    {
        public Dictionary<string, int> colors = new Dictionary<string, int>();
        public bool customWheels;
        public Dictionary<int, bool> extras = new Dictionary<int, bool>();
        public int livery;
        public uint model;
        public Dictionary<int, int> mods = new Dictionary<int, int>();
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
