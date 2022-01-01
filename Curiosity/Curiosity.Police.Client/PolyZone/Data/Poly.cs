using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Collections.Generic;
using System.Drawing;
using CitizenFX.Core.UI;

namespace Curiosity.Police.Client.PolyZone.Data
{
    public class Poly
    {
        // events
        public delegate void PlayerPolygonEvent(bool isInside);

        public List<Vector2> Points = new List<Vector2>();
        public string Name;
        public float MinZ = 0;
        public float MaxZ = 0;

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

        public void Draw()
        {
            float zDrawDist = 45.0f;
            Vector3 playerPos = Game.PlayerPed.Position;
            float minZ = MinZ > 0 ? MinZ : playerPos.Z - zDrawDist;
            float maxZ = MaxZ > 0 ? MaxZ : playerPos.Z - zDrawDist;

            int oR = DebugColourOutLine.R;
            int oG = DebugColourOutLine.G;
            int oB = DebugColourOutLine.B;

            int wR = DebugColourWall.R;
            int wG = DebugColourWall.G;
            int wB = DebugColourWall.B;

            for (int i = 0; i < Points.Count; i++)
            {
                Vector2 point = Points[i];
                DrawLine(point.X, point.Y, minZ, point.X, point.Y, maxZ, oR, oG, oB, 164);

                if (i < Points.Count)
                {
                    int nextPointInt = i + 1;

                    if (nextPointInt >= Points.Count)
                        nextPointInt = Points.Count - 1;

                    Vector2 nextPoint = Points[nextPointInt];
                    DrawLine(point.X, point.Y, minZ, nextPoint.X, nextPoint.Y, maxZ, oR, oG, oB, 164);
                    DrawWall(point, nextPoint, minZ, maxZ, wR, wG, wB, 48);
                }
            }

            if (Points.Count > 2)
            {
                Vector2 firstPoint = Points[0];
                Vector2 lastPoint = Points[Points.Count - 1];
                DrawLine(firstPoint.X, firstPoint.Y, minZ, lastPoint.X, lastPoint.Y, maxZ, oR, oG, oB, 164);
                DrawWall(firstPoint, lastPoint, minZ, maxZ, wR, wG, wB, 48);
            }

            Screen.ShowSubtitle($"{Grid.Count}");
        }

        void DrawWall(Vector2 p1, Vector2 p2, float minZ, float maxZ, int r, int g, int b, int a)
        {
            Vector3 bottomLeft = new Vector3(p1.X, p1.Y, minZ);
            Vector3 topLeft = new Vector3(p1.X, p1.Y, maxZ);
            Vector3 bottomRight = new Vector3(p2.X, p2.Y, minZ);
            Vector3 topRight = new Vector3(p2.X, p2.Y, maxZ);

            DrawPoly(bottomLeft.X, bottomLeft.Y, bottomLeft.Z, topLeft.X, topLeft.Y, topLeft.Z, bottomRight.X, bottomRight.Y, bottomRight.Z, r, g, b, a);
            DrawPoly(topLeft.X, topLeft.Y, topLeft.Z, topRight.X, topRight.Y, topRight.Z, bottomRight.X, bottomRight.Y, bottomRight.Z, r, g, b, a);
            DrawPoly(bottomRight.X, bottomRight.Y, bottomRight.Z, topRight.X, topRight.Y, topRight.Z, topLeft.X, topLeft.Y, topLeft.Z, r, g, b, a);
            DrawPoly(bottomRight.X, bottomRight.Y, bottomRight.Z, topLeft.X, topLeft.Y, topLeft.Z, bottomLeft.X, bottomLeft.Y, bottomLeft.Z, r, g, b, a);
        }
    }

    public class Line
    {
        public Vector2 Min;
        public Vector2 Max;
    }
}
