using CitizenFX.Core;
using System;

namespace Curiosity.Shared.Client.net.Helper.Area
{
    public class AreaSphere : AreaBase
    {
        public Vector3 Pos { get; set; }
        public float Radius { get; set; }

        public override void Check()
        {
            if (Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared(this.Pos)) <= this.Radius && !this.PlayerInside)
            {
                this.PlayerInside = true;
                this.TriggerEnter();
            }
            else if (Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared(this.Pos)) > this.Radius && this.PlayerInside)
            {
                this.PlayerInside = false;
                this.TriggerExit();
            }
        }

        public override void Draw()
        {
            if (Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared(Pos)) > Radius * 5)
                return;

            Vector3 end;
            float angle = (float)(Math.Cos(Math.PI / 4));

            end = new Vector3 { X = Pos.X, Y = Pos.Y, Z = Pos.Z + Radius };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X, Y = Pos.Y, Z = Pos.Z - Radius };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X + Radius, Y = Pos.Y, Z = Pos.Z };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X - Radius, Y = Pos.Y, Z = Pos.Z };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X, Y = Pos.Y + Radius, Z = Pos.Z };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X, Y = Pos.Y - Radius, Z = Pos.Z };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X, Y = Pos.Y + Radius * angle, Z = Pos.Z + Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X, Y = Pos.Y - Radius * angle, Z = Pos.Z + Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X + Radius * angle, Y = Pos.Y, Z = Pos.Z + Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X - Radius * angle, Y = Pos.Y, Z = Pos.Z + Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X, Y = Pos.Y + Radius * angle, Z = Pos.Z - Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X, Y = Pos.Y - Radius * angle, Z = Pos.Z - Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X + Radius * angle, Y = Pos.Y, Z = Pos.Z - Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X - Radius * angle, Y = Pos.Y, Z = Pos.Z - Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X + Radius * angle * angle, Y = Pos.Y + Radius * angle * angle, Z = Pos.Z + Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X + Radius * angle * angle, Y = Pos.Y - Radius * angle * angle, Z = Pos.Z + Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X - Radius * angle * angle, Y = Pos.Y - Radius * angle * angle, Z = Pos.Z + Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X - Radius * angle * angle, Y = Pos.Y + Radius * angle * angle, Z = Pos.Z + Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X + Radius * angle * angle, Y = Pos.Y + Radius * angle * angle, Z = Pos.Z - Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X + Radius * angle * angle, Y = Pos.Y - Radius * angle * angle, Z = Pos.Z - Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X - Radius * angle * angle, Y = Pos.Y - Radius * angle * angle, Z = Pos.Z - Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X - Radius * angle * angle, Y = Pos.Y + Radius * angle * angle, Z = Pos.Z - Radius * angle };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X + Radius * angle, Y = Pos.Y + Radius * angle, Z = Pos.Z };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X + Radius * angle, Y = Pos.Y - Radius * angle, Z = Pos.Z };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X - Radius * angle, Y = Pos.Y - Radius * angle, Z = Pos.Z };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));

            end = new Vector3 { X = Pos.X - Radius * angle, Y = Pos.Y + Radius * angle, Z = Pos.Z };
            World.DrawLine(Pos, end, System.Drawing.Color.FromArgb(255, 0, 0));
        }

        public override bool CoordsInside(Vector3 coords)
        {
            return Math.Sqrt(coords.DistanceToSquared(this.Pos)) <= this.Radius;
        }
    }
}
