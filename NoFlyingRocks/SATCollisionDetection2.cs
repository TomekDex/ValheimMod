using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace TomekDexValheimMod
{
    public class SATCollisionDetection2
    {
        public static bool CheckCollision(Point3D[] vertices1, int[] indices1, Point3D[] vertices2, int[] indices2)
        {
            List<Point3D> normals = new List<Point3D>();

            for (int i = 0; i < indices1.Length; i += 3)
            {
                Point3D normal = Point3D.Cross(
                    vertices1[indices1[i + 1]] - vertices1[indices1[i]],
                    vertices1[indices1[i + 2]] - vertices1[indices1[i]]
                ).Normalized();

                normals.Add(normal);
            }

            for (int i = 0; i < indices2.Length; i += 3)
            {
                Point3D normal = Point3D.Cross(
                    vertices2[indices2[i + 1]] - vertices2[indices2[i]],
                    vertices2[indices2[i + 2]] - vertices2[indices2[i]]
                ).Normalized();

                normals.Add(normal);
            }

            foreach (Point3D normal in normals)
            {
                float min1 = float.MaxValue;
                float max1 = float.MinValue;

                for (int i = 0; i < indices1.Length; i++)
                {
                    float dot = Point3D.Dot(vertices1[indices1[i]], normal);

                    if (dot < min1) min1 = dot;
                    if (dot > max1) max1 = dot;
                }

                float min2 = float.MaxValue;
                float max2 = float.MinValue;

                for (int i = 0; i < indices2.Length; i++)
                {
                    float dot = Point3D.Dot(vertices2[indices2[i]], normal);

                    if (dot < min2) min2 = dot;
                    if (dot > max2) max2 = dot;
                }

                if (max1 < min2 || max2 < min1) return false;
            }

            return true;
        }

        private static ConcurrentDictionary<(FlattenedDataCollider, FlattenedDataCollider), bool> cache = new ConcurrentDictionary<(FlattenedDataCollider, FlattenedDataCollider), bool>();
        internal static bool CheckCollision(FlattenedDataCollider flattenedDataCollider1, FlattenedDataCollider flattenedDataCollider2)
        {
            var cacheKey1 = (flattenedDataCollider1, flattenedDataCollider2);
            var cacheKey2 = (flattenedDataCollider2, flattenedDataCollider1);
            if (cache.TryGetValue(cacheKey1, out bool result) || cache.TryGetValue(cacheKey2, out result))
                return result;
            result = CheckCollision(flattenedDataCollider1.vertices, flattenedDataCollider1.indices, flattenedDataCollider2.vertices, flattenedDataCollider2.indices);
            cache.TryAdd(cacheKey1, result);
            return result;
        }
    }
}