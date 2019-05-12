using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class PointFExtension
    {
        public static PointF Add(this PointF c1, PointF c2)
        {
            return new PointF(c1.X + c2.X, c1.Y + c2.Y);
        }

        public static PointF Subtract(this PointF c1, PointF c2)
        {
            return new PointF(c1.X - c2.X, c1.Y - c2.Y);
        }

    }
}
