using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TomekDexValheimMod
{
    [Serializable]
    public struct FlattenedDataCollider
    {
        public Point3D[] vertices;
        public int[] indices;
        public Point3D center;
        public int id;
        private static readonly PropertyInfo meshColliderSharedMesh = typeof(MeshCollider).GetProperty("sharedMesh");

        public FlattenedDataCollider(Point3D[] vertices, int[] indices, Point3D center, int id)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.center = center;
            this.id = id;
        }

        public FlattenedDataCollider(MeshCollider meshCollider, int id)
        {
            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider);
            vertices = sharedMesh.vertices.Select(a => (Point3D)meshCollider.transform.TransformPoint(a)).ToArray();
            indices = sharedMesh.GetIndices(0);
            center = meshCollider.transform.position;
            this.id = id;
        }
    }
}
