using HarmonyLib;
using System;
using System.Text;
using TomekDexValheimModHelper;

namespace TomekDexValheimMod
{
    [HarmonyPatch]
    public class ContainerPatch
    {
        private static readonly int sortModeLength = Enum.GetValues(typeof(SortMode)).Length;

        [HarmonyPriority(800)]
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Container), "Awake")]
        public static void PostfixContainerAwake(Container __instance)
        {
            if (__instance.m_nview.GetZDO() != null)
            {
                __instance.Load();
                ContainerQuickAccess.AddContainer(__instance);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Container), "GetHoverText")]
        public static void PostfixContainerGetHoverText(Container __instance, ref string __result)
        {
            ContainerExtension.LastUse = __instance.GetComponent<ContainerExtension>();
            if (ContainerQuickAccessConfig.HideText || ContainerExtension.LastUse == null)
                return;
            ContainerExtension containerExtension = __instance.GetComponent<ContainerExtension>();
            StringBuilder sb = new StringBuilder();
            sb.Append("\n");
            if (!ContainerQuickAccessConfig.BlockChange)
                sb.Append($"[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] ");
            sb.Append(containerExtension.Mode.ToString());
            sb.Append("\n");
            if (!ContainerQuickAccessConfig.BlockChange)
                sb.Append($"[<color=yellow><b>1-8</b></color>] ");
            int count = 0;
            foreach (int item in containerExtension.ItemsFilter)
            {
                count++;
                sb.Append(ObjectDB.instance.GetItemPrefab(item)?.GetComponent<ItemDrop>().m_itemData.m_shared.m_name);
                sb.Append(", ");
                if (count % 4 == 0)
                    sb.Append("\n");
            }
            __result += Localization.instance.Localize(sb.ToString().TrimEnd('\n', ' ', ','));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Container), "Interact")]
        public static bool PrefixContainerInteract(Container __instance, ref bool __result, Humanoid character, bool hold, bool alt)
        {
            if (!hold && ContainerQuickAccessConfig.UseAutoClear)
            {
                ContainerExtension containerExtension = __instance.GetComponent<ContainerExtension>();
                if (containerExtension != null)
                    ContainerQuickAccess.Clear(__instance, __instance.GetComponent<ContainerExtension>());
            }
            if (ContainerQuickAccessConfig.BlockChange)
                return true;
            if (alt)
            {
                ContainerExtension containerExtension = __instance.GetComponent<ContainerExtension>();
                containerExtension.Mode = (SortMode)((int)(containerExtension.Mode + 1) % sortModeLength);
                containerExtension.Save();
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Container), "UseItem")]
        public static void PostfixContainerUseItem(Container __instance, ref bool __result, ItemDrop.ItemData item)
        {
            if (ContainerQuickAccessConfig.BlockChange)
                return;
            ContainerExtension.LastUse = __instance.GetComponent<ContainerExtension>();
            ContainerExtension containerExtension = __instance.GetComponent<ContainerExtension>();
            ItemDrop itemDrop = ItemsHelper.GetItemDropBySharedNameOrName(item.m_shared.m_name);
            if (ObjectDB.instance.GetItemPrefab(itemDrop.name.GetStableHashCode()) != null)
            {
                if (!containerExtension.ItemsFilter.Add(itemDrop.name.GetStableHashCode()))
                    containerExtension.ItemsFilter.Remove(itemDrop.name.GetStableHashCode());
                containerExtension.Save();
            }
            __result = true;
        }
    }
}
