using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace TomekDexValheimMod
{
    [BepInPlugin("TomekDexValheimMod.NoFlyingRocks", "NoFlyingRocks", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class NoFlyingRocks : BaseUnityPlugin
    {
        public void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            InvokeRepeating("DestroyOnTime", 1f, 1f);
            InvokeRepeating("Process", 1f, 1f);
        }

        public void DestroyOnTime()
        {
            NoFlyingRocksProcess.DestroyOnTime();
        }

        public void Process()
        {
            NoFlyingRocksProcess.Process();
        }
    }
}
