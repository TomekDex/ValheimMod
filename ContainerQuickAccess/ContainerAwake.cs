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
            ZNetView view = __instance.GetComponent<ZNetView>();
            if (view != null)
                ContainerQuickAccess.AddContainer(__instance);
        }
    }
}
