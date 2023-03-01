using System.IO;
using TomekDexValheimMod;


namespace NoFlyingRocksCalculator
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            System.Console.WriteLine($"da");
            string path = args[0];

            FlattenedDataMineRock5 fdm = FlattenedDataMineRock5.ReadFlattenedDataMineRock5FromFile(path);
            fdm.SATCollisionDetectionClear();
            File.Delete(path);
            SATCollisionDetectionClear(fdm);
            FlattenedDataMineRock5.SaveFlattenedDataMineRock5ToFile(fdm, path);
            System.Console.WriteLine($"posralo");

        }

        private static void SATCollisionDetectionClear(FlattenedDataMineRock5 fdm)
        {
            for (int i = 0; i < fdm.colliders.Length; i++)
            {
                FlattenedDataCollider collider = fdm.colliders[i];
                FlattenedDataCollider[] collidersAdjacent = fdm.collidersAdjacent[i];
                for (int j = 0; j < collidersAdjacent.Length; j++)
                {
                    collidersAdjacent[j].haveCollision = SATCollisionDetection2.CheckCollision(collider, collidersAdjacent[j]);
                    collidersAdjacent[j].indices = null;
                    collidersAdjacent[j].vertices = null;
                }
                FlattenedDataCollider[] collidersGround = fdm.collidersGround[i];
                for (int j = 0; j < collidersGround.Length; j++)
                {
                    collidersGround[j].haveCollision = SATCollisionDetection2.CheckCollision(collider, collidersGround[j]);
                    collidersGround[j].indices = null;
                    collidersGround[j].vertices = null;
                }
            }
        }
    }
}
