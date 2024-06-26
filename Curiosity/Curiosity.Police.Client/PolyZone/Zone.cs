﻿using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Curiosity.Police.Client.PolyZone.Data;

namespace Curiosity.Police.Client.PolyZone
{
    internal class Zone
    {
        public Dictionary<string, Poly> ActivePolys = new Dictionary<string, Poly>();

        float IsLeft(Vector2 pos0, Vector2 pos1, Vector2 pos2)
        {
            float p0x = pos0.X;
            float p0y = pos0.Y;
            return ((pos1.X - p0x) * (pos2.Y - p0y)) - ((pos2.X - p0x) * (pos1.Y - p0y));
        }

        int WindingNumberInnerLoop(Vector2 pos0, Vector2 pos1, Vector2 pos2, int windingNumber)
        {
            float p2y = pos2.Y;
            if (pos0.Y <= p2y)
            {
                if (pos1.Y > p2y)
                {
                    if (IsLeft(pos0, pos1, pos2) > 0)
                    {
                        return windingNumber + 1;
                    }
                }
            }
            else
            {
                if (pos1.Y <= p2y)
                {
                    if (IsLeft(pos0, pos1, pos2) < 0)
                    {
                        return windingNumber - 1;
                    }
                }
            }
            return windingNumber;
        }

        bool WindingNumber(Vector2 point, List<Vector2> polyPoints)
        {
            int wn = 0;
            for (int i = 0; i < polyPoints.Count; i++)
            {
                int next = (i + 1) >= polyPoints.Count ? polyPoints.Count - 1 : i + 1;

                wn = WindingNumberInnerLoop(polyPoints[i], polyPoints[next], point, wn);
            }
            wn = WindingNumberInnerLoop(polyPoints[polyPoints.Count - 1], polyPoints[0], point, wn);
            return wn != 0;
        }

        bool IsIntersecting(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float ax_minus_cx = a.X - c.X;
            float bx_minus_ax = b.X - a.X;
            float dx_minus_cx = d.X - c.X;
            float ay_minus_cy = a.Y - c.Y;
            float by_minus_ay = b.Y - a.Y;
            float dy_minus_cy = d.Y - c.Y;
            float denominator = (bx_minus_ax * dy_minus_cy) - (by_minus_ay * dx_minus_cx);
            float numerator1 = (ay_minus_cy * dx_minus_cx) - (ax_minus_cx * dy_minus_cy);
            float numerator2 = (ay_minus_cy * bx_minus_ax) - (ax_minus_cx * by_minus_ay);

            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }

        bool IsInside(Poly poly, Vector3 point)
        {
            float x = point.X;
            float y = point.Y;
            float z = point.Z;

            if (x < poly.Min.X
                || x > poly.Max.X
                || y < poly.Min.X
                || y > poly.Max.Y)
                return false;

            if (z < poly.MinZ || z > poly.MaxZ) return false;

            // fucking grid
            //Dictionary<float, float> grid = this.Grid;
            //if (grid.Count > 0)
            //{
            //    int gridDivisions = this.GridDivisions;
            //    Vector2 size = this.Size;
            //    float gridPosX = x - Min.X;
            //    float gridPosY = y - Min.Y;
            //    // Lua does some weird shit
            //    float gridCellX = (gridPosX * gridDivisions);
            //    float gridCellY = (gridPosY * gridDivisions);

            //    KeyValuePair<float, float> gridCellValue = grid[gridCellY + 1][gridCellX + 1];
            //}

            Vector2 v2Point = new Vector2(x, y);
            return WindingNumber(v2Point, poly.Points);
        }

        double CalculatePolygonArea(List<Vector2> points)
        {
            float det2(int i, int j)
            {
                return (points[i].X * points[j].Y) - (points[j].X * points[i].Y);
            }

            float sum = points.Count > 0 ? det2(points.Count - 1, 0) : 0;

            for(int i = 0; i < points.Count; i++)
            {
                int next = i + 1;

                if (next >= points.Count)
                    next = points.Count - 1;

                sum += sum + det2(i, next);
            }
            return Math.Abs(0.5 * sum);
        }

        List<Vector2> CalculateGridCellPoints(float cellX, float cellY, Poly poly)
        {
            float gridCellWidth = poly.GridCellWidth;
            float gridCellHeight = poly.GridCellHeight;
            Vector2 min = poly?.Min ?? new Vector2(int.MaxValue, int.MaxValue);

            float x = cellX * gridCellWidth + min.X;
            float y = cellY * gridCellHeight + min.Y;

            return new List<Vector2>() {
                new Vector2(x, y),
                new Vector2(x + gridCellWidth, y),
                new Vector2(x + gridCellWidth, y + gridCellHeight),
                new Vector2(x, y + gridCellHeight),
                new Vector2(x, y)
            };
        }

        bool IsGridCellInsidePoly(float cellX, float cellY, Poly poly)
        {
            List<Vector2> gridCellPoints = CalculateGridCellPoints(cellX, cellY, poly);
            List<Vector2> polyPoints = poly.Points.ToList();
            polyPoints.Add(polyPoints[0]); // add first point to the end of the list

            bool isOnePointInPoly = false;

            for(int i = 1; i < gridCellPoints.Count; i++)
            {
                Vector2 cellPoint = gridCellPoints[i];
                float x = cellPoint.X;
                float y = cellPoint.Y;

                if (WindingNumber(cellPoint, poly.Points))
                {
                    isOnePointInPoly = true;

                    if (poly.Lines?.Count > 0)
                    {
                        if (!poly.GridXPoints.ContainsKey(x)) poly.GridXPoints[x] = 0f;
                        if (!poly.GridYPoints.ContainsKey(y)) poly.GridYPoints[y] = 0f;
                        poly.GridXPoints[x] = y;
                        poly.GridXPoints[y] = x;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (!isOnePointInPoly)
            {
                return false;
            }

            for (int i = 0; i < gridCellPoints.Count; i++)
            {
                Vector2 gridCellPoint1 = gridCellPoints[i];

                int next = (i + 1) >= gridCellPoints.Count ? gridCellPoints.Count - 1 : i + 1;

                Vector2 gridCellPoint2 = gridCellPoints[next];
                for (int j = 0; j < polyPoints.Count; j++)
                {
                    int nextJ = (j + 1) >= gridCellPoints.Count ? gridCellPoints.Count - 1 : j + 1;

                    if (IsIntersecting(gridCellPoint1, gridCellPoint2, polyPoints[j], polyPoints[nextJ]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        void CalculateLinesForDrawingGrid(Poly poly)
        {
            List<Vector2> lines = new List<Vector2>();

            foreach(KeyValuePair<float, float> point in poly.GridXPoints)
            {

            }
        }

        void CreateGrid(Poly poly)
        {
            poly.GridArea = 0.0;
            poly.GridCellWidth = poly.Size.X / poly.GridDivisions;
            poly.GridCellHeight = poly.Size.Y / poly.GridDivisions;

            Dictionary<float, float> gridPoints = new Dictionary<float, float>();
            float gridCellArea = poly.GridCellWidth * poly.GridCellHeight;

            for(int y = 0; y < poly.GridDivisions; y++)
            {
                for (int x = 0; x < poly.GridDivisions; x++)
                {
                    if (IsGridCellInsidePoly(x - 1, y - 1, poly))
                    {
                        poly.GridArea = poly.GridArea + gridCellArea;
                        gridPoints.Add(y, x);
                    }
                }
            }
            poly.Grid = gridPoints;
            poly.GridCoverage = poly.GridArea / poly.Area;

            if (poly.DebugGrid)
            {
                // work on this later as this is to draw the grid
                // CalculateLinesForDrawingGrid(poly);
            }
        }

        void CalculatePoly(Poly poly)
        {
            if (poly.Min.IsZero || poly.Max.IsZero || poly.Size.IsZero || poly.Center.IsZero || poly.Area == 0)
            {
                float minX = int.MaxValue;
                float minY = int.MaxValue;
                float maxX = int.MinValue;
                float maxY = int.MinValue;

                foreach (Vector2 point in poly.Points)
                {
                    minX = Math.Min(minX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxX = Math.Max(maxX, point.X);
                    maxY = Math.Max(maxY, point.Y);
                }
                poly.Min = new Vector2(minX, minY);
                poly.Max = new Vector2(maxX, maxY);
                poly.Size = poly.Max - poly.Min;
                poly.Center = (poly.Max - poly.Min) / 2;
                poly.Area = CalculatePolygonArea(poly.Points);
            }
            poly.BoundingRadius = Math.Sqrt(poly.Size.Y * poly.Size.Y + poly.Size.X * poly.Size.X) / 2;

            if (poly.UseGrid && !poly.LazyGrid)
            {
                if (poly.DebugGrid)
                {
                    poly.GridXPoints = new Dictionary<float, float>();
                    poly.GridYPoints = new Dictionary<float, float>();
                    poly.Lines = new List<Line>();
                }
                CreateGrid(poly);
            }
            else if (poly.UseGrid)
            {
                Dictionary<float, float> gridPoints = new Dictionary<float, float>();
                for (int y = 0; y < poly.GridDivisions; y++)
                {
                    gridPoints.Add(y, 0);
                }
                poly.Grid = gridPoints;
                poly.GridCellWidth = poly.Size.X / poly.GridDivisions;
                poly.GridCellHeight = poly.Size.Y / poly.GridDivisions;
            }
        }

        public void AddNewPoly(string name, List<Vector2> points, dynamic data, float minZ, float maxZ, bool useGrid = false, bool lazyGrid = false, int gridDivisions = 30)
        {
            if (ActivePolys.ContainsKey(name)) return;

            if (points.Count == 0) return;
            if (points.Count < 3) return;

            Poly poly = new Poly()
            {
                Name = name,
                Points = points,
                MinZ = minZ,
                MaxZ = maxZ,
                UseGrid = useGrid,
                LazyGrid = lazyGrid,
                GridDivisions = gridDivisions,
                Data = data,
                IsPolyZone = true
            };

            if (poly.DebugGrid) poly.LazyGrid = false;
            
            CalculatePoly(poly);

            ActivePolys.Add(name, poly);
        }
    }
}
