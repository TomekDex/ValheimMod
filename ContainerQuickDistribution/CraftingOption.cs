using System.Collections.Generic;
using UnityEngine;

namespace TomekDexValheimMod
{
    public class Crafting
    {
        internal static List<Crafting> CraftingOptions { get; } = new List<Crafting>();
        public string Name { get; set; }
        public int Quantity { get; set; }
        public int Limit { get; set; }
        public Dictionary<string, int> MatsLimit { get; set; }
        public Dictionary<string, int> OtherLimit { get; set; }
        private Dictionary<Recipe, List<Dictionary<string, (int amount, int extraAmount)>>> recipesResourcesNeeded;
        public Dictionary<Recipe, List<Dictionary<string, (int amount, int extraAmount)>>> RecipesResourcesNeeded
        {
            get
            {
                if (recipesResourcesNeeded == null)
                    GetRecipesResourcesNeeded();
                return recipesResourcesNeeded;
            }
        }

        #region GetRecipesResourcesNeeded
        private void GetRecipesResourcesNeeded()
        {
            recipesResourcesNeeded = new Dictionary<Recipe, List<Dictionary<string, (int amount, int extraAmount)>>>();
            foreach (Recipe recipe in ObjectDB.instance.m_recipes)
            {
                if (recipe?.m_item?.name == Name)
                    AddRecipes(recipe, Quantity);
            }
        }

        private void AddRecipes(Recipe recipe, int quantity)
        {
            if (recipe.m_item.m_itemData.m_shared.m_dlc.Length > 0 && !DLCMan.instance.IsDLCInstalled(recipe.m_item.m_itemData.m_shared.m_dlc))
                return;
            Dictionary<string, (int amount, int extraAmount)> resourcesNeeded = new Dictionary<string, (int amount, int extraAmount)>();
            foreach (Piece.Requirement requirement in recipe.m_resources)
            {
                if ((bool)requirement.m_resItem)
                {
                    for (int i = 1; i <= quantity; i++)
                    {
                        int amountRes = requirement.GetAmount(i);
                        if (resourcesNeeded.ContainsKey(requirement.m_resItem.name))
                            resourcesNeeded[requirement.m_resItem.name] = (resourcesNeeded[requirement.m_resItem.name].amount + amountRes, resourcesNeeded[requirement.m_resItem.name].extraAmount);
                        else
                            resourcesNeeded[requirement.m_resItem.name] = (amountRes, requirement.m_extraAmountOnlyOneIngredient);
                    }
                    if (recipe.m_requireOnlyOneIngredient)
                    {
                        AddResourcesNeeded(recipe, resourcesNeeded);
                        resourcesNeeded = new Dictionary<string, (int amount, int extraAmount)>();
                    }
                }
            }
            if (!recipe.m_requireOnlyOneIngredient)
                AddResourcesNeeded(recipe, resourcesNeeded);
        }

        private void AddResourcesNeeded(Recipe recipe, Dictionary<string, (int amount, int extraAmount)> resourcesNeeded)
        {
            if (resourcesNeeded.Count == 0)
                return;
            if (!recipesResourcesNeeded.ContainsKey(recipe))
                recipesResourcesNeeded[recipe] = new List<Dictionary<string, (int amount, int extraAmount)>>();
            recipesResourcesNeeded[recipe].Add(resourcesNeeded);
        }
        #endregion GetRecipesResourcesNeeded

        #region ParseCrafting
        internal static void ParseCrafting(string value)
        {
            foreach (string conf in value.Split('|'))
            {
                string[] values = conf.Split(';');
                if (values.Length != 5)
                {
                    Debug.LogError($"TomekDexValheimMod CraftingStation wrong config {values.Length}/5 ; \r\n{conf}");
                    continue;
                }
                CraftingOptions.Add(ParseValues(conf, values));
            }
        }

        private static Crafting ParseValues(string conf, string[] values)
        {
            Crafting option = new Crafting();
            if (string.IsNullOrWhiteSpace(values[0]))
            {
                Debug.LogError($"TomekDexValheimMod CraftingStation wrong config no name\r\n{conf}");
                return option;
            }
            option.Name = values[0].Trim();
            if (!int.TryParse(values[1].Trim(), out int quantity))
            {
                Debug.LogError($"TomekDexValheimMod CraftingStation wrong config can not convert to int {values[1]}\r\n{conf}");
                return option;
            }
            option.Quantity = quantity;
            if (!string.IsNullOrWhiteSpace(values[2]))
            {
                if (!int.TryParse(values[2].Trim(), out int limit))
                {
                    Debug.LogError($"TomekDexValheimMod CraftingStation wrong config can not convert to int {values[2]}\r\n{conf}");
                    return option;
                }
                else
                    option.Limit = limit;
            }
            option.MatsLimit = GetDiconaryNameLimit(values[3]);
            option.OtherLimit = GetDiconaryNameLimit(values[4]);
            return option;
        }

        private static Dictionary<string, int> GetDiconaryNameLimit(string values)
        {
            Dictionary<string, int> diconaryNameLimit = new Dictionary<string, int>();
            if (string.IsNullOrWhiteSpace(values))
            {
                return diconaryNameLimit;
            }
            foreach (string splited in values.Split(','))
            {
                string[] stringInt = splited.Split('=');
                if (stringInt.Length != 2)
                {
                    Debug.LogError($"TomekDexValheimMod CraftingStation wrong config {stringInt.Length}/2 =\r\n{splited}");
                    continue;
                }
                if (string.IsNullOrWhiteSpace(stringInt[0]))
                {
                    Debug.LogError($"TomekDexValheimMod CraftingStation wrong config no name\r\n{splited}");
                    continue;
                }
                if (!int.TryParse(stringInt[1].Trim(), out int limit))
                {
                    Debug.LogError($"TomekDexValheimMod CraftingStation wrong config can not convert to int {stringInt[1]}\r\n{splited}");
                    continue;
                }
                diconaryNameLimit[stringInt[0].Trim()] = limit;
            }
            return diconaryNameLimit;
        }
        #endregion ParseCrafting
    }
}
