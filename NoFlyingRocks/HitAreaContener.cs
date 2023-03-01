using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace TomekDexValheimMod
{
    public class HitAreaContener
    {
        public MineRock5.HitArea HitArea { get; }
        public MineRock5 MineRock5 { get; }
        public int Id { get; }
        public HashSet<Collider> CollidersAdjacent { get; internal set; }
        public ConcurrentDictionary<Collider, object> GroundColldiders { get; internal set; }

        public HitAreaContener(MineRock5.HitArea hitArea, MineRock5 mineRock5)
        {
            HitArea = hitArea;
            MineRock5 = mineRock5;
        }
    }
}
