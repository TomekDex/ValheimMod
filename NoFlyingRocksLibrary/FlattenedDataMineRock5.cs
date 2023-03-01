using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TomekDexValheimMod
{
    [Serializable]
    public struct FlattenedDataMineRock5
    {
        public int id;
        public FlattenedDataCollider[] colliders;
        public FlattenedDataCollider[][] collidersAdjacent;
        public FlattenedDataCollider[][] collidersGround;

        public FlattenedDataMineRock5(int id, FlattenedDataCollider[] colliders, FlattenedDataCollider[][] collidersAdjacent, FlattenedDataCollider[][] collidersGround)
        {
            this.id = id;
            this.colliders = colliders;
            this.collidersAdjacent = collidersAdjacent;
            this.collidersGround = collidersGround;
        }

        public static FlattenedDataMineRock5 ReadFlattenedDataMineRock5FromFile(string path)
        {
            FlattenedDataMineRock5 deserializedRockData;
            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                deserializedRockData = (FlattenedDataMineRock5)formatter.Deserialize(fileStream);
            }

            return deserializedRockData;
        }

        public static void SaveFlattenedDataMineRock5ToFile(FlattenedDataMineRock5 flattenedDataMineRock5, string tempFilePath)
        {
            using (FileStream fileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, flattenedDataMineRock5);
            }
        }

        public void SATCollisionDetectionClear()
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                FlattenedDataCollider collider = colliders[i];
                FlattenedDataCollider[] collidersAdj = collidersAdjacent[i];
                for (int j = 0; j < collidersAdj.Length; j++)
                {
                    collidersAdj[j].haveCollision = SATCollisionDetection2.CheckCollision(collider, collidersAdj[j]);
                    collidersAdj[j].indices = null;
                    collidersAdj[j].vertices = null;
                }
                FlattenedDataCollider[] collidersGro = collidersGround[i];
                for (int j = 0; j < collidersGro.Length; j++)
                {
                    collidersGro[j].haveCollision = SATCollisionDetection2.CheckCollision(collider, collidersGro[j]);
                    collidersGro[j].indices = null;
                    collidersGro[j].vertices = null;
                }
            }
        }
    }
}
