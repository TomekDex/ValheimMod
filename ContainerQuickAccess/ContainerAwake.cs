using HarmonyLib;

namespace TomekDexValheimMod
{
    [HarmonyPatch]
    public class ContainerAwake
    {
        [HarmonyPriority(800)]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Container), "Awake")]
        public static void PostfixContainerAwake(Container __instance)
        {
            ContainerQuickAccess.AddContainer(__instance);
        }
    }
}
