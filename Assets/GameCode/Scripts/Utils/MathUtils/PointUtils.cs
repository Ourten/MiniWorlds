using UnityEngine;

namespace GameCode.Scripts.Utils.MathUtils
{
    public static class PointUtils
    {
        public static bool IsPointInsideSphere(Vector3Int center, Vector3Int point, int radius)
        {
            return IsPointInsideSphere(center, point.x, point.y, point.z, radius);
        }

        public static bool IsPointInsideSphere(Vector3Int center, int pointX, int pointY, int pointZ, int radius)
        {
            return IsPointInsideSphere(center.x, center.y, center.z, pointX, pointY, pointZ, radius);
        }

        public static bool IsPointInsideSphere(int centerX, int centerY, int centerZ, int pointX, int pointY,
            int pointZ, int radius)
        {
            return System.Math.Pow(pointX - centerX, 2)
                   + System.Math.Pow(pointY - centerY, 2) +
                   System.Math.Pow(pointZ - centerZ, 2)
                   < System.Math.Pow(radius, 2);
        }
    }
}