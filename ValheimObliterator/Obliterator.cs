using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TomekDexValheimModHelper;
using UnityEngine;
using static Incinerator;
using static TomekDexValheimMod.EpicLootHelper;

namespace TomekDexValheimMod
{
    [HarmonyPatch]
    public class Obliterator
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(IncineratorConversion), "AttemptCraft")]
        public static bool PrefixIncinerate(IncineratorConversion __instance, ref int __result, Inventory inv, List<ItemDrop> toAdd)
        {
            List<ItemDrop.ItemData> items = inv.GetAllItems();
            if (items.Count == 0)
            {
                return false;
            }
            ObliteratorConfig.Inicialize();
            Dictionary<string, int> itemsCount = CalculateItemCount(items);
            Conversion(itemsCount);
            DisenchantProductsAfterConversion(itemsCount);
            ulong itemsValue = GetItemsValue(toAdd, itemsCount);
            Debug.Log($"Convert count.");
            foreach (KeyValuePair<string, int> item in itemsCount)
                Debug.Log($"{item.Key} {item.Value}");
            Debug.Log($"Convert ItemsValue {itemsValue}");
            ConvertToReward(toAdd, itemsValue);
            inv.RemoveAll();
            return false;
        }

        private static void DisenchantProductsAfterConversion(Dictionary<string, int> itemsCount)
        {
            if (!EnabledEpicLoot)
                return;
            foreach (string name in itemsCount.Keys.ToList())
            {
                ItemDrop item = ItemsHelper.GetItemDropBySharedNameOrName(name);
                List<ItemDrop.ItemData> disenchantProducts = GetDisenchantProducts(item.m_itemData).ToList();
                if (disenchantProducts.Any())
                {
                    int count = itemsCount[name];
                    itemsCount.Remove(name);
                    foreach (ItemDrop.ItemData product in disenchantProducts)
                        foreach (ItemDrop.ItemData itemC in GetComponets(product))
                            AddToItemCount(itemsCount, ItemsHelper.GetItemDropBySharedNameOrName(itemC.m_shared.m_name).name, count);
                }
            }
        }

        private static void ConvertToReward(List<ItemDrop> toAdd, ulong itemsValue)
        {
            KeyValuePair<string, int>[] posibleRevards = ObliteratorConfig.NameAndCostDic.Where(a => Convert.ToUInt64(a.Value) <= itemsValue).ToArray();
            while (posibleRevards.Any())
            {
                KeyValuePair<string, int> revard = GetReward(posibleRevards);
                itemsValue -= Convert.ToUInt64(revard.Value);
                toAdd.Add(ItemsHelper.GetItemDropBySharedNameOrName(revard.Key));
                posibleRevards = ObliteratorConfig.NameAndCostDic.Where(a => Convert.ToUInt64(a.Value) <= itemsValue).ToArray();
            }
            Debug.Log($"Convert result.");
            foreach (KeyValuePair<string, int> item in toAdd.Select(a => a.name).Distinct().ToDictionary(a => a, a => toAdd.Count(b => b.name == a)))
                Debug.Log($"{item.Key} {item.Value}");
        }

        private static ulong GetItemsValue(List<ItemDrop> toAdd, Dictionary<string, int> itemsCount)
        {
            ulong itemsValue = 0;
            foreach (KeyValuePair<string, int> itemCount in itemsCount)
            {
                if (ObliteratorConfig.IgnoreHash.Contains(itemCount.Key) || ConversionHashSet?.Contains(itemCount.Key) == true)
                    for (int i = 0; i < itemCount.Value; i++)
                        toAdd.Add(ItemsHelper.GetItemDropBySharedNameOrName(itemCount.Key));
                else
                {
                    int itemValue = 1;
                    if (ObliteratorConfig.NameAndValueDic.TryGetValue(itemCount.Key, out int value))
                    {
                        itemValue = value;
                    }
                    else
                    {
                        ItemDrop itemDrop = ItemsHelper.GetItemDropBySharedNameOrName(itemCount.Key);
                        if (ObliteratorConfig.TypeAndValueDic.TryGetValue(itemDrop.m_itemData.m_shared.m_itemType.ToString(), out value))
                            itemValue = value;
                    }
                    itemsValue += Convert.ToUInt64(itemValue) * Convert.ToUInt64(itemCount.Value);
                }
            }
            return itemsValue;
        }

        private static Dictionary<string, int> CalculateItemCount(List<ItemDrop.ItemData> items)
        {
            Dictionary<string, int> itemsCount = new Dictionary<string, int>();

            foreach (ItemDrop.ItemData item in items)
                foreach (ItemDrop.ItemData componet in GetComponets(item))
                {
                    List<ItemDrop.ItemData> disenchantProducts = GetDisenchantProducts(componet).ToList();
                    if (disenchantProducts.Any())
                        foreach (ItemDrop.ItemData product in disenchantProducts)
                        {
                            ItemDrop itemDrop = ItemsHelper.GetItemDropBySharedNameOrName(product.m_shared.m_name);
                            AddToItemCount(itemsCount, itemDrop.name, 1);
                        }
                    else
                    {
                        ItemDrop itemDrop = ItemsHelper.GetItemDropBySharedNameOrName(componet.m_shared.m_name);
                        AddToItemCount(itemsCount, itemDrop.name, 1);
                    }
                }

            return itemsCount;
        }

        private static void AddToItemCount(Dictionary<string, int> itemsCount, string name, int amount)
        {
            if (!itemsCount.ContainsKey(name))
                itemsCount.Add(name, amount);
            else
                itemsCount[name] += amount;
        }

        private static void Conversion(Dictionary<string, int> itemsCount)
        {
            if (ConversionList != null)
            {
                Conversion recipe = ConversionList.FirstOrDefault(a => HaveResources(a.Resources, itemsCount));
                while (recipe != null)
                {
                    foreach (Conversion res in recipe.Resources)
                        itemsCount[res.Name] -= res.Amount;
                    AddToItemCount(itemsCount, recipe.Name, recipe.Amount);
                    recipe = ConversionList.FirstOrDefault(a => HaveResources(a.Resources, itemsCount));
                }
            }
        }

        private static bool HaveResources(List<Conversion> resources, Dictionary<string, int> itemsCount)
        {
            foreach (Conversion res in resources)
                if (!itemsCount.Any(a => a.Key == res.Name && a.Value >= res.Amount))
                    return false;
            return true;
        }

        private static KeyValuePair<string, int> GetReward(KeyValuePair<string, int>[] posibleRevards)
        {
            int sum = posibleRevards.Sum(a => ObliteratorConfig.NameAndRewardChanceDic.TryGetValue(a.Key, out int value) ? value : 1);
            int random = UnityEngine.Random.Range(0, sum);
            while (true)
                foreach (KeyValuePair<string, int> item in posibleRevards)
                {
                    int rewardChanceDic = ObliteratorConfig.NameAndRewardChanceDic.TryGetValue(item.Key, out int value) ? value : 1;
                    for (int j = 0; j < rewardChanceDic; j++)
                    {
                        if (random == 0)
                            return item;
                        random--;
                    }
                }
        }

        private static IEnumerable<ItemDrop.ItemData> GetComponets(ItemDrop.ItemData item)
        {
            if (ObliteratorConfig.IgnoreHash.Contains(ItemsHelper.GetItemDropBySharedNameOrName(item.m_shared.m_name).name))
            {
                for (int i = 0; i < item.m_stack; i++)
                    yield return item;
                yield break;
            }
            if (EnabledEpicLoot)
            {
                List<ItemDrop.ItemData> disenchantProducts = GetDisenchantProducts(item).ToList();
                if (disenchantProducts.Any())
                {
                    foreach (ItemDrop.ItemData product in disenchantProducts)
                        foreach (ItemDrop.ItemData itemC in GetComponets(product))
                            for (int i = 0; i < item.m_stack; i++)
                                yield return itemC;
                    yield break;
                }
                else
                    foreach (ItemDrop.ItemData componet in GetEnchantCosts(item))
                        foreach (ItemDrop.ItemData itemC in GetComponets(componet))
                            for (int i = 0; i < item.m_stack; i++)
                                yield return itemC;

            }
            Recipe recipe = ObjectDB.instance.GetRecipe(item);
            if (recipe != null)
            {
                foreach (ItemDrop.ItemData componet in GetComponetsByRecipe(item, recipe))
                    yield return componet;
            }
            else
                for (int i = 0; i < item.m_stack; i++)
                    yield return item;
        }

        private static IEnumerable<ItemDrop.ItemData> GetComponetsByRecipe(ItemDrop.ItemData item, Recipe recipe)
        {
            if (recipe.m_amount <= 0)
                yield break;
            foreach (Piece.Requirement resource in recipe.m_resources)
            {
                int itemCount = 0;
                for (int i = item.m_quality; i > 0; i--)
                    itemCount += resource.GetAmount(i);
                itemCount *= item.m_stack;
                itemCount /= recipe.m_amount;
                for (int i = 0; i < itemCount; i++)
                    foreach (ItemDrop.ItemData itemC in GetComponets(resource.m_resItem.m_itemData))
                        yield return itemC;
            }
        }
    }
}
