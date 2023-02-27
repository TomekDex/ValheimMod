using System;
using UnityEngine;

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

        public static implicit operator Point3D(Vector3 v)
        {
            return new Point3D(v.x, v.y, v.z);
        }

        public static implicit operator Vector3(Point3D p)
        {
            return new Vector3(p.x, p.y, p.z);
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

        //private static bool IsParallelTo(Point3D p1, Point3D p2, Point3D p3, Point3D q1, Point3D q2, Point3D q3)
        //{
        //    Point3D pNormal = GetNormal(p1, p2, p3);
        //    Point3D qNormal = GetNormal(q1, q2, q3);

        //    if (pNormal.IsZeroVector() || qNormal.IsZeroVector())
        //    {
        //        return false;
        //    }

        //    float dotProduct = DotProduct(pNormal, qNormal);
        //    return Math.Abs(dotProduct - 1.0) < 0.0001;
        //}

        //private bool IsZeroVector()
        //{
        //    return X == 0 && Y == 0 && Z == 0;
        //}

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

        //private static Point3D GetNormal(Point3D p1, Point3D p2, Point3D p3)
        //{
        //    Point3D pEdge1 = new Point3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        //    Point3D pEdge2 = new Point3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);
        //    return CrossProduct(pEdge1, pEdge2);
        //}


        //private static float[] GetProjectionX(Point3D p1, Point3D p2, Point3D p3, Point3D normal)
        //{
        //    return new float[] {
        //DotProduct(p1, normal),
        //DotProduct(p2, normal),
        //DotProduct(p3, normal)};
        //}

        //private static float DotProduct(Point3D p1, Point3D p2)
        //{
        //    return p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z;
        //}
        //private static void GetMinMax(float[] values, out float min, out float max)
        //{
        //    min = max = values[0];
        //    for (int i = 1; i < values.Length; i++)
        //    {
        //        if (values[i] < min)
        //        {
        //            min = values[i];
        //        }
        //        else
        //        if (values[i] > max)
        //        {
        //            max = values[i];
        //        }
        //    }
        //}

        //private static bool IsOverlap(float min1, float max1, float min2, float max2)
        //{
        //    return max1 >= min2 && max2 >= min1;
        //}

        //private static Point3D[] GetNormals(Point3D p1, Point3D p2, Point3D p3)
        //{
        //    Point3D[] normals = new Point3D[3];
        //    normals[0] = GetNormal(p1, p2, p3);
        //    normals[1] = GetNormal(p2, p3, p1);
        //    normals[2] = GetNormal(p3, p1, p2);
        //    return normals;
        //}

        //private static bool IsOverlap(float[] projection1, float[] projection2)
        //{
        //    GetMinMax(projection1, out float min1, out float max1);
        //    GetMinMax(projection2, out float min2, out float max2);
        //    return IsOverlap(min1, max1, min2, max2);
        //}


        //public static bool AreTrianglesIntersecting(Point3D p1, Point3D p2, Point3D p3, Point3D q1, Point3D q2, Point3D q3)
        //{
        //    Point3D pEdge1 = new Point3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        //    Point3D pEdge2 = new Point3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);
        //    Point3D pEdge3 = new Point3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);

        //    Point3D qEdge1 = new Point3D(q2.X - q1.X, q2.Y - q1.Y, q2.Z - q1.Z);
        //    Point3D qEdge2 = new Point3D(q3.X - q1.X, q3.Y - q1.Y, q3.Z - q1.Z);
        //    Point3D qEdge3 = new Point3D(q1.X - q2.X, q1.Y - q2.Y, q1.Z - q2.Z);

        //    Point3D[] pEdges = new Point3D[3] { pEdge1, pEdge2, pEdge3 };
        //    Point3D[] qEdges = new Point3D[3] { qEdge1, qEdge2, qEdge3 };

        //    for (int i = 0; i < pEdges.Length; i++)
        //    {
        //        for (int j = 0; j < qEdges.Length; j++)
        //        {
        //            Point3D cross = CrossProduct(pEdges[i], qEdges[j]);
        //            Point3D difference = new Point3D(p1.X - q1.X, p1.Y - q1.Y, p1.Z - q1.Z);
        //            if (DotProduct(cross, difference) != 0)
        //            {
        //                continue;
        //            }

        //            float[] pProjection = GetProjectionX(p1, p2, p3, cross);
        //            float[] qProjection = GetProjectionX(q1, q2, q3, cross);
        //            if (!IsOverlap(pProjection, qProjection))
        //            {
        //                return false;
        //            }
        //        }
        //    }

        //    return true;
        //}



        //public static bool AreTrianglesIntersecting3(Point3D p1, Point3D p2, Point3D p3, Point3D q1, Point3D q2, Point3D q3)
        //{
        //    Point3D pEdge1 = new Point3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        //    Point3D pEdge2 = new Point3D(p3.X - p1.X, p3.Y - p1.Y, p3.Z - p1.Z);
        //    Point3D pEdge3 = new Point3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);

        //    Point3D qEdge1 = new Point3D(q2.X - q1.X, q2.Y - q1.Y, q2.Z - q1.Z);
        //    Point3D qEdge2 = new Point3D(q3.X - q1.X, q3.Y - q1.Y, q3.Z - q1.Z);
        //    Point3D qEdge3 = new Point3D(q1.X - q2.X, q1.Y - q2.Y, q1.Z - q2.Z);

        //    Point3D[] pEdges = new Point3D[3] { pEdge1, pEdge2, pEdge3 };
        //    Point3D[] qEdges = new Point3D[3] { qEdge1, qEdge2, qEdge3 };

        //    for (int i = 0; i < pEdges.Length; i++)
        //    {
        //        for (int j = 0; j < qEdges.Length; j++)
        //        {
        //            Point3D cross = CrossProduct(pEdges[i], qEdges[j]);

        //            Point3D difference = new Point3D(p1.X - q1.X, p1.Y - q1.Y, p1.Z - q1.Z);
        //            if (DotProduct(cross, difference) != 0)
        //            {
        //                continue;
        //            }

        //            float[] pProjection = GetProjectionX(p1, p2, p3, cross);
        //            float[] qProjection = GetProjectionX(q1, q2, q3, cross);
        //            if (!IsOverlap(pProjection, qProjection))
        //            {
        //                return false;
        //            }
        //        }
        //    }

        //    return true;
        //}

        //public static bool AreTrianglesIntersecting2(Point3D p1, Point3D p2, Point3D p3,
        //                             Point3D q1, Point3D q2, Point3D q3)
        //{
        //    Point3D[] pNormals = GetNormals(p1, p2, p3);
        //    Point3D[] qNormals = GetNormals(q1, q2, q3);

        //    for (int i = 0; i < pNormals.Length; i++)
        //    {
        //        float[] pProjection = GetProjectionX(p1, p2, p3, pNormals[i]);
        //        float[] qProjection = GetProjectionX(q1, q2, q3, pNormals[i]);
        //        if (!IsOverlap(pProjection, qProjection))
        //        {
        //            return false;
        //        }
        //    }

        //    for (int i = 0; i < qNormals.Length; i++)
        //    {
        //        float[] pProjection = GetProjectionX(p1, p2, p3, qNormals[i]);
        //        float[] qProjection = GetProjectionX(q1, q2, q3, qNormals[i]);
        //        if (!IsOverlap(pProjection, qProjection))
        //        {
        //            return false;
        //        }
        //    }

        //    Point3D pEdge1 = new Point3D(p2.X - p1.X, p2.Y - p1.Y, p2.Z - p1.Z);
        //    Point3D pEdge2 = new Point3D(p3.X - p2.X, p3.Y - p2.Y, p3.Z - p2.Z);
        //    Point3D pEdge3 = new Point3D(p1.X - p3.X, p1.Y - p3.Y, p1.Z - p3.Z);

        //    Point3D qEdge1 = new Point3D(q2.X - q1.X, q2.Y - q1.Y, q2.Z - q1.Z);
        //    Point3D qEdge2 = new Point3D(q3.X - q2.X, q3.Y - q2.Y, q3.Z - q2.Z);
        //    Point3D qEdge3 = new Point3D(q1.X - q3.X, q1.Y - q3.Y, q1.Z - q3.Z);

        //    Point3D[] pEdges = new Point3D[3] { pEdge1, pEdge2, pEdge3 };
        //    Point3D[] qEdges = new Point3D[3] { qEdge1, qEdge2, qEdge3 };

        //    Point3D pNormal = GetNormal(p1, p2, p3);
        //    Point3D qNormal = GetNormal(q1, q2, q3);
        //    Point3D difference = new Point3D(p1.X - q1.X, p1.Y - q1.Y, p1.Z - q1.Z);

        //    for (int i = 0; i < pEdges.Length; i++)
        //    {
        //        for (int j = 0; j < qEdges.Length; j++)
        //        {
        //            Point3D cross = CrossProduct(pEdges[i], qEdges[j]);
        //            if (DotProduct(cross, difference) != 0)
        //            {
        //                continue;
        //            }

        //            float[] pProjection = GetProjectionX(p1, p2, p3, cross);
        //            float[] qProjection = GetProjectionX(q1, q2, q3, cross);
        //            if (!IsOverlap(pProjection, qProjection))
        //            {
        //                return false;
        //            }
        //        }
        //    }

        //    return true;
        //}
        ////public static bool AreTrianglesIntersecting22(Point3D p1, Point3D p2, Point3D p3,
        ////                           Point3D q1, Point3D q2, Point3D q3)
        ////{
        ////    Triangle t1 = new Triangle(p1, p2, p3);
        ////    Triangle t2 = new Triangle(q1, q2, q3);
        ////    return t1.Intersects(t2);
        ////}


        //public static bool MeshCollision(Point3D[] vector1, int[] indx1, Point3D[] vector2, int[] indx2)
        //{
        //    for (int i = 0; i < indx1.Length; i += 3)
        //    {
        //        Point3D p1 = vector1[indx1[i]];
        //        Point3D p2 = vector1[indx1[i + 1]];
        //        Point3D p3 = vector1[indx1[i + 2]];
        //        for (int j = 0; j < indx2.Length; j += 3)
        //        {

        //            Point3D q1 = vector2[indx2[j]];
        //            Point3D q2 = vector2[indx2[j + 1]];
        //            Point3D q3 = vector2[indx2[j + 2]];

        //            if (AreTrianglesIntersecting2(p1, p2, p3, q1, q2, q3))
        //            {
        //                return true;
        //            }
        //        }
        //    }

        //    return false;
        //}





    }
}
