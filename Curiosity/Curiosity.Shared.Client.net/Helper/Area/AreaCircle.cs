using CitizenFX.Core;
using System;

namespace Curiosity.Shared.Client.net.Helper.Area
{
    public class AreaCircle : AreaBase
    {
        public Vector2 Pos { get; set; }
        public float Radius { get; set; }

        private Vector3 Pos3D
        {
            get
            {
                return new Vector3 { X = Pos.X, Y = Pos.Y, Z = 0.0f };
            }
        }

        public override void Check()
        {
            if (Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared2D(this.Pos3D)) <= this.Radius && !this.PlayerInside)
            {
                this.PlayerInside = true;
                this.TriggerEnter();
            }
            else if (Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared2D(this.Pos3D)) > this.Radius && this.PlayerInside)
            {
                this.PlayerInside = false;
                this.TriggerExit();
            }
        }

        public override void Draw()
        {

            if (Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared2D(Pos3D)) > Radius * 5)
                return;

            Vector3 start;
            Vector3 end;
            float angle = (float)(Math.Cos(Math.PI / 4));

            start = new Vector3 { X = Pos.X, Y = Pos.Y + Radius, Z = 0.0f };
            end = new Vector3 { X = Pos.X, Y = Pos.Y + Radius, Z = 200.0f };
            World.DrawLine(start, end, System.Drawing.Color.FromArgb(255, 0, 0));

            start = new Vector3 { X = Pos.X + Radius, Y = Pos.Y, Z = 0.0f };
            end = new Vector3 { X = Pos.X + Radius, Y = Pos.Y, Z = 200.0f };
            World.DrawLine(start, end, System.Drawing.Color.FromArgb(255, 0, 0));

            start = new Vector3 { X = Pos.X, Y = Pos.Y - Radius, Z = 0.0f };
            end = new Vector3 { X = Pos.X, Y = Pos.Y - Radius, Z = 200.0f };
            World.DrawLine(start, end, System.Drawing.Color.FromArgb(255, 0, 0));

            start = new Vector3 { X = Pos.X - Radius, Y = Pos.Y, Z = 0.0f };
            end = new Vector3 { X = Pos.X - Radius, Y = Pos.Y, Z = 200.0f };
            World.DrawLine(start, end, System.Drawing.Color.FromArgb(255, 0, 0));

            start = new Vector3 { X = Pos.X + Radius * angle, Y = Pos.Y + Radius * angle, Z = 0.0f };
            end = new Vector3 { X = Pos.X + Radius * angle, Y = Pos.Y + Radius * angle, Z = 200.0f };
            World.DrawLine(start, end, System.Drawing.Color.FromArgb(255, 0, 0));

            start = new Vector3 { X = Pos.X - Radius * angle, Y = Pos.Y + Radius * angle, Z = 0.0f };
            end = new Vector3 { X = Pos.X - Radius * angle, Y = Pos.Y + Radius * angle, Z = 200.0f };
            World.DrawLine(start, end, System.Drawing.Color.FromArgb(255, 0, 0));

            start = new Vector3 { X = Pos.X + Radius * angle, Y = Pos.Y - Radius * angle, Z = 0.0f };
            end = new Vector3 { X = Pos.X + Radius * angle, Y = Pos.Y - Radius * angle, Z = 200.0f };
            World.DrawLine(start, end, System.Drawing.Color.FromArgb(255, 0, 0));

            start = new Vector3 { X = Pos.X - Radius * angle, Y = Pos.Y - Radius * angle, Z = 0.0f };
            end = new Vector3 { X = Pos.X - Radius * angle, Y = Pos.Y - Radius * angle, Z = 200.0f };
            World.DrawLine(start, end, System.Drawing.Color.FromArgb(255, 0, 0));
        }

        public override bool CoordsInside(Vector3 coords)
        {
            return Math.Sqrt(coords.DistanceToSquared2D(this.Pos3D)) <= this.Radius;
        }
    }
}
