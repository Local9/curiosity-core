using CitizenFX.Core;
using System;

namespace Curiosity.Shared.Client.net.Helper.Area
{
    public class AreaBox : AreaBase
    {
        public Vector3 Pos1 { get; set; }
        public Vector3 Pos2 { get; set; }
        public float Angle { get; set; }

        public override void Check()
        {
            if (CoordsInside(Game.PlayerPed.Position) && !this.PlayerInside)
            {
                Log.Info("Entered Area");
                this.PlayerInside = true;
                this.TriggerEnter();
            }
            else if (!CoordsInside(Game.PlayerPed.Position) && this.PlayerInside)
            {
                Log.Info("Exited Area");
                this.PlayerInside = false;
                this.TriggerExit();
            }
        }

        public override void Draw()
        {
            if (Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared((Pos1 + Pos2) / 2)) > Math.Sqrt(Pos1.DistanceToSquared(Pos2)) * 5)
                return;

            Vector3 center = new Vector3 { X = (Pos1.X + Pos2.X) / 2, Y = (Pos1.Y + Pos2.Y) / 2, Z = (Pos1.Z + Pos2.Z) / 2 };
            Matrix3x3 rot = Matrix3x3.RotationZ(Angle);

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

            World.DrawLine(A, B, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(A, C, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(A, E, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(H, F, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(H, G, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(H, D, System.Drawing.Color.FromArgb(255, 0, 0));

            World.DrawLine(D, B, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(D, C, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(G, B, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(G, E, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(F, E, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(F, C, System.Drawing.Color.FromArgb(255, 0, 0));

        }

        public override bool CoordsInside(Vector3 coords)
        {
            Vector3 A = Vector3.Min(Pos1, Pos2);
            Vector3 B = Vector3.Max(Pos1, Pos2);
            if (!(coords.Z >= Pos1.Z && coords.Z <= Pos2.Z))
                return false;

            Vector3 center = new Vector3 { X = (A.X + B.X) / 2, Y = (A.Y + B.Y) / 2, Z = (A.Z + B.Z) / 2 };
            Matrix3x3 rot = Matrix3x3.RotationZ(-Angle);

            Vector3 C = coords;

            C = C - center;

            C = Vector3.Transform(C, rot);

            C = C + center;

            return (C.X >= A.X && C.X <= B.X && C.Y >= A.Y && C.Y <= B.Y);
        }
    }
}
