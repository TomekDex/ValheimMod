using System.Collections.Generic;
using TomekDexValheimModHelper;
using UnityEngine;

namespace TomekDexValheimMod.Controllers
{
    public class CraftingStationController : ContainerQuickDistributionObject<CraftingStation>
    {
        private Dictionary<Recipe, Crafting> recipes;
        private readonly Dictionary<Recipe, List<Dictionary<string, int>>> recipesResourcesNeeded = new Dictionary<Recipe, List<Dictionary<string, int>>>();
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
                    Debug.Log($"{item.m_resItem.name} {item.m_extraAmountOnlyOneIngredient}" );
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
                product = Product(resources, recipe, options.Limit, options.Quantity) || product;
            return product;
        }

        private bool Product(Dictionary<string, (int amount, int extraAmount)> resources, Recipe recipe, int limit, int quantity)
        {
            if (limit > 0)
            {
                int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, recipe.m_item);
                if (limit + recipe.m_amount < count)
                {
                    if (ContainerQuickDistributionConfig.Logs)
                        Debug.Log($"Limit {recipe.m_item.m_itemData.m_shared.m_name} {count}/{limit}");
                    return false;
                }
            }
            int? ammound = null;
            foreach (KeyValuePair<string, (int amount, int extraAmount)> resource in resources)
            {
                ItemDrop item = ItemsHelper.GetItemDropBySharedNameOrName(resource.Key);
                int countResource = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, item);
                int posibleAmmound = countResource / resource.Value.amount;
                if (posibleAmmound == 0)
                    return false;
                if (ammound == null || ammound > posibleAmmound)
                    ammound = posibleAmmound;
            }
            if (ammound == null)
                return false;
            if (limit > 0)
            {
                int count = ContainerQuickAccess.CountItems(MBComponet.transform.position, WorkingArea, recipe.m_item);
                int wanted = (limit - count) / recipe.m_amount;
                if (ammound > wanted)
                    ammound = wanted;
            }
            if (ammound == 0)
                return false;

            int stack = 0;
            foreach (KeyValuePair<string, (int amount, int extraAmount)> resource in resources)
            {
                ItemDrop item = ItemsHelper.GetItemDropBySharedNameOrName(resource.Key);
                item = Instantiate(item);
                int toRemove = (ammound.Value * resource.Value.amount);
                for (int i = 1; i <= item.m_itemData.m_shared.m_maxQuality; i++)
                {
                    item.m_itemData.m_quality = i;
                    int removed = ContainerQuickAccess.TryRemoveItemRegistertNearbyContainer(MBComponet.transform.position, WorkingArea, item, toRemove);
                    if (removed > 0 && recipe.m_requireOnlyOneIngredient)
                        stack += (recipe.m_amount + (int)Mathf.Ceil((item.m_itemData.m_quality - 1) * recipe.m_amount * recipe.m_qualityResultAmountMultiplier) + resource.Value.extraAmount) * removed / resource.Value.amount;
                    toRemove -= removed;
                }
                if (toRemove != 0)
                    Debug.LogError($"Removed the wrong amount of material {resource.Key} {toRemove}/{ammound.Value * resource.Value.amount}");
            }
            ItemDrop itemDrop = Instantiate(recipe.m_item);

            itemDrop.m_itemData.m_stack = recipe.m_requireOnlyOneIngredient ? stack : recipe.m_amount * ammound.Value;
            itemDrop.m_itemData.m_quality = quantity;
            itemDrop.m_itemData.m_durability = itemDrop.m_itemData.GetMaxDurability();
            if (!ContainerQuickAccess.TryAddItemNearbyContainers(MBComponet.transform.position, WorkingArea, itemDrop))
                ItemsHelper.Drop(itemDrop, MBComponet.transform.position);
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