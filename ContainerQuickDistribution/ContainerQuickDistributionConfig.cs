using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomekDexValheimMod.Controllers;

namespace TomekDexValheimMod
{
    public class ContainerQuickDistributionConfig
    {
        public static bool Logs { get; private set; }
        public static bool StartQueue { get; private set; } = false;
        public static Dictionary<Type, bool> ConfigOn = new Dictionary<Type, bool>();

        internal static void GetConfig(ConfigFile config)
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ContainerQuickDistributionObject<>));
            foreach (Type type in assembly.GetTypes())
                if (type.BaseType?.IsGenericType == true && type.BaseType?.GetGenericTypeDefinition() == typeof(ContainerQuickDistributionObject<>))
                {
                    Type baseTypeGenericArgument = type.BaseType.GetGenericArguments()[0];
                    ConfigOn[baseTypeGenericArgument] = config.Bind(baseTypeGenericArgument.Name, "On", true, "Runs sections").Value;
                    PropertyInfo property = type.BaseType?.GetProperty("On", BindingFlags.Public | BindingFlags.Static);
                    property.SetValue(null, ConfigOn[baseTypeGenericArgument]);
                    property = type.BaseType?.GetProperty("UseQueue", BindingFlags.Public | BindingFlags.Static);
                    bool queue = config.Bind(baseTypeGenericArgument.Name, "UseQueue", true, "Instead of calling itself in the loop every now and then, it will be added to the queue and called in the loop sequentially").Value;
                    StartQueue = StartQueue || queue && ConfigOn[baseTypeGenericArgument];
                    property.SetValue(null, queue);
                    property = type.BaseType?.GetProperty("WorkingArea", BindingFlags.Public | BindingFlags.Static);
                    property.SetValue(null, config.Bind(baseTypeGenericArgument.Name, "WorkingArea", 50, "Operating range to boxes").Value);
                    property = type.BaseType?.GetProperty("MinSecUpdate", BindingFlags.Public | BindingFlags.Static);
                    property.SetValue(null, config.Bind(baseTypeGenericArgument.Name, "MinSecUpdate", 30, "Every object starts every radom amount of time").Value);
                    property = type.BaseType?.GetProperty("MaxSecUpdate", BindingFlags.Public | BindingFlags.Static);
                    property.SetValue(null, config.Bind(baseTypeGenericArgument.Name, "MaxSecUpdate", 90, "").Value);
                    string disableText = "Disables specific items by ID\r\n";
                    property = type.BaseType?.GetProperty("Disable", BindingFlags.Public | BindingFlags.Static);
                    switch (baseTypeGenericArgument.Name)
                    {
                        case "Fireplace":
                            string posibleDisableForFireplace = "fire_pit,bonfire,hearth,piece_groundtorch,piece_groundtorch_green,piece_groundtorch_blue,piece_groundtorch_wood,piece_walltorch,piece_brazierfloor01,piece_brazierceiling01";
                            property.SetValue(null, ParseHashSet(config.Bind(baseTypeGenericArgument.Name, "Disable", "", disableText + posibleDisableForFireplace).Value));
                            break;
                        case "Smelter":
                            string posibleDisableForSmelter = "piece_bathtub,smelter,blastfurnace,eitrrefinery,charcoal_kiln,piece_spinningwheel,windmill";
                            property.SetValue(null, ParseHashSet(config.Bind(baseTypeGenericArgument.Name, "Disable", "", disableText + posibleDisableForSmelter).Value));
                            break;
                        case "CookingStation":
                            string posibleDisableCookingStation = "piece_oven,piece_cookingstation_iron,piece_cookingstation";
                            property.SetValue(null, ParseHashSet(config.Bind(baseTypeGenericArgument.Name, "Disable", "", disableText + posibleDisableCookingStation).Value));
                            break;
                        case "ItemDrop":
                            string posibleDisableItemDrop = "ChickenEgg,Fish1,Fish2,Fish3,Fish4_cave,Fish5,Fish6,Fish7,Fish8,Fish9,Fish10,Fish11,Fish12";
                            property.SetValue(null, ParseHashSet(config.Bind(baseTypeGenericArgument.Name, "Disable", posibleDisableItemDrop, disableText + posibleDisableItemDrop).Value));
                            break;
                        default:
                            property.SetValue(null, ParseHashSet(config.Bind(baseTypeGenericArgument.Name, "Disable", "", disableText.Trim()).Value));
                            break;
                    }
                    property = type.BaseType?.GetProperty("DisableAllExcept", BindingFlags.Public | BindingFlags.Static);
                    switch (baseTypeGenericArgument.Name)
                    {
                        case "MonsterAI":
                            string posibleDisableAllExceptMonsterAI = "Lox,Hen,Wolf,Boar";
                            property.SetValue(null, ParseHashSet(config.Bind(baseTypeGenericArgument.Name, "DisableAllExcept", posibleDisableAllExceptMonsterAI, posibleDisableAllExceptMonsterAI).Value));
                            break;
                        default:
                            property.SetValue(null, ParseHashSet(config.Bind(baseTypeGenericArgument.Name, "DisableAllExcept", "", "").Value));
                            break;
                    }
                }
            CookingStationController.LimitWood = ParseConfigNameValue(config.Bind("CookingStation", "LimitWood", "", "Stops adding to production if nearby chests don't have enough\r\nWood=100000000\r\n0 skip, null unlimit"));
            CookingStationController.LimitCooking = ParseConfigNameValue(config.Bind("CookingStation", "LimitCooking", "", "Stops adding to production if nearby chests already have enough\r\nSerpentMeatCooked=5\r\n0 skip, null unlimit"));
            CookingStationController.LimitItemUseToCook = ParseConfigNameValue(config.Bind("CookingStation", "LimitItemUseToCook", "NeckTail=0,RawMeat=0,WolfMeat=0,LoxMeat=0,BugMeat=0,HareMeat=0,ChickenMeat=0", "Stops adding to production if nearby chests don't have enough\r\nNeckTail=0,RawMeat=0,WolfMeat=0,LoxMeat=0,BugMeat=0,HareMeat=0\r\n0 skip, null unlimit"));
            SmelterController.LimitOre = ParseConfigNameValue(config.Bind("Smelter", "LimitOre", "Barley=300,Flax=300", "Stops adding to production if nearby chests don't have enough\r\nCoal=100,Copper=200\r\n0 skip, null unlimit"));
            SmelterController.LimitProdution = ParseConfigNameValue(config.Bind("Smelter", "LimitProdution", "Coal=100", "Stops adding to production if nearby chests already have enough\r\nCoal=100,Copper=200\r\n0 skip, null unlimit"));
            SmelterController.SkipOre = ParseConfigNameNames(config.Bind("Smelter", "SkipOre", "charcoal_kiln=FineWood,charcoal_kiln=RoundLog", "Don't add thes ore\r\ncharcoal_kiln=Wood,charcoal_kiln=FineWood,charcoal_kiln=RoundLog,smelter=CopperOre"));
            MonsterAIController.ChekPath = config.Bind("MonsterAI", "ChekPath", true, "").Value;
            MonsterAIController.LimitFood = ParseConfigNameValue(config.Bind("MonsterAI", "LimitFood", "Dandelion=200,Barley=300,Flax=300,BeechSeeds=0,BirchSeeds=0,CarrotSeeds=0,OnionSeeds=0,TurnipSeeds=0,Cloudberry=0", "Stops adding to animal if nearby chests don't have enough\r\nOnionSeeds=0,TurnipSeeds=0\r\n0 skip, null unlimit"));
            Crafting.ParseCrafting(config.Bind("CraftingStation", "Crafting", craftingConfig, "{IdCraftingItems};{Quantity};{LimitCrafting};{IdResource=Limit},{IdResource=Limit};{IdItem=LimitThisItem},{IdItem=LimitThisItem}").Value);
            Logs = config.Bind("", "Logs", false, "Turn on logs").Value;
            ContainerQuickDistributionQueue.ProcessingTimeMilliseconds = config.Bind("Queue", "ProcessingTimeMilliseconds", 5, "").Value;
            ContainerQuickDistributionQueue.TimeSecondsToProcess = config.Bind("Queue", "TimeSecondsToProcess", 1f, "Time in seconds every time to process").Value;
            ContainerQuickDistributionQueue.DelayedWhenStart = config.Bind("Queue", "DelayedWhenStart", 30f, "Delayed when to start").Value;
        }

        private static HashSet<string> ParseHashSet(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new HashSet<string>();
            return value.Split(',').Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).ToHashSet();
        }

        private static Dictionary<string, HashSet<string>> ParseConfigNameNames(ConfigEntry<string> configEntry)
        {
            Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
            if (configEntry.Value == null)
                return dictionary;
            foreach (string nameAndCost in configEntry.Value.Split(','))
            {
                string[] values = nameAndCost.Split('=');
                if (values.Length == 2)
                {
                    string key = values[0].Trim();
                    if (!dictionary.ContainsKey(key))
                        dictionary[key] = new HashSet<string>();
                    dictionary[key].Add(values[1].Trim());
                }
            }
            return dictionary;
        }

        private static Dictionary<string, int> ParseConfigNameValue(ConfigEntry<string> configEntry)
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            if (configEntry.Value == null)
                return dictionary;
            foreach (string nameAndCost in configEntry.Value.Split(','))
            {
                string[] values = nameAndCost.Split('=');
                if (values.Length == 2)
                    if (int.TryParse(values[1], out int result))
                        dictionary.Add(values[0], result);
            }
            return dictionary;
        }

        private const string craftingConfig = @"TurretBoltWood;1;100;Feathers=100,RoundLog=100;|HoneyGlazedChickenUncooked;1;4;MushroomJotunpuffs=100;HoneyGlazedChicken=20|FishAndBreadUncooked;1;;;|FishingBaitAshlands;1;;;|FishingBaitCave;1;;;|FishingBaitDeepNorth;1;;;|FishingBaitForest;1;;;|FishingBaitMistlands;1;;;|FishingBaitOcean;1;;;|FishingBaitPlains;1;;;|FishingBaitSwamp;1;;;|FishWraps;1;;;|SeekerAspic;1;30;;|MisthareSupremeUncooked;1;;Carrot=50;|MushroomOmelette;1;;MushroomJotunPuffs=100,ChickenEgg=20;|SerpentStew;1;;;|Salad;1;50;MushroomJotunPuffs=100,Onion=50;";
    }
}
