using System;

namespace TomekDexValheimMod
{
    [Serializable]
    public struct Point3D
    {
        public float x;
        public float y;
        public float z;

        public static Point3D Zero { get { return new Point3D(0, 0, 0); } }

        public Point3D(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public float Distance(Point3D point1)
        {
            float xDiff = point1.x - x;
            float yDiff = point1.y - y;
            float zDiff = point1.z - z;
            float distance = (float)Math.Sqrt(xDiff * xDiff + yDiff * yDiff + zDiff * zDiff);
            return distance;
        }

        public static Point3D operator -(Point3D point1, Point3D point2)
        {
            float x = point1.x - point2.x;
            float y = point1.y - point2.y;
            float z = point1.z - point2.z;

            return new Point3D(x, y, z);
        }

        public static Point3D operator /(Point3D point, float scalar)
        {
            float x = point.x / scalar;
            float y = point.y / scalar;
            float z = point.z / scalar;

            return new Point3D(x, y, z);
        }

        public static Point3D operator +(Point3D point1, Point3D point2)
        {
            float x = point1.x + point2.x;
            float y = point1.y + point2.y;
            float z = point1.z + point2.z;

            return new Point3D(x, y, z);
        }

        public static Point3D operator -(Point3D point)
        {
            return new Point3D(-point.x, -point.y, -point.z);
        }

        public static Point3D operator *(Point3D point, float scalar)
        {
            float x = point.x * scalar;
            float y = point.y * scalar;
            float z = point.z * scalar;
            return new Point3D(x, y, z);
        }

        public static Point3D Cross(Point3D p, Point3D q)
        {
            float x = p.y * q.z - p.z * q.y;
            float y = p.z * q.x - p.x * q.z;
            float z = p.x * q.y - p.y * q.x;
            return new Point3D(x, y, z);
        }

        public static float Dot(Point3D p, Point3D q)
        {
            return p.x * q.x + p.y * q.y + p.z * q.z;
        }

        public Point3D Normalized()
        {
            float length = (float)Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            float x = this.x / length;
            float y = this.y / length;
            float z = this.z / length;
            return new Point3D(x, y, z);
        }

        public float Magnitude()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
    }
}
