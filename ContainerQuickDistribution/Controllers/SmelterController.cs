using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Smelter;

namespace TomekDexValheimMod.Controllers
{
    [HarmonyPatch]
    public class SmelterController : ContainerQuickDistributionObject<Smelter>
    {
        private class SmelterControllerOnDestroy : MonoBehaviour
        {
            private Smelter _componet;
            private void Awake()
            {
                _componet = GetComponent<Smelter>();
            }
            private void OnDestroy()
            {
                produtionInProgres.Remove(_componet);
                isOn.Remove(_componet);
            }
        }

        public static Dictionary<string, int> LimitProdution { get; set; }
        public static Dictionary<string, HashSet<string>> SkipOre { get; set; }
        public static Dictionary<string, int> LimitOre { get; set; }

        private static readonly Dictionary<Smelter, Dictionary<string, int>> produtionInProgres = new Dictionary<Smelter, Dictionary<string, int>>();
        private static readonly Dictionary<Smelter, bool> isOn = new Dictionary<Smelter, bool>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Smelter), "Awake")]
        public static void PostfixAwake(Smelter __instance)
        {
            __instance.gameObject.AddComponent(typeof(SmelterControllerOnDestroy));
            if (!On)
            {
                isOn[__instance] = false;
                return;
            }
            string name = __instance.name.Replace("(Clone)", "").Trim();
            if (Disable.Contains(name))
            {
                isOn[__instance] = false;
                return;
            }
            produtionInProgres[__instance] = new Dictionary<string, int>();
            isOn[__instance] = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Smelter), "QueueProcessed")]
        public static bool PrefixQueueProcessed(Smelter __instance, string ore)
        {
            if (!isOn[__instance])
                return true;
            ItemConversion itemConversion = __instance.GetItemConversion(ore);
            if (itemConversion == null)
                return true;
            int stack = __instance.m_nview.GetZDO().GetInt("SpawnAmount") + 1;
            __instance.m_produceEffects.Create(__instance.transform.position, __instance.transform.rotation);
            ItemDrop item = Instantiate(itemConversion.m_to.gameObject).GetComponent<ItemDrop>();
            item.m_itemData.m_stack = stack;
            if (ContainerQuickAccess.TryAddItemNearbyContainers(__instance.transform.position, WorkingArea, item))
            {
                __instance.m_produceEffects.Create(__instance.transform.position, __instance.transform.rotation);
                __instance.m_nview.GetZDO().Set("SpawnAmount", 0);
                return false;
            }
            __instance.m_nview.GetZDO().Set("SpawnAmount", item.m_itemData.m_stack - 1);
            return true;
        }

        public override void UpdateOnTime()
        {
            AddFuel();
            AddOre();
        }

        private void AddOre()
        {
            int queue = MBComponet.GetQueueSize();
            if (MBComponet.m_maxOre <= queue)
                return;

            foreach (ItemConversion conversion in MBComponet.m_conversion)
            {
                string name = MBComponet.name.Replace("(Clone)", "").Trim();
                if (SkipAdd(conversion))
                    continue;

                queue = MBComponet.GetQueueSize();
                if (MBComponet.m_maxOre <= queue)
                    break;
                int removed = ContainerQuickAccess.TryRemoveItemNearbyContainer(MBComponet.transform.position, WorkingArea, conversion.m_from, MBComponet.m_maxOre - queue);
                if (removed > 0)
                {
                    for (int i = 0; i < removed; i++)
                        MBComponet.QueueOre(conversion.m_from.name);
                    produtionInProgres[MBComponet][conversion.m_from.name] = removed + queue;
                }
            }
            MBComponet.m_oreAddedEffects.Create(MBComponet.transform.position, MBComponet.transform.rotation);
        }

        private void AddFuel()
        {
            int fuel = Mathf.CeilToInt(MBComponet.GetFuel());
            if (fuel >= MBComponet.m_maxFuel)
                return;
            int removed = ContainerQuickAccess.TryRemoveItemNearbyContainer(MBComponet.transform.position, WorkingArea, MBComponet.m_fuelItem, MBComponet.m_maxFuel - fuel);
            fuel += removed;
            MBComponet.SetFuel(fuel);
            MBComponet.m_fuelAddedEffects.Create(MBComponet.transform.position, MBComponet.transform.rotation);
            MBComponet.UpdateState();
        }

        private bool SkipAdd(ItemConversion conversion)
        {
            if (SkipOre.TryGetValue(name, out HashSet<string> skip) && skip.Contains(conversion.m_from?.name))
            {
                if (ContainerQuickDistribution.Logs)
                    Debug.Log($"SkipOre {conversion.m_from?.name} {name}");
                return true;
            }
            if (LimitProdution.TryGetValue(conversion.m_to?.name, out int limit))
            {
                if (limit == 0)
                    return true;
                int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, conversion.m_to);
                if (ContainerQuickDistribution.Logs)
                    Debug.Log($"Limit {limit}/{count} {conversion.m_to?.name}");
                if (count >= limit)
                    return true;
                else
                {
                    count += produtionInProgres.Select(a => a.Value.TryGetValue(conversion.m_to?.name, out int production) ? production : 0).Sum();
                    if (count >= limit)
                        return true;
                }
            }
            if (LimitOre.TryGetValue(conversion.m_from?.name, out limit))
            {
                if (limit == 0)
                    return true;
                int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, conversion.m_from);
                if (ContainerQuickDistribution.Logs)
                    Debug.Log($"Limit {limit}/{count} {conversion.m_from.name}");
                if (count <= limit)
                    return true;
            }
            return false;
        }
    }
}