using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TomekDexValheimMod
{
    internal class MeshObjectCollision
    {
        public static PropertyInfo meshColliderSharedMesh = typeof(MeshCollider).GetProperty("sharedMesh");
        private static readonly ConcurrentDictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cacheRaycast = new ConcurrentDictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();
        private static readonly ConcurrentDictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cacheGJK = new ConcurrentDictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();
        private static readonly ConcurrentDictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool> cacheSAT = new ConcurrentDictionary<(MeshCollider meshCollider1, MeshCollider meshCollider2), bool>();

        public static bool DetectionRaycast(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            return DetectionSingle(meshCollider1, meshCollider2) || DetectionSingle(meshCollider2, meshCollider1);
        }

        private static bool DetectionSingle(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey = (meshCollider1, meshCollider2);
            if (cacheRaycast.TryGetValue(cacheKey, out bool result))
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
                    cacheRaycast.TryAdd(cacheKey, true);
                    return true;
                }
            }
            cacheRaycast.TryAdd(cacheKey, false);
            return false;
        }

        public static bool DetectionSAT(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey1 = (meshCollider1, meshCollider2);
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey2 = (meshCollider1, meshCollider2);
            if (cacheSAT.TryGetValue(cacheKey1, out bool result))
                return result;
            if (cacheSAT.TryGetValue(cacheKey2, out result))
                return result;
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider2);
            result = SATCollisionDetection.CheckCollision(sharedMesh.vertices.Select(a => meshCollider1.transform.TransformPoint(a)).ToArray(), sharedMesh.GetIndices(0), sharedMesh2.vertices.Select(a => meshCollider2.transform.TransformPoint(a)).ToArray(), sharedMesh2.GetIndices(0)); ;
            cacheSAT.TryAdd(cacheKey1, result);
            return result;
        }

        public static bool DetectionGJK(MeshCollider meshCollider1, MeshCollider meshCollider2)
        {
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey1 = (meshCollider1, meshCollider2);
            (MeshCollider meshCollider1, MeshCollider meshCollider2) cacheKey2 = (meshCollider1, meshCollider2);
            if (cacheGJK.TryGetValue(cacheKey1, out bool result))
                return result;
            if (cacheGJK.TryGetValue(cacheKey2, out result))
                return result;
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider1);
            Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider2);
            result = GJKCollisionDetection.CheckCollision(sharedMesh.vertices.Select(a => meshCollider1.transform.TransformPoint(a)).ToArray(), sharedMesh.GetIndices(0), meshCollider1.transform.position, sharedMesh2.vertices.Select(a => meshCollider2.transform.TransformPoint(a)).ToArray(), sharedMesh2.GetIndices(0), meshCollider2.transform.position);
            cacheGJK.TryAdd(cacheKey1, result);
            return result;
        }

        internal static bool InGroundRaycast(MeshCollider meshGround, MeshCollider meshCollider)
        {
            Mesh sharedMesh2 = (Mesh)meshColliderSharedMesh.GetValue(meshCollider);
            foreach (Vector3 vector in sharedMesh2.vertices.Distinct().Select(a=> meshCollider.transform.TransformPoint(a)))
            {
                Ray ray = new Ray(vector + Vector3.up * 100f, Vector3.down);
                if (meshGround.Raycast(ray, out RaycastHit hitInfo, 300f))
                {
                    if (hitInfo.point.y + 0.1 >= vector.y)
                        return true;
                }
            }
            return false;
        }

        public static void ClearCache()
        {
            cacheRaycast.Clear();
            cacheGJK.Clear();
            cacheSAT.Clear();
        }
    }
}
