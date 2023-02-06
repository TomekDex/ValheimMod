using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomekDexValheimModHelper;
using UnityEngine;

namespace TomekDexValheimMod
{
    public class EpicLootHelper
    {
        public class Conversion
        {
            public string Name { get; set; }
            public int Amount { get; set; }
            public List<Conversion> Resources { get; set; }
        }

        public static bool EnabledEpicLoot { get { return epicLootAssembly != null && ObliteratorConfig.TryProcessEpicLoot.Value; } }
        private static readonly Assembly epicLootAssembly;
        private static readonly MethodInfo methodIsMagic;
        private static readonly MethodInfo methodGetDisenchantProducts;
        private static readonly MethodInfo methodGetRarity;
        private static readonly MethodInfo methodGetEnchantCosts;
        private static readonly FieldInfo fieldConversions;

        private static HashSet<string> conversionHashSet;
        public static HashSet<string> ConversionHashSet
        {
            get
            {
                InicializeConversionHashSet();
                return conversionHashSet;
            }
        }

        private static List<Conversion> conversionList;
        public static List<Conversion> ConversionList
        {
            get
            {
                InicializeConversionList();
                return conversionList;
            }
        }

        static EpicLootHelper()
        {
            if (Chainloader.PluginInfos.ContainsKey("randyknapp.mods.epicloot") && ObliteratorConfig.TryProcessEpicLoot.Value)
            {
                epicLootAssembly = Chainloader.PluginInfos["randyknapp.mods.epicloot"].Instance.GetType().Assembly;
                methodIsMagic = epicLootAssembly.GetType("EpicLoot.ItemDataExtensions").GetMethod("IsMagic", new Type[] { typeof(ItemDrop.ItemData) });
                methodGetDisenchantProducts = epicLootAssembly.GetType("EpicLoot.Crafting.EnchantCostsHelper").GetMethod("GetDisenchantProducts", new[] { typeof(ItemDrop.ItemData) });
                methodGetRarity = epicLootAssembly.GetType("EpicLoot.ItemDataExtensions").GetMethod("GetRarity");
                methodGetEnchantCosts = epicLootAssembly.GetType("EpicLoot.Crafting.EnchantTabController").GetMethod("GetEnchantCosts");
                fieldConversions = epicLootAssembly.GetType("EpicLoot.CraftingV2.MaterialConversions").GetField("Conversions");
            }
        }

        private static void InicializeConversionHashSet()
        {
            if (conversionHashSet != null || !ObliteratorConfig.ConvertEpicLootYunk.Value || !ObliteratorConfig.LeaveRestAfterCnversion.Value || ConversionList == null)
                return;
            conversionHashSet = new HashSet<string>();
            foreach (Conversion conversion in ConversionList)
            {
                conversionHashSet.Add(conversion.Name);
                foreach (Conversion resources in conversion.Resources)
                    conversionHashSet.Add(resources.Name);
            }
        }

        private static void InicializeConversionList()
        {
            if (conversionList != null || !ObliteratorConfig.ConvertEpicLootYunk.Value || epicLootAssembly == null)
                return;
            dynamic conversions = epicLootAssembly.GetType("EpicLoot.CraftingV2.MaterialConversions").GetField("Conversions").GetValue(null);
            Array enumType = epicLootAssembly.GetType("EpicLoot.CraftingV2.MaterialConversionType").GetEnumValues();
            dynamic enu = null;
            foreach (dynamic e in enumType)
                if (e.ToString() == "Junk")
                {
                    enu = e;
                    break;
                }
            dynamic conversionsValues = conversions.GetValues(enu, true);

            conversionList = new List<Conversion>();
            foreach (dynamic conversion in conversionsValues)
            {
                ItemDrop itemDrop = ItemsHelper.GetItemDropBySharedNameOrName(conversion.Product);
                if (itemDrop == null)
                    continue;
                List<Conversion> resource = new List<Conversion>();
                foreach (dynamic res in conversion.Resources)
                {
                    ItemDrop itemResource = ItemsHelper.GetItemDropBySharedNameOrName(res.Item);
                    if (itemResource == null)
                        continue;
                    resource.Add(new Conversion { Name = itemResource.name, Amount = res.Amount });
                }
                if (resource.Any() && resource.Sum(a => a.Amount) > 0)
                    conversionList.Add(new Conversion { Name = itemDrop.name, Amount = conversion.Amount, Resources = resource });
            }
        }

        public static bool IsMagic(ItemDrop.ItemData item)
        {
            if (methodIsMagic == null)
                return false;
            return (bool)methodIsMagic.Invoke(null, new[] { item });
        }

        public static IEnumerable<ItemDrop.ItemData> GetDisenchantProducts(ItemDrop.ItemData item)
        {
            if (!ObliteratorConfig.Sacrifice.Value || methodGetDisenchantProducts == null)
                yield break;
            dynamic disenchantProducts = methodGetDisenchantProducts.Invoke(null, new object[] { item });
            if (disenchantProducts != null)
                for (int i = 0; i < disenchantProducts.Count; i++)
                {
                    dynamic disenchantProduct = disenchantProducts[i];
                    for (int j = 0; j < disenchantProduct.Amount; j++)
                        yield return ItemsHelper.GetItemDropBySharedNameOrName(disenchantProduct.Item).m_itemData;
                }
        }

        public static IEnumerable<ItemDrop.ItemData> GetEnchantCosts(ItemDrop.ItemData item)
        {
            if (!IsMagic(item))
                yield break;
            int rarity = (int)methodGetRarity.Invoke(null, new[] { item });
            List<KeyValuePair<ItemDrop, int>> magicReqs = (List<KeyValuePair<ItemDrop, int>>)methodGetEnchantCosts.Invoke(null, new object[] { item, rarity });
            foreach (KeyValuePair<ItemDrop, int> kvp in magicReqs)
                for (int i = 0; i < kvp.Value; i++)
                    yield return kvp.Key.m_itemData;
        }
    }
}
