using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TomekDexValheimMod
{
    public class GJKCollisionDetection
    {
        public static Vector3 CalculateMeshCenter(Vector3[] vertices1, int[] indices1)
        {
            Vector3 center = Vector3.zero;
            float totalArea = 0f;

            for (int i = 0; i < indices1.Length; i += 3)
            {
                Vector3 v0 = vertices1[indices1[i]];
                Vector3 v1 = vertices1[indices1[i + 1]];
                Vector3 v2 = vertices1[indices1[i + 2]];

                float area = Vector3.Cross(v1 - v0, v2 - v0).magnitude;
                center += area * (v0 + v1 + v2) / 3f;
                totalArea += area;
            }

            center /= totalArea;
            return center;
        }

        public static bool CheckCollision(Vector3[] vertices1, int[] indices1, Vector3 center1, Vector3[] vertices2, int[] indices2, Vector3 center2)
        {
            Vector3 d = center1 - center2;
            List<Vector3> simplex = new List<Vector3>();
            simplex.Add(Support(vertices1, indices1, vertices2, indices2, d));

            d = -simplex[0];

            while (true)
            {
                simplex.Add(Support(vertices1, indices1, vertices2, indices2, d));

                if (Vector3.Dot(simplex.Last(), d) < 0)
                {
                    return false;
                }
                else
                {
                    if (DoSimplex(ref simplex, ref d))
                    {
                        return true;
                    }
                }
            }
        }

        private static bool DoSimplex(ref List<Vector3> simplex, ref Vector3 d)
        {
            Vector3 a = simplex.Last();
            Vector3 ao = -a;

            if (simplex.Count == 3)
            {
                Vector3 b = simplex[1];
                Vector3 c = simplex[0];
                Vector3 ab = b - a;
                Vector3 ac = c - a;
                Vector3 abc = Vector3.Cross(ab, ac);

                if (Vector3.Dot(abc, ao) > 0)
                {
                    simplex.Remove(c);
                    d = Vector3.Cross(ab, ao);
                }
                else
                {
                    Vector3 bc = c - b;
                    Vector3 bac = Vector3.Cross(b, bc);

                    if (Vector3.Dot(bac, ao) > 0)
                    {
                        simplex.Remove(a);
                        d = Vector3.Cross(bc, ao);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                Vector3 b = simplex[0];
                Vector3 ab = b - a;
                d = Vector3.Cross(ab, ao);
            }

            return false;
        }

        private static Vector3 Support(Vector3[] vertices1, int[] indices1, Vector3[] vertices2, int[] indices2, Vector3 d)
        {
            return Support(d, vertices1, indices1) - Support(-d, vertices2, indices2);
        }

        public static Vector3 Support(Vector3 d, Vector3[] vertices, int[] indices)
        {
            // Przechowywanie punktu o największym iloczynie skalarnym z wektorem d
            Vector3 supportVertex = vertices[0];
            float maxDot = Vector3.Dot(vertices[0], d);

            // Iteracja przez wszystkie trójkąty
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 v1 = vertices[indices[i]];
                Vector3 v2 = vertices[indices[i + 1]];
                Vector3 v3 = vertices[indices[i + 2]];

                // Obliczenie normalnej trójkąta
                Vector3 triangleNormal = Vector3.Cross(v2 - v1, v3 - v1);

                // Sprawdzenie, czy wektor d jest skierowany w kierunku normalnej trójkąta
                if (Vector3.Dot(d, triangleNormal) > 0)
                {
                    // Wybór wierzchołka o największym iloczynie skalarnym z wektorem d
                    Vector3 triangleSupportVertex = v1;
                    float triangleMaxDot = Vector3.Dot(v1, d);

                    if (Vector3.Dot(v2, d) > triangleMaxDot)
                    {
                        triangleMaxDot = Vector3.Dot(v2, d);
                        triangleSupportVertex = v2;
                    }

                    if (Vector3.Dot(v3, d) > triangleMaxDot)
                    {
                        triangleMaxDot = Vector3.Dot(v3, d);
                        triangleSupportVertex = v3;
                    }

                    // Zapisanie wierzchołka z trójkąta o największym iloczynie skalarnym z wektorem d
                    if (triangleMaxDot > maxDot)
                    {
                        maxDot = triangleMaxDot;
                        supportVertex = triangleSupportVertex;
                    }
                }
            }

            return supportVertex;
        }
    }
}
