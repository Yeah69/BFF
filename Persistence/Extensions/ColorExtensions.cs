using System.Drawing;

namespace BFF.Persistence.Extensions
{
    internal static class ColorExtensions
    {
        internal static long ToLong(this Color color) => color.A << 24 | color.R << 16 | color.G << 8 | color.B << 0;
    }
}
