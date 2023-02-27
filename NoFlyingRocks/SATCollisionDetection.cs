using System.Collections.Generic;
using UnityEngine;

namespace TomekDexValheimMod
{
    public class SATCollisionDetection
    {
        public static bool CheckCollision(Vector3[] vertices1, int[] indices1, Vector3[] vertices2, int[] indices2)
        {
            List<Vector3> normals = new List<Vector3>();

            for (int i = 0; i < indices1.Length; i += 3)
            {
                Vector3 normal = Vector3.Cross(
                    vertices1[indices1[i + 1]] - vertices1[indices1[i]],
                    vertices1[indices1[i + 2]] - vertices1[indices1[i]]
                ).normalized;

                normals.Add(normal);
            }

            for (int i = 0; i < indices2.Length; i += 3)
            {
                Vector3 normal = Vector3.Cross(
                    vertices2[indices2[i + 1]] - vertices2[indices2[i]],
                    vertices2[indices2[i + 2]] - vertices2[indices2[i]]
                ).normalized;

                normals.Add(normal);
            }

            foreach (Vector3 normal in normals)
            {
                float min1 = float.MaxValue;
                float max1 = float.MinValue;

                for (int i = 0; i < indices1.Length; i++)
                {
                    float dot = Vector3.Dot(vertices1[indices1[i]], normal);

                    if (dot < min1) min1 = dot;
                    if (dot > max1) max1 = dot;
                }

                float min2 = float.MaxValue;
                float max2 = float.MinValue;

                for (int i = 0; i < indices2.Length; i++)
                {
                    float dot = Vector3.Dot(vertices2[indices2[i]], normal);

                    if (dot < min2) min2 = dot;
                    if (dot > max2) max2 = dot;
                }

                if (max1 < min2 || max2 < min1) return false;
            }

            return true;
        }
    }
}