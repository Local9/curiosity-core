using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;

namespace Curiosity.Shared.Client.net.Helper.Area
{
    public class AreaRectangle : AreaBase
    {
        public Vector2 Pos1 { get; set; }
        public Vector2 Pos2 { get; set; }
        public float Angle { get; set; }

        public override void Check()
        {
            if (API.IsEntityInArea(Game.PlayerPed.Handle, Pos1.X, Pos1.Y, 0.0f, Pos2.X, Pos2.Y, 200.0f, false, false, -1) && !this.PlayerInside)
            {
                this.PlayerInside = true;
                this.TriggerEnter();
            }
            else if (!API.IsEntityInArea(Game.PlayerPed.Handle, Pos1.X, Pos1.Y, 0.0f, Pos2.X, Pos2.Y, 200.0f, false, false, -1) && this.PlayerInside)
            {
                this.PlayerInside = false;
                this.TriggerExit();
            }
        }

        public override void Draw()
        {
            if (Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared2D(((new Vector3 { X = Pos1.X, Y = Pos1.Y, Z = 0.0f }) + (new Vector3 { X = Pos2.X, Y = Pos2.Y, Z = 0.0f })) / 2)) > 
                Math.Sqrt((new Vector3 { X = Pos1.X, Y = Pos1.Y, Z = 0.0f }).DistanceToSquared2D(new Vector3 { X = Pos2.X, Y = Pos2.Y, Z = 0.0f })) * 5)
                return;

            Vector3 center = new Vector3 { X = (Pos1.X + Pos2.X) / 2, Y = (Pos1.Y + Pos2.Y) / 2, Z = 0.0f };
            Matrix3x3 rot = Matrix3x3.RotationZ(Angle);

            Vector3 A = new Vector3(Pos1, 0.0f);
            Vector3 B = new Vector3 { X = Pos2.X, Y = Pos1.Y, Z = 0.0f };
            Vector3 C = new Vector3 { X = Pos1.X, Y = Pos2.Y, Z = 0.0f };
            Vector3 D = new Vector3 { X = Pos2.X, Y = Pos2.Y, Z = 0.0f };

            A = A - center;
            B = B - center;
            C = C - center;
            D = D - center;

            A = Vector3.Transform(A, rot);
            B = Vector3.Transform(B, rot);
            C = Vector3.Transform(C, rot);
            D = Vector3.Transform(D, rot);

            A = A + center;
            B = B + center;
            C = C + center;
            D = D + center;

            World.DrawLine(new Vector3 { X = A.X, Y = A.Y, Z = 0.0f }, new Vector3 { X = A.X, Y = A.Y, Z = 200.0f }, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(new Vector3 { X = B.X, Y = B.Y, Z = 0.0f }, new Vector3 { X = B.X, Y = B.Y, Z = 200.0f }, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(new Vector3 { X = C.X, Y = C.Y, Z = 0.0f }, new Vector3 { X = C.X, Y = C.Y, Z = 200.0f }, System.Drawing.Color.FromArgb(255, 0, 0));
            World.DrawLine(new Vector3 { X = D.X, Y = D.Y, Z = 0.0f }, new Vector3 { X = D.X, Y = D.Y, Z = 200.0f }, System.Drawing.Color.FromArgb(255, 0, 0));

        }
        public override bool CoordsInside(Vector3 coords)
        {
            Vector3 A = new Vector3(Vector2.Min(Pos1, Pos2), 0.0f);
            Vector3 B = new Vector3(Vector2.Max(Pos1, Pos2), 0.0f);

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
