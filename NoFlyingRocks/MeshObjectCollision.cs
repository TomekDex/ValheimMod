using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TomekDexValheimMod
{
    internal class MeshObjectCollision
    {
        private static readonly Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cache = new Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();
        private static readonly Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cache2 = new Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();
        private static readonly Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cache3 = new Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();
        private static readonly Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cache4 = new Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();

        public static bool Detection(MeshFilter meshFilter1, MeshFilter meshFilter2)
        {
            MeshCollider meshCollider1 = meshFilter1.GetComponent<MeshCollider>();
            MeshCollider meshCollider2 = meshFilter2.GetComponent<MeshCollider>();
            return Detection(meshFilter1.sharedMesh, meshCollider1, meshCollider2);
        }

        public static bool Detection(MeshFilter meshFilter1, MeshCollider meshCollider2)
        {
            MeshCollider meshCollider1 = meshFilter1.GetComponent<MeshCollider>();
            return Detection(meshFilter1.sharedMesh, meshCollider1, meshCollider2);
        }
        public static bool Detection(Mesh sharedMesh, MeshCollider meshCollider1, MeshFilter meshFilter2)
        {
            MeshCollider meshCollider2 = meshFilter2.GetComponent<MeshCollider>();
            return Detection(sharedMesh, meshCollider1, meshCollider2);
        }

        private static bool Detection1(Mesh sharedMesh, MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey = (meshCollider1, meshCollider2);
            if (cache.TryGetValue(cacheKey, out bool result))
                return result;
            int[] indices = sharedMesh.GetIndices(0);
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 vertex0 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i]]);
                Vector3 vertex1 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i + 1]]);
                Vector3 vertex2 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i + 2]]);
                if (meshCollider2.Raycast(new Ray(vertex0, (vertex1 - vertex0).normalized), out _, (vertex1 - vertex0).magnitude) ||
                    meshCollider2.Raycast(new Ray(vertex1, (vertex2 - vertex1).normalized), out _, (vertex2 - vertex1).magnitude) ||
                    meshCollider2.Raycast(new Ray(vertex2, (vertex0 - vertex2).normalized), out _, (vertex0 - vertex2).magnitude))
                {

                    return cache[cacheKey] = true;
                }
            }
            return cache[cacheKey] = false;
        }

        private static bool Detection(Mesh sharedMesh, MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            //System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            //bool r1 = Detection2(meshCollider1, meshCollider2) || Detection2(meshCollider2, meshCollider1);
            //sw.Stop();
            //System.Diagnostics.Stopwatch sw1 = System.Diagnostics.Stopwatch.StartNew();
            //var m1 = (MeshData)meshCollider1;
            //var m2 = (MeshData)meshCollider2;
            //bool r2 =   Point3D.MeshCollision(m1.vertices.Select(a=> new Point3D (a.x,a.y,a.z)).ToArray(), m1.indices, m2.vertices.Select(a => new Point3D(a.x, a.y, a.z)).ToArray(), m2.indices);
            //bool r2 = Detection4(meshCollider1, meshCollider2);
            bool r1 = DetectionSAT(meshCollider1, meshCollider2);
            bool r2 = DetectionSAT2(meshCollider1, meshCollider2);
            bool r3 = DetectionGJK(meshCollider1, meshCollider2);
            bool r4 = DetectionGJK2(meshCollider1, meshCollider2);
            //sw1.Stop();
            Debug.Log($"{r1 == r2} {r3 == r4}");
            return r3;
        }
        public static PropertyInfo meshColliderSharedMesh = typeof(MeshCollider).GetProperty("sharedMesh");
        private static bool Detection2(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey = (meshCollider1, meshCollider2);
            if (cache3.TryGetValue(cacheKey, out bool result))
                return result;
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            int[] indices = sharedMesh.GetIndices(0);
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 vertex0 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i]]);
                Vector3 vertex1 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i + 1]]);
                Vector3 vertex2 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i + 2]]);
                if (meshCollider2.Raycast(new Ray(vertex0, (vertex1 - vertex0).normalized), out _, (vertex1 - vertex0).magnitude) ||
                    meshCollider2.Raycast(new Ray(vertex1, (vertex2 - vertex1).normalized), out _, (vertex2 - vertex1).magnitude) ||
                    meshCollider2.Raycast(new Ray(vertex2, (vertex0 - vertex2).normalized), out _, (vertex0 - vertex2).magnitude))
                {

                    return cache3[cacheKey] = true;
                }
            }
            return cache3[cacheKey] = false;
        }

        private static bool Detection22(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            int[] indices = sharedMesh.GetIndices(0);

            //var a = (MeshCollider)(MeshData)meshCollider1;
            ////a.GetType().GetProperty("bounds").SetValue(meshCollider1, meshCollider1.bounds, BindingFlags.GetProperty, null, null, CultureInfo.CurrentCulture);
            //var b = (Mesh)meshColliderSharedMesh.GetValue(a);
            //Debug.Log("---------------");
            //Log(sharedMesh);
            //Debug.Log("++++++++++");
            //Log(b);


            //Debug.Log("meshCollider1");
            //Log(a);
            //Debug.Log("++++++++++");
            //Log(meshCollider1);
            //Debug.Log("transform");
            //Log(a.transform);
            //Debug.Log("++++++++++");
            //Log(meshCollider1.transform);
            //Debug.Log("bounds");
            //Log(a.bounds);
            //Debug.Log("++++++++++");
            //Log(meshCollider1.bounds);
            //Debug.Log("---------------");
            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 vertex0 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i]]);
                Vector3 vertex1 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i + 1]]);
                Vector3 vertex2 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i + 2]]);
                if (meshCollider2.Raycast(new Ray(vertex0, (vertex1 - vertex0).normalized), out _, (vertex1 - vertex0).magnitude) ||
                    meshCollider2.Raycast(new Ray(vertex1, (vertex2 - vertex1).normalized), out _, (vertex2 - vertex1).magnitude) ||
                    meshCollider2.Raycast(new Ray(vertex2, (vertex0 - vertex2).normalized), out _, (vertex0 - vertex2).magnitude))
                {

                    return true;
                }
            }
            return false;
        }

        //private static bool Detection3(MeshCollider meshCollider1, MeshCollider meshCollider2)
        //{
        //    (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey = (meshCollider1, meshCollider2);
        //    if (cache.TryGetValue(cacheKey, out bool result))
        //        return result;
        //    Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
        //    Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider2);
        //    int[] indices = sharedMesh.GetIndices(0);
        //    for (int i = 0; i < indices.Length; i += 3)
        //    {
        //        Vector3 vertex0 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i]]);
        //        Vector3 vertex1 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i + 1]]);
        //        Vector3 vertex2 = meshCollider1.transform.TransformPoint(sharedMesh.vertices[indices[i + 2]]);
        //        if (Point.MyRaycast(vertex0, vertex1, sharedMesh2.vertices.Select(a => meshCollider2.transform.TransformPoint(a)).ToArray()) ||
        //            Point.MyRaycast(vertex1, vertex2, sharedMesh2.vertices.Select(a => meshCollider2.transform.TransformPoint(a)).ToArray()) ||
        //            Point.MyRaycast(vertex2, vertex0, sharedMesh2.vertices.Select(a => meshCollider2.transform.TransformPoint(a)).ToArray()))
        //        {

        //            return cache[cacheKey] = true;
        //        }
        //    }
        //    return cache[cacheKey] = false;
        //}


        public static bool DetectionSAT(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey = (meshCollider1, meshCollider2);
            if (cache3.TryGetValue(cacheKey, out bool result))
                return result;
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider2);
            return cache3[cacheKey] = SATCollisionDetection.CheckCollision(sharedMesh.vertices.Select(a => meshCollider1.transform.TransformPoint(a)).ToArray(), sharedMesh.GetIndices(0), sharedMesh2.vertices.Select(a => meshCollider2.transform.TransformPoint(a)).ToArray(), sharedMesh2.GetIndices(0)); ;

        }

        private static bool DetectionSAT2(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey = (meshCollider1, meshCollider2);
            if (cache.TryGetValue(cacheKey, out bool result))
                return result;
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider2);
            return cache[cacheKey] = SATCollisionDetection2.CheckCollision(sharedMesh.vertices.Select(a => (Point3D)meshCollider1.transform.TransformPoint(a)).ToArray(), sharedMesh.GetIndices(0), sharedMesh2.vertices.Select(a => (Point3D)meshCollider2.transform.TransformPoint(a)).ToArray(), sharedMesh2.GetIndices(0)); ;

        }
        private static bool DetectionGJK(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey = (meshCollider1, meshCollider2);
            if (cache2.TryGetValue(cacheKey, out bool result))
                return result;
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider2);
            return cache2[cacheKey] = GJKCollisionDetection.CheckCollision(sharedMesh.vertices.Select(a => meshCollider1.transform.TransformPoint(a)).ToArray(), sharedMesh.GetIndices(0), meshCollider1.transform.position, sharedMesh2.vertices.Select(a => meshCollider2.transform.TransformPoint(a)).ToArray(), sharedMesh2.GetIndices(0), meshCollider2.transform.position); 

        }
        private static bool DetectionGJK2(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey = (meshCollider1, meshCollider2);
            if (cache4.TryGetValue(cacheKey, out bool result))
                return result;
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider2);
            return cache4[cacheKey] = GJKCollisionDetection2.CheckCollision(sharedMesh.vertices.Select(a => (Point3D)meshCollider1.transform.TransformPoint(a)).ToArray(), sharedMesh.GetIndices(0), meshCollider1.transform.position, sharedMesh2.vertices.Select(a => (Point3D)meshCollider2.transform.TransformPoint(a)).ToArray(), sharedMesh2.GetIndices(0), meshCollider2.transform.position);
        }

        public static void Log(object obj)
        {
            if (obj == null)
                return;

            foreach (PropertyInfo item in obj.GetType().GetProperties())
            {
                object v = item.GetValue(obj);
                if (v != default)
                    Debug.Log($"{item.Name} {v}");
            }
            foreach (FieldInfo item in obj.GetType().GetFields())
            {
                object v = item.GetValue(obj);
                if (v != default)
                    Debug.Log($"{item.Name} {v}");
            }
        }

        //internal static bool Detection(Vector[] vector1, int[] indices1, Vector[] vector2, int[] indices2)
        //{
        //    GameObject tempGO = new GameObject();
        //    Mesh mesh2 = new Mesh();
        //    mesh2.vertices = vector2.Select(a => a.GetVector3()).ToArray();
        //    mesh2.SetIndices(indices2, MeshTopology.Triangles, 0);
        //    mesh2.RecalculateBounds();
        //    mesh2.RecalculateNormals();
        //    MeshCollider meshCollider2 = tempGO.AddComponent<MeshCollider>();
        //    meshCollider2.sharedMesh = mesh2;

        //    for (int i = 0; i < indices1.Length; i += 3)
        //    {
        //        Vector3 vertex0 = vector1[indices1[i]].GetVector3();
        //        Vector3 vertex1 = vector1[indices1[i + 1]].GetVector3();
        //        Vector3 vertex2 = vector1[indices1[i + 2]].GetVector3();
        //        if (meshCollider2.Raycast(new Ray(vertex0, (vertex1 - vertex0).normalized), out RaycastHit val, (vertex1 - vertex0).magnitude) ||
        //            meshCollider2.Raycast(new Ray(vertex1, (vertex2 - vertex1).normalized), out val, (vertex2 - vertex1).magnitude) ||
        //            meshCollider2.Raycast(new Ray(vertex2, (vertex0 - vertex2).normalized), out val, (vertex0 - vertex2).magnitude))
        //        {
        //            ////Debug.Log(string.Join(" ", vector2.Select(a=> $"({a.x},{a.y},{a.z})")));
        //            //Debug.Log($"-----------------------------------");
        //            //Debug.Log($"vertex0 {vertex0}");
        //            //Debug.Log($"vertex1 {vertex1}");
        //            //Debug.Log($"vertex2 {vertex2}");
        //            //Debug.Log($"(vertex1 - vertex0).normalized {(vertex1 - vertex0).normalized}");
        //            //Debug.Log($"(vertex2 - vertex1).normalized) {(vertex2 - vertex1).normalized}");
        //            //Debug.Log($"(vertex0 - vertex2).normalized {(vertex0 - vertex2).normalized}");
        //            //Debug.Log($"val {val}");
        //            //Debug.Log($"(vertex1 - vertex0).magnitude {(vertex1 - vertex0).magnitude}");
        //            //Debug.Log($"(vertex2 - vertex1).magnitude {(vertex2 - vertex1).magnitude}");
        //            //Debug.Log($"(vertex0 - vertex2).magnitude {(vertex0 - vertex2).magnitude}");
        //            //Debug.Log($"-----------------------------------");
        //            //throw new System.Exception();
        //            GameObject.Destroy(tempGO);
        //            return true;
        //        }
        //    }

        //    GameObject.Destroy(tempGO);
        //    return false;

        //}
    }
}
