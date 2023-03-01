using System;

namespace TomekDexValheimMod
{
    [Serializable]
    public struct FlattenedDataCollider
    {
        public Point3D[] vertices;
        public int[] indices;
        public Point3D center;
        public int id;
        public bool haveCollision;

        public FlattenedDataCollider(Point3D[] vertices, int[] indices, Point3D center, int id)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.center = center;
            this.id = id;
            haveCollision = false;
        }

        public FlattenedDataCollider(Point3D[] vertices, int[] indices, Point3D center, int id, bool haveCollision)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.center = center;
            this.id = id;
            this.haveCollision = haveCollision;
        }
    }
}
