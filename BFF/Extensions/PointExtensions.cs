using System.Windows;

namespace BFF.Extensions
{
    public static class PointExtensions
    {
        public static Point Coerce(this Point point, double minX, double maxX, double minY, double maxY)
        {
            if (point.X < minX) point.X = minX;
            if (point.X > maxX) point.X = maxX;
            if (point.Y < minY) point.Y = minY;
            if (point.Y > maxY) point.Y = maxY;
            return point;
        }
    }
}
