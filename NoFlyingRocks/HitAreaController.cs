using UnityEngine;

namespace TomekDexValheimMod
{
    public class HitAreaController : MonoBehaviour
    {
        public MineRock5 mineRock5;

        public void Awake()
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            mineRock5 = GetComponent<MineRock5>();
            NoFlyingRocksProcess.AddMineRock5(mineRock5);
            sw.Stop();
            Debug.Log($"Awake {sw.Elapsed}");
        }

        public void OnDestroy()
        {
            Debug.Log("OnDestroy");
            NoFlyingRocksProcess.RemoveMineRock5(mineRock5);
        }
    }
}
