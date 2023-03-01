using UnityEngine;

namespace TomekDexValheimMod
{
    public class MineRock5Extension : MonoBehaviour
    {
        public MineRock5 mineRock5;

        public void Awake()
        {
            mineRock5 = GetComponent<MineRock5>();
            NoFlyingRocksProcess.AddMineRock5(mineRock5);
        }

        public void OnDestroy()
        {
            NoFlyingRocksProcess.RemoveMineRock5(mineRock5);
        }
    }
}
