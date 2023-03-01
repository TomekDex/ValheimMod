using HarmonyLib;

namespace TomekDexValheimMod
{
    [HarmonyPatch]
    public class Controller
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock5), "Start")]
        public static void PostfixMineRock5Start(MineRock5 __instance)
        {
            __instance.gameObject.AddComponent<MineRock5Extension>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MineRock5), "CheckSupport")]
        public static void PostfixMineRock5CheckSupport(MineRock5 __instance)
        {
            NoFlyingRocksProcess.AddToPriorityQueue(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Heightmap), "Poke")]
        public static void PrefixHeightmapPoke(Heightmap __instance, bool delayed)
        {
            if (!delayed)
                NoFlyingRocksProcess.PokeGroundCollider(__instance.m_collider);
        }
    }
}
