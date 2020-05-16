using System;
using System.Drawing;

namespace Curiosity.Shared.Client.net.Extensions
{
    public static class ColorExtension
    {
        public static Color ToColor(this string color)
        {
            try
            {
                return Color.FromArgb(int.Parse(color.Replace("#", ""),
                             System.Globalization.NumberStyles.AllowHexSpecifier));
            }
            catch (Exception ex)
            {
                Log.Error($"ToColor exception: {ex.Data}");
            }
            return Color.FromArgb(255, 255, 255, 255);
        }
    }
}
