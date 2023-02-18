using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CookingStation;

namespace TomekDexValheimMod.Controllers
{
    [HarmonyPatch]
    public class CookingStationController : ContainerQuickDistributionObject<CookingStation>
    {
        private class CookingStationControllerOnDestroy : MonoBehaviour
        {
            private CookingStation _componet;
            private void Awake()
            {
                _componet = GetComponent<CookingStation>();
            }
            private void OnDestroy()
            {
                produtionInProgres.Remove(_componet);
                isOn.Remove(_componet);
            }
        }

        public static Dictionary<string, int> LimitCooking { get; internal set; }
        public static Dictionary<string, int> LimitItemUseToCook { get; internal set; }
        public static Dictionary<string, int> LimitWood { get; internal set; }
        private static readonly Dictionary<CookingStation, Dictionary<string, int>> produtionInProgres = new Dictionary<CookingStation, Dictionary<string, int>>();
        private static readonly Dictionary<CookingStation, bool> isOn = new Dictionary<CookingStation, bool>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CookingStation), "Awake")]
        public static void PostfixAwake(CookingStation __instance)
        {
            __instance.gameObject.AddComponent(typeof(CookingStationControllerOnDestroy));
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
        [HarmonyPatch(typeof(CookingStation), "UpdateCooking")]
        public static void PrefixUpdateCooking(CookingStation __instance)
        {
            if (!isOn[__instance])
                return;
            for (int i = 0; i < __instance.m_slots.Length; i++)
            {
                __instance.GetSlot(i, out string itemName, out float cookedTime, out Status status);
                ItemConversion itemConversion = __instance.GetItemConversion(itemName);
                if (itemConversion == null)
                    continue;
                switch (status)
                {
                    case Status.NotDone:
                        if (cookedTime > itemConversion.m_cookTime && itemName == itemConversion.m_from.name)
                            GetItem(__instance, itemConversion.m_to, i);
                        break;
                    case Status.Done:
                        GetItem(__instance, itemConversion.m_to, i);
                        break;
                    case Status.Burnt:
                        GetItem(__instance, __instance.m_overCookedItem, i);
                        break;
                    default:
                        break;
                }
            }
        }

        private static void GetItem(CookingStation cookingStation, ItemDrop item, int slot)
        {
            cookingStation.m_doneEffect.Create(cookingStation.m_slots[slot].position, Quaternion.identity);
            ItemDrop itemNew = Instantiate(item);
            item.m_itemData.m_stack = 1;
            if (!ContainerQuickAccess.TryAddItemNearbyContainers(cookingStation.transform.position, WorkingArea, itemNew))
            {
                Vector3 position = new Vector3(cookingStation.transform.position.x + 1, cookingStation.transform.position.y, cookingStation.transform.position.z + 1);
                Drop(itemNew, position);
            }
            if (produtionInProgres[cookingStation].ContainsKey(item.name))
                produtionInProgres[cookingStation][item.name]--;
            cookingStation.SetSlot(slot, "", 0f, Status.NotDone);
        }

        private static void Drop(ItemDrop item, Vector3 position)
        {
            for (int i = 0; i < item.m_itemData.m_stack;)
            {
                ItemDrop itemNew = Instantiate(item);
                if (item.m_itemData.m_shared.m_maxStackSize < item.m_itemData.m_stack)
                    itemNew.m_itemData.m_stack = item.m_itemData.m_shared.m_maxStackSize;
                else
                    itemNew.m_itemData.m_stack = item.m_itemData.m_stack;
                i += itemNew.m_itemData.m_stack;
                Instantiate(itemNew, position, Quaternion.identity);
            }
        }

        public override void UpdateOnTime()
        {
            AddItem();
            AddFuelIfNeed();
        }

        private void AddFuelIfNeed()
        {
            if (!(bool)MBComponet.m_addFuelSwitch)
                return;
            if (MBComponet.IsEmpty())
                return;
            float fuel = MBComponet.GetFuel();
            if (fuel > 0 || fuel > MBComponet.m_maxFuel - 1)
                return;
            if (LimitWood.TryGetValue(MBComponet.m_fuelItem.name, out int limit))
                if (limit >= ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, MBComponet.m_fuelItem))
                    return;
            int removed = ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, MBComponet.m_fuelItem, 1);
            if (removed == 0)
                return;
            MBComponet.SetFuel(fuel + 1f);
            MBComponet.m_fuelAddedEffects.Create(MBComponet.transform.position, MBComponet.transform.rotation, MBComponet.transform);
        }

        private void AddItem()
        {
            if (MBComponet.m_requireFire && !MBComponet.IsFireLit())
                return;
            foreach (ItemConversion conversion in MBComponet.m_conversion)
            {
                int freeSlot = MBComponet.GetFreeSlot();
                if (freeSlot == -1)
                    return;
                if (SkipAdd(conversion))
                    continue;
                int removed = ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, conversion.m_from, 1);
                while (removed != 0)
                {
                    MBComponet.SetSlot(freeSlot, conversion.m_from.name, 0f, Status.NotDone);
                    MBComponet.m_nview.InvokeRPC(ZNetView.Everybody, "SetSlotVisual", freeSlot, conversion.m_from.name);
                    MBComponet.m_addEffect.Create(MBComponet.m_slots[freeSlot].position, Quaternion.identity);
                    if (!produtionInProgres[MBComponet].ContainsKey(conversion.m_to.name))
                        produtionInProgres[MBComponet][conversion.m_to.name] = 1;
                    else
                        produtionInProgres[MBComponet][conversion.m_to.name]++;
                    freeSlot = MBComponet.GetFreeSlot();
                    if (freeSlot == -1)
                        return;
                    removed = ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, conversion.m_from, 1);
                }
            }
        }

        private bool SkipAdd(ItemConversion conversion)
        {
            if (LimitItemUseToCook.TryGetValue(conversion.m_from.name, out int limit))
            {
                if (limit == 0)
                    return true;
                int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, conversion.m_from);
                if (ContainerQuickDistributionConfig.Logs)
                    Debug.Log($"Limit {limit}/{count} {conversion.m_from?.name}");
                if (limit >= count)
                    return true;
            }
            if (LimitCooking.TryGetValue(conversion.m_to.name, out limit))
            {
                if (limit == 0)
                    return true;
                int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, conversion.m_to);
                if (ContainerQuickDistributionConfig.Logs)
                    Debug.Log($"Limit {limit}/{count} {conversion.m_to?.name}");
                if (limit <= count)
                    return true;
                else
                {
                    count += produtionInProgres.Select(a => a.Value.TryGetValue(conversion.m_to?.name, out int production) ? production : 0).Sum();
                    if (count >= limit)
                        return true;
                }
            }
            return false;
        }
    }
}