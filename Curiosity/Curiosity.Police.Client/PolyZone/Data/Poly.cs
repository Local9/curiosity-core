using CitizenFX.Core;
using System.Collections.Generic;
using System.Drawing;

namespace Curiosity.Police.Client.PolyZone.Data
{
    public class Poly
    {
        // events
        public delegate void PlayerPolygonEvent(bool isInside);

        public List<Vector2> Points = new List<Vector2>();
        public string Name;
        public float MinZ;
        public float MaxZ;

        public int GridDivisions = 25;
        public bool LazyGrid;

        public Color DebugColourWall = Color.FromArgb(255, 0, 255, 0);
        public Color DebugColourOutLine = Color.FromArgb(255, 255, 0, 0);
        public Color DebugColourGrid = Color.FromArgb(255, 255, 255, 255);
        public bool DebugPoly;
        public bool DebugGrid;

        public dynamic Data;

        // unknown but is in the code and is dynamicly added, because fuck Lua
        public float GridCellWidth;
        public float GridCellHeight;

        public Dictionary<float, float> GridXPoints = new Dictionary<float, float>();
        public Dictionary<float, float> GridYPoints = new Dictionary<float, float>();
        public double GridArea;
        public double GridCoverage;
        internal Dictionary<float, float> Grid = new Dictionary<float, float>();

        public Vector2 Min = Vector2.Zero;
        public Vector2 Max = Vector2.Zero;
        public Vector2 Size = Vector2.Zero;
        public Vector2 Center = Vector2.Zero;
        public double Area = 0;
        public double BoundingRadius;
        public List<Line> Lines = new List<Line>();

        public bool UseGrid;
        public bool IsPolyZone;

        void Draw()
        {

        }
    }

    public class Line
    {
        public Vector2 Min;
        public Vector2 Max;
    }
}
