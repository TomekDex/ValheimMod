using System;
using System.Collections.Concurrent;
using System.Linq;
using UnityEngine;

namespace TomekDexValheimMod
{
    [Serializable]
    public struct FlattenedDataMineRock5
    {
        public int id;
        public FlattenedDataCollider[] coliders;
        public FlattenedDataCollider[][] colidersStick;
        public FlattenedDataCollider[][] colidersGround;
        private static int nextID;

        public FlattenedDataMineRock5(HitAreaContener[] mineRock5Hits, ConcurrentDictionary<Collider, HitAreaContener> hitAreaContenersAll)
        {
            id = nextID++;
            colidersStick = new FlattenedDataCollider[mineRock5Hits.Length][];
            colidersGround = new FlattenedDataCollider[mineRock5Hits.Length][];
            coliders = new FlattenedDataCollider[mineRock5Hits.Length];
            for (int i = 0; i < mineRock5Hits.Length; i++)
            {
                HitAreaContener hit = mineRock5Hits[i];
                coliders[i] = hit.FlattenedDataCollider;
                Collider[] colliders = hit.Colliders.ToArray();
                colidersStick[i] = new FlattenedDataCollider[colliders.Length];
                colidersGround[i] = hit.GroundFlattenedDataCollider.ToArray();
                for (int j = 0; j < colliders.Length; j++)
                {
                    if (hitAreaContenersAll.TryGetValue(colliders[j], out HitAreaContener hited))
                        if (hited.HitArea.m_health > 0)
                            colidersStick[i][j] = hited.FlattenedDataCollider;
                }
            }
        }

        public FlattenedDataMineRock5(int id, FlattenedDataCollider[] coliders, FlattenedDataCollider[][] colidersStick, FlattenedDataCollider[][] colidersGround)
        {
            this.id = id;
            this.coliders = coliders;
            this.colidersStick = colidersStick;
            this.colidersGround = colidersGround;
        }

        public long GetCapacity()
        {
            long colliderCapacity = sizeof(int) + sizeof(float) * 3 * 2 + sizeof(int) * 3 + sizeof(float) * 3;

            long capacity = sizeof(int) * 4 + colliderCapacity * coliders.Length;
            for (int i = 0; i < coliders.Length; i++)
            {
                capacity += colidersStick[i].Length * colliderCapacity;
                capacity += colidersGround[i].Length * colliderCapacity;
            }
            capacity += coliders.Length * sizeof(int);
            return capacity;
        }
    }
}
