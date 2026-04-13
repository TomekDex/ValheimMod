using System.Collections.Generic;
using TomekDexValheimModHelper;
using UnityEngine;

namespace TomekDexValheimMod.Controllers
{
    public class CraftingStationController : ContainerQuickDistributionObject<CraftingStation>
    {
        private Dictionary<Recipe, Crafting> recipes;
        private int recipeCount;
        private int level;
        public override void UpdateOnTime()
        {
            if (Player.m_localPlayer == null)
                return;
            UpdateRecipe();
            if (ContainerQuickDistributionConfig.Logs)
                Log();
            bool succes = false;
            foreach (KeyValuePair<Recipe, Crafting> recipe in recipes)
                succes = ProcessRecipe(recipe.Key, recipe.Value) || succes;
            if (succes)
                MBComponet.m_craftItemDoneEffects.Create(MBComponet.transform.position, Quaternion.identity);
        }

        private void Log()
        {
            foreach (KeyValuePair<Recipe, Crafting> recipe in recipes)
            {
                foreach (var item in recipe.Key.m_resources)
                {
                    Debug.Log($"{item.m_resItem.name} {item.m_extraAmountOnlyOneIngredient}");
                }

                string log = $"{recipe.Key.name} {recipe.Key.m_item.name}, {recipe.Value.Limit}, {recipe.Value.Quantity}";
                log += "\r\nMatsLimit";
                foreach (KeyValuePair<string, int> c in recipe.Value.MatsLimit)
                {
                    log += $"\r\n{c.Key} {c.Value}";
                }
                log += "\r\nOtherLimit";
                foreach (KeyValuePair<string, int> c in recipe.Value.OtherLimit)
                {
                    log += $"\r\n{c.Key} {c.Value}";
                }
                log += "\r\nRecipesResourcesNeeded";
                foreach (Dictionary<string, (int amount, int extraAmount)> resourcesNeeded in recipe.Value.RecipesResourcesNeeded[recipe.Key])
                {
                    foreach (KeyValuePair<string, (int amount, int extraAmount)> resource in resourcesNeeded)
                    {
                        log += $"\r\n{resource.Key} {resource.Value}";
                    }
                    log += "\r\n";
                }
                Debug.Log(log);
            }
        }

        private bool ProcessRecipe(Recipe recipe, Crafting options)
        {
            if (options.Limit > 0)
            {
                int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, recipe.m_item);
                if (options.Limit + recipe.m_amount < count)
                {
                    if (ContainerQuickDistributionConfig.Logs)
                        Debug.Log($"Limit {recipe.m_item.m_itemData.m_shared.m_name} {count}/{options.Limit}");
                    return false;
                }
            }
            foreach (KeyValuePair<string, int> limit in options.OtherLimit)
            {
                if (limit.Value > 0)
                {
                    ItemDrop item = ItemsHelper.GetItemDropBySharedNameOrName(limit.Key);
                    int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, item);
                    if (limit.Value <= count)
                    {
                        if (ContainerQuickDistributionConfig.Logs)
                            Debug.Log($"Limit {limit.Key} {count}/{limit.Value}");
                        return false;
                    }
                }
            }
            foreach (KeyValuePair<string, int> limit in options.MatsLimit)
            {
                if (limit.Value > 0)
                {
                    ItemDrop item = ItemsHelper.GetItemDropBySharedNameOrName(limit.Key);
                    int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, item);
                    if (limit.Value >= count)
                    {
                        if (ContainerQuickDistributionConfig.Logs)
                            Debug.Log($"Limit {limit.Key} {count}/{limit.Value}");
                        return false;
                    }
                }
            }
            bool product = false;
            foreach (Dictionary<string, (int amount, int extraAmount)> resources in options.RecipesResourcesNeeded[recipe])
                product = Product(resources, recipe, options) || product;
            return product;
        }

        private bool Product(Dictionary<string, (int amount, int extraAmount)> resources, Recipe recipe, Crafting options)
        {
            if (options.Limit > 0)
            {
                int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, recipe.m_item);
                if (options.Limit + recipe.m_amount < count)
                {
                    if (ContainerQuickDistributionConfig.Logs)
                        Debug.Log($"Limit {recipe.m_item.m_itemData.m_shared.m_name} {count}/{options.Limit}");
                    return false;
                }
            }
            int? ammound = null;
            foreach (KeyValuePair<string, (int amount, int extraAmount)> resource in resources)
            {
                ItemDrop item = ItemsHelper.GetItemDropBySharedNameOrName(resource.Key);
                int countResource = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, item);
                if (options.MatsLimit.TryGetValue(resource.Key, out int matsLimit))
                {
                    Debug.LogError($"limit {resource.Key}, {matsLimit}/{countResource}");
                    countResource -= matsLimit;
                }
                int posibleAmmound = countResource / resource.Value.amount;
                if (posibleAmmound == 0)
                    return false;
                if (ammound == null || ammound > posibleAmmound)
                    ammound = posibleAmmound;
            }
            if (ammound == null)
                return false;
            if (options.Limit > 0)
            {
                int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, recipe.m_item);
                int wanted = (options.Limit - count) / recipe.m_amount;
                if (ammound > wanted)
                    ammound = wanted;
            }
            if (ammound == 0)
                return false;
            int stack = 0;
            if (ammound < 1)
                return false;
            foreach (KeyValuePair<string, (int amount, int extraAmount)> resource in resources)
            {
                ItemDrop prefab = ItemsHelper.GetItemDropBySharedNameOrName(resource.Key);
                if (prefab == null)
                {
                    Debug.LogError($"Missing prefab for resource {resource.Key}");
                    return false;
                }
                int toRemove = ammound.Value * resource.Value.amount;
                int maxQuality = prefab.m_itemData.m_shared.m_maxQuality;
                for (int i = 1; i <= maxQuality; i++)
                {
                    // create ItemData clone for this quality without instantiating the prefab in scene
                    ItemDrop.ItemData data = CreateItemData(prefab, 1);
                    if (data == null)
                        continue;
                    data.m_quality = i;
                    int removed = ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, data, toRemove);
                    if (removed > 0 && recipe.m_requireOnlyOneIngredient)
                        stack += (recipe.m_amount + (int)Mathf.Ceil((data.m_quality - 1) * recipe.m_amount * recipe.m_qualityResultAmountMultiplier) + resource.Value.extraAmount) * removed / resource.Value.amount;
                    toRemove -= removed;
                }
                if (toRemove != 0)
                    Debug.LogError($"Removed the wrong amount of material {resource.Key} {toRemove}/{ammound.Value * resource.Value.amount}");
            }
            int craftedStack = recipe.m_requireOnlyOneIngredient ? stack : recipe.m_amount * ammound.Value;
            ItemDrop.ItemData craftedData = CreateItemData(recipe.m_item, craftedStack);
            if (craftedData == null)
                return true;
            craftedData.m_quality = options.Quantity;
            craftedData.m_durability = craftedData.GetMaxDurability();
            if (!ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, craftedData))
                ItemsHelper.Drop(recipe.m_item, craftedData.m_stack, MBComponet.transform.position);
            return true;
        }

        private void UpdateRecipe()
        {
            if (recipes == null || recipeCount != Player.m_localPlayer.m_knownRecipes.Count || MBComponet.GetLevel() != level)
            {
                recipeCount = Player.m_localPlayer.m_knownRecipes.Count;
                level = MBComponet.GetLevel();
                recipes = new Dictionary<Recipe, Crafting>();
                foreach (Crafting craftingOptions in Crafting.CraftingOptions)
                    foreach (Recipe recipe in craftingOptions.RecipesResourcesNeeded.Keys)
                        if (recipe.m_enabled && (recipe.m_item.m_itemData.m_shared.m_dlc.Length <= 0 || DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc)) && (Player.m_localPlayer.m_knownRecipes.Contains(recipe.m_item.m_itemData.m_shared.m_name) || Player.m_localPlayer.m_noPlacementCost) && (RequiredCraftingStation(recipe, craftingOptions.Quantity) || Player.m_localPlayer.m_noPlacementCost))
                            recipes.Add(recipe, craftingOptions);
            }
        }

        private bool RequiredCraftingStation(Recipe recipe, int quantity)
        {
            CraftingStation requiredStation = recipe.GetRequiredStation(quantity);
            if (requiredStation != null)
            {
                if (requiredStation.m_name != MBComponet.m_name)
                {
                    return false;
                }
                int requiredStationLevel = recipe.GetRequiredStationLevel(quantity);
                if (MBComponet.GetLevel() < requiredStationLevel)
                {
                    return false;
                }
            }
            else if (!MBComponet.m_showBasicRecipies)
            {
                return false;
            }
            return true;
        }
    }
}