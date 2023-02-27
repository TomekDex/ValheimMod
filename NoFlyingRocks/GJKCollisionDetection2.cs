using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TomekDexValheimMod
{
    public class GJKCollisionDetection2
    {
        public static Point3D CalculateMeshCenter(Point3D[] vertices1, int[] indices1)
        {
            Point3D center = Point3D.Zero;
            float totalArea = 0f;

            for (int i = 0; i < indices1.Length; i += 3)
            {
                Point3D v0 = vertices1[indices1[i]];
                Point3D v1 = vertices1[indices1[i + 1]];
                Point3D v2 = vertices1[indices1[i + 2]];

                float area = Point3D.Cross(v1 - v0, v2 - v0).Magnitude();
                center += (v0 + v1 + v2) * area / 3f;
                totalArea += area;
            }

            center /= totalArea;
            return center;
        }

        public static bool CheckCollision(Point3D[] vertices1, int[] indices1, Point3D center1, Point3D[] vertices2, int[] indices2, Point3D center2)
        {
            //Debug.Log($"{center1} {CalculateMeshCenter(vertices1, indices1)} {center2} {CalculateMeshCenter(vertices2, indices2)}");
            //Point3D d = CalculateMeshCenter(vertices1, indices1) - CalculateMeshCenter(vertices2, indices2);

            Point3D d = center1 - center2;
            List<Point3D> simplex = new List<Point3D>();
            simplex.Add(Support(vertices1, indices1, vertices2, indices2, d));

            d = -simplex[0];

            while (true)
            {
                simplex.Add(Support(vertices1, indices1, vertices2, indices2, d));

                if (Point3D.Dot(simplex.Last(), d) < 0)
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

        private static bool DoSimplex(ref List<Point3D> simplex, ref Point3D d)
        {
            Point3D a = simplex.Last();
            Point3D ao = -a;

            if (simplex.Count == 3)
            {
                Point3D b = simplex[1];
                Point3D c = simplex[0];
                Point3D ab = b - a;
                Point3D ac = c - a;
                Point3D abc = Point3D.Cross(ab, ac);

                if (Point3D.Dot(abc, ao) > 0)
                {
                    simplex.Remove(c);
                    d = Point3D.Cross(ab, ao);
                }
                else
                {
                    Point3D bc = c - b;
                    Point3D bac = Point3D.Cross(b, bc);

                    if (Point3D.Dot(bac, ao) > 0)
                    {
                        simplex.Remove(a);
                        d = Point3D.Cross(bc, ao);
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                Point3D b = simplex[0];
                Point3D ab = b - a;
                d = Point3D.Cross(ab, ao);
            }

            return false;
        }

        private static Point3D Support(Point3D[] vertices1, int[] indices1, Point3D[] vertices2, int[] indices2, Point3D d)
        {
            return Support(d, vertices1, indices1) - Support(-d, vertices2, indices2);
        }

        public static Point3D Support(Point3D d, Point3D[] vertices, int[] indices)
        {
            // Przechowywanie punktu o największym iloczynie skalarnym z wektorem d
            Point3D supportVertex = vertices[0];
            float maxDot = Point3D.Dot(vertices[0], d);

            // Iteracja przez wszystkie trójkąty
            for (int i = 0; i < indices.Length; i += 3)
            {
                Point3D v1 = vertices[indices[i]];
                Point3D v2 = vertices[indices[i + 1]];
                Point3D v3 = vertices[indices[i + 2]];

                // Obliczenie normalnej trójkąta
                Point3D triangleNormal = Point3D.Cross(v2 - v1, v3 - v1);

                // Sprawdzenie, czy wektor d jest skierowany w kierunku normalnej trójkąta
                if (Point3D.Dot(d, triangleNormal) > 0)
                {
                    // Wybór wierzchołka o największym iloczynie skalarnym z wektorem d
                    Point3D triangleSupportVertex = v1;
                    float triangleMaxDot = Point3D.Dot(v1, d);

                    if (Point3D.Dot(v2, d) > triangleMaxDot)
                    {
                        triangleMaxDot = Point3D.Dot(v2, d);
                        triangleSupportVertex = v2;
                    }

                    if (Point3D.Dot(v3, d) > triangleMaxDot)
                    {
                        triangleMaxDot = Point3D.Dot(v3, d);
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
