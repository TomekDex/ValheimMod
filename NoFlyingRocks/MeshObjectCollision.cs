using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TomekDexValheimMod
{
    internal class MeshObjectCollision
    {
        public static PropertyInfo meshColliderSharedMesh = typeof(MeshCollider).GetProperty("sharedMesh");
        private static readonly Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cache = new Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();
        private static readonly Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cache2 = new Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();
        private static readonly Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cache3 = new Dictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();

        public static bool DetectionRaycast(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            return DetectionSingle(meshCollider1, meshCollider2) || DetectionSingle(meshCollider2, meshCollider1);
        }

        private static bool DetectionSingle(MeshCollider meshCollider1, MeshCollider meshCollider2)
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
                    return cache3[cacheKey] = true;
            }
            return cache3[cacheKey] = false;
        }

        public static bool DetectionSAT(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey1 = (meshCollider1, meshCollider2);
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey2 = (meshCollider1, meshCollider2);
            if (cache3.TryGetValue(cacheKey1, out bool result))
                return result;
            if (cache3.TryGetValue(cacheKey2, out result))
                return result;
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider2);
            return cache3[cacheKey1] = SATCollisionDetection.CheckCollision(sharedMesh.vertices.Select(a => meshCollider1.transform.TransformPoint(a)).ToArray(), sharedMesh.GetIndices(0), sharedMesh2.vertices.Select(a => meshCollider2.transform.TransformPoint(a)).ToArray(), sharedMesh2.GetIndices(0)); ;

        }

        public static bool DetectionGJK(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey1 = (meshCollider1, meshCollider2);
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey2 = (meshCollider1, meshCollider2);
            if (cache2.TryGetValue(cacheKey1, out bool result))
                return result;
            if (cache2.TryGetValue(cacheKey2, out result))
                return result;
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider2);
            return cache2[cacheKey1] = GJKCollisionDetection.CheckCollision(sharedMesh.vertices.Select(a => meshCollider1.transform.TransformPoint(a)).ToArray(), sharedMesh.GetIndices(0), meshCollider1.transform.position, sharedMesh2.vertices.Select(a => meshCollider2.transform.TransformPoint(a)).ToArray(), sharedMesh2.GetIndices(0), meshCollider2.transform.position);

        }
    }
}
