//using System;
//using System.Linq;
//using System.Reflection;
//using UnityEngine;
//using UnityEngine.Rendering;

//namespace TomekDexValheimMod
//{
//    [Serializable]
//    public struct MeshData
//    {
//         public static PropertyInfo meshColliderSharedMesh = typeof(MeshCollider).GetProperty("sharedMesh");
//        public Vector[] vertices;
//        public int[] indices;
//        public Vector center;
//        public float convexRadius;
//        public Transform transform;
//        public static implicit operator MeshData(MeshCollider meshCollider)
//        {
//            Mesh sharedMesh = (Mesh)meshColliderSharedMesh.GetValue(meshCollider);

//            MeshData data = new MeshData();
//            data.vertices = sharedMesh.vertices.Select(a => (Vector)a).ToArray();
//            //data.normals = sharedMesh.normals;
//            data.indices = sharedMesh.GetIndices(0);
//            data.center = meshCollider.transform.position;
//            data.transform = meshCollider.transform;
//            return data;
//        }

//        public static implicit operator MeshCollider(MeshData v)
//        {
//            GameObject tempGO = new GameObject();
//            Mesh mesh2 = new Mesh();
//            mesh2.vertices = v.vertices.Select(a => (Vector3)a).ToArray();
//            mesh2.SetIndices(v.indices, MeshTopology.Triangles, 0);
//            mesh2.RecalculateBounds();
//            mesh2.RecalculateNormals();
//            MeshCollider meshCollider = tempGO.AddComponent<MeshCollider>();
//            meshCollider.sharedMesh = mesh2;
//            meshCollider.transform.position = v.center;
//            //meshCollider.transform.localPosition =  v.center;
//            //meshCollider.transform.localScale = v.transform.localScale;
//            //meshCollider.transform.localRotation = v.transform.localRotation;
//            //meshCollider.transform.rotation = v.transform.rotation;
//            return meshCollider;
//        }
//    }

//    [Serializable]
//    public struct Vector
//    {
//        public float x;
//        public float y;
//        public float z;

//        public Vector(float x, float y, float z)
//        {
//            this.x = x;
//            this.y = y;
//            this.z = z;
//        }

//        public static implicit operator Vector3(Vector v)
//        {
//            return new Vector3(v.x, v.y, v.z);
//        }

//        public static implicit operator Vector(Vector3 v)
//        {
//            return new Vector(v.x, v.y, v.z);
//        }
//    }
//}
