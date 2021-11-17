using System.Collections.Generic;

namespace Curiosity.Police.Client.Environment.Data
{
    static class CompassDirections
    {
        public static Dictionary<int, string> Direction = new()
        {
            { 0, "N" },
            { 45, "NW" },
            { 90, "W" },
            { 135, "SW" },
            { 180, "S" },
            { 225, "SE" },
            { 270, "E" },
            { 315, "NE" },
            { 360, "N" }
        };
    }
}
