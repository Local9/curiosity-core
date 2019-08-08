using CitizenFX.Core;

namespace Curiosity.Shared.Client.net.Helper.Area
{
    class AreaCustom : AreaBase
    {
        public Vector2[] Points { get; set; }

        public override void Check()
        {
            if (CoordsInside(Game.PlayerPed.Position) && !this.PlayerInside)
            {
                this.PlayerInside = true;
                this.TriggerEnter();
            }
            else if (!CoordsInside(Game.PlayerPed.Position) && this.PlayerInside)
            {
                this.PlayerInside = false;
                this.TriggerExit();
            }
        }

        public override void Draw()
        {
            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 p = Points[i];
                World.DrawLine(new Vector3 { X = p.X, Y = p.Y, Z = 0.0f }, new Vector3 { X = p.X, Y = p.Y, Z = 200.0f }, System.Drawing.Color.FromArgb(255, 0, 0));
            }
        }

        public override bool CoordsInside(Vector3 pos)
        {
            bool inside = false;

            for (int i = 0; i < Points.Length; i++)
            {
                Vector2 p1 = Points[i];
                Vector2 p2 = Points[i == Points.Length - 1 ? 0 : i + 1];
                if (((p1.Y > pos.Y) != (p2.Y > pos.Y)) && (pos.X < (p2.X - p1.X) * (pos.Y - p1.Y) / (p2.Y - p1.Y) + p1.X))
                {
                    inside = !inside;
                }
            }           

            return inside;
        }
    }
}
