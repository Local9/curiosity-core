using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Drawing;

namespace Curiosity.Racing.Client.Utils
{
    internal class WorldArea
    {
        private Color red = Color.FromArgb(255, 0, 0);
        private Color green = Color.FromArgb(0, 255, 0);

        private Vector3? _pos2;

        public Vector3 Pos1 { get; set; }
        public Vector3 Pos2
        {
            get
            {
                return _pos2 ?? Pos1 + new Vector3(Width);
            }
            set
            {
                _pos2 = value;
            }
        }
        public float Width { get; set; }
        public Vector3? MarkerScale;

        public bool IsInArea => Game.PlayerPed.IsInAngledArea(Pos1, Pos2, Width);

        public void Draw()
        {
            if (Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared((Pos1 + Pos2) / 2)) > Math.Sqrt(Pos1.DistanceToSquared(Pos2)) * 5)
                return;

            Vector3 center = new Vector3 { X = (Pos1.X + Pos2.X) / 2, Y = (Pos1.Y + Pos2.Y) / 2, Z = (Pos1.Z + Pos2.Z) / 2 };

            Vector3 markerPosition = center;
            float _height = 0f;

            if (API.GetGroundZFor_3dCoord(markerPosition.X, markerPosition.Y, markerPosition.Z, ref _height, false))
                markerPosition = new Vector3(markerPosition.X, markerPosition.Y, _height + 0.03f);

            // World.DrawMarker(MarkerType.VerticalCylinder, markerPosition, Vector3.Zero, Vector3.Zero, MarkerScale ?? new Vector3(Width), green);

            float rX = Pos1.X - Pos2.X;
            float rY = Pos1.Y - Pos2.Y;
            float rZ = Pos1.Z - Pos2.Z;
            Vector3 rotation = new Vector3(rX, rY, rZ);
            rotation.Normalize();

            float r = rotation.Z * ((float)Math.PI / 180.0f);

            Matrix3x3 rot = Matrix3x3.RotationZ(rotation.Z);

            Vector3 A = Pos1;
            Vector3 B = new Vector3 { X = Pos2.X, Y = Pos1.Y, Z = Pos1.Z };
            Vector3 C = new Vector3 { X = Pos1.X, Y = Pos2.Y, Z = Pos1.Z };
            Vector3 D = new Vector3 { X = Pos2.X, Y = Pos2.Y, Z = Pos1.Z };
            Vector3 E = new Vector3 { X = Pos1.X, Y = Pos1.Y, Z = Pos2.Z };
            Vector3 F = new Vector3 { X = Pos1.X, Y = Pos2.Y, Z = Pos2.Z };
            Vector3 G = new Vector3 { X = Pos2.X, Y = Pos1.Y, Z = Pos2.Z };
            Vector3 H = Pos2;

            A = A - center;
            B = B - center;
            C = C - center;
            D = D - center;
            E = E - center;
            F = F - center;
            G = G - center;
            H = H - center;

            A = Vector3.Transform(A, rot);
            B = Vector3.Transform(B, rot);
            C = Vector3.Transform(C, rot);
            D = Vector3.Transform(D, rot);
            E = Vector3.Transform(E, rot);
            F = Vector3.Transform(F, rot);
            G = Vector3.Transform(G, rot);
            H = Vector3.Transform(H, rot);

            A = A + center;
            B = B + center;
            C = C + center;
            D = D + center;
            E = E + center;
            F = F + center;
            G = G + center;
            H = H + center;

            Color color = IsInArea ? green : red;

            World.DrawMarker(MarkerType.DebugSphere, Pos1, Vector3.Zero, Vector3.Zero, new Vector3(0.1f), color);
            World.DrawMarker(MarkerType.DebugSphere, Pos2, Vector3.Zero, Vector3.Zero, new Vector3(0.1f), color);

            //World.DrawMarker(MarkerType.DebugSphere, A, Vector3.Zero, Vector3.Zero, new Vector3(0.5f), color);
            //World.DrawMarker(MarkerType.DebugSphere, H, Vector3.Zero, Vector3.Zero, new Vector3(0.5f), color);

            World.DrawLine(A, B, color);
            World.DrawLine(A, C, color);
            World.DrawLine(A, E, color);
            World.DrawLine(H, F, color);
            World.DrawLine(H, G, color);
            World.DrawLine(H, D, color);

            World.DrawLine(D, B, color);
            World.DrawLine(D, C, color);
            World.DrawLine(G, B, color);
            World.DrawLine(G, E, color);
            World.DrawLine(F, E, color);
            World.DrawLine(F, C, color);

        }
    }
}
