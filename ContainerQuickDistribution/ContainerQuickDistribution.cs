using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomekDexValheimMod.Controllers;

namespace TomekDexValheimMod
{
    [BepInPlugin("TomekDexValheimMod.ContainerQuickDistribution", "ContainerQuickDistribution", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class ContainerQuickDistribution : BaseUnityPlugin
    {
        public static bool Logs { get; private set; }
        public static Dictionary<Type, bool> ConfigOn = new Dictionary<Type, bool>();

        public void Awake()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ContainerQuickDistributionObject<>));
            foreach (Type type in assembly.GetTypes())
                if (type.BaseType.IsGenericType && type.BaseType?.GetGenericTypeDefinition() == typeof(ContainerQuickDistributionObject<>))
                {
                    Type baseTypeGenericArgument = type.BaseType.GetGenericArguments()[0];
                    ConfigOn[baseTypeGenericArgument] = Config.Bind(baseTypeGenericArgument.Name, "On", true, "").Value;
                    PropertyInfo property = type.BaseType?.GetProperty("On", BindingFlags.Public | BindingFlags.Static);
                    property.SetValue(null, ConfigOn[baseTypeGenericArgument]);
                    property = type.BaseType?.GetProperty("WorkingArea", BindingFlags.Public | BindingFlags.Static);
                    property.SetValue(null, Config.Bind(baseTypeGenericArgument.Name, "WorkingArea", 50, "").Value);
                    property = type.BaseType?.GetProperty("MinSecUpdate", BindingFlags.Public | BindingFlags.Static);
                    property.SetValue(null, Config.Bind(baseTypeGenericArgument.Name, "MinSecUpdate", 10, "").Value);
                    property = type.BaseType?.GetProperty("MaxSecUpdate", BindingFlags.Public | BindingFlags.Static);
                    property.SetValue(null, Config.Bind(baseTypeGenericArgument.Name, "MaxSecUpdate", 30, "").Value);
                    property = type.BaseType?.GetProperty("Disable", BindingFlags.Public | BindingFlags.Static);
                    switch (baseTypeGenericArgument.Name)
                    {
                        case "Fireplace":
                            string posibleDisableForFireplace = "fire_pit,bonfire,hearth,piece_groundtorch,piece_groundtorch_green,piece_groundtorch_blue,piece_groundtorch_wood,piece_walltorch,piece_brazierfloor01,piece_brazierceiling01";
                            property.SetValue(null, ParseHashSet(Config.Bind(baseTypeGenericArgument.Name, "Disable", posibleDisableForFireplace, posibleDisableForFireplace).Value));
                            break;
                        case "Smelter":
                            string posibleDisableForSmelter = "piece_bathtub,smelter,blastfurnace,eitrrefinery,charcoal_kiln,piece_spinningwheel,windmill";
                            property.SetValue(null, ParseHashSet(Config.Bind(baseTypeGenericArgument.Name, "Disable", "", posibleDisableForSmelter).Value));
                            break;
                        case "CookingStation":
                            string posibleDisableCookingStation = "piece_oven,piece_cookingstation_iron,piece_cookingstation";
                            property.SetValue(null, ParseHashSet(Config.Bind(baseTypeGenericArgument.Name, "Disable", "", posibleDisableCookingStation).Value));
                            break;
                        case "ItemDrop":
                            string posibleDisableItemDrop = "ChickenEgg,Fish1,Fish2,Fish3,Fish4,Fish5,Fish6,Fish7,Fish8,Fish9,Fish10,Fish11,Fish12";
                            property.SetValue(null, ParseHashSet(Config.Bind(baseTypeGenericArgument.Name, "Disable", posibleDisableItemDrop, posibleDisableItemDrop).Value));
                            break;
                        default:
                            property.SetValue(null, ParseHashSet(Config.Bind(baseTypeGenericArgument.Name, "Disable", "", "").Value));
                            break;
                    }
                    property = type.BaseType?.GetProperty("DisableAllExcept", BindingFlags.Public | BindingFlags.Static);
                    switch (baseTypeGenericArgument.Name)
                    {
                        case "MonsterAI":
                            string posibleDisableAllExceptMonsterAI = "Lox,Hen,Wolf,Boar";
                            property.SetValue(null, ParseHashSet(Config.Bind(baseTypeGenericArgument.Name, "DisableAllExcept", "", posibleDisableAllExceptMonsterAI).Value));
                            break;
                        default:
                            property.SetValue(null, ParseHashSet(Config.Bind(baseTypeGenericArgument.Name, "DisableAllExcept", "", "").Value));
                            break;
                    }
                }
            CookingStationController.LimitWood = ParseConfigNameValue(Config.Bind("CookingStation", "LimitWood", "", "Stops adding to production if nearby chests don't have enough\r\nWood=100000000\r\n0 skip, null unlimit"));
            CookingStationController.LimitCooking = ParseConfigNameValue(Config.Bind("CookingStation", "LimitCooking", "", "Stops adding to production if nearby chests already have enough\r\nSerpentMeatCooked=5\r\n0 skip, null unlimit"));
            CookingStationController.LimitItemUseToCook = ParseConfigNameValue(Config.Bind("CookingStation", "LimitItemUseToCook", "NeckTail=0,RawMeat=0,WolfMeat=0,LoxMeat=0,BugMeat=0,HareMeat=0", "Stops adding to production if nearby chests don't have enough\r\nNeckTail=0,RawMeat=0,WolfMeat=0,LoxMeat=0,BugMeat=0,HareMeat=0\r\n0 skip, null unlimit"));
            SmelterController.LimitOre = ParseConfigNameValue(Config.Bind("Smelter", "LimitOre", "Barley=300,Flax=300", "Stops adding to production if nearby chests don't have enough\r\nCoal=100,Copper=200\r\n0 skip, null unlimit"));
            SmelterController.LimitProdution = ParseConfigNameValue(Config.Bind("Smelter", "LimitProdution", "Coal=100,Copper=200", "Stops adding to production if nearby chests already have enough\r\nCoal=100,Copper=200\r\n0 skip, null unlimit"));
            SmelterController.SkipOre = ParseConfigNameNames(Config.Bind("Smelter", "SkipOre", "charcoal_kiln=FineWood,charcoal_kiln=RoundLog", "Don't add thes ore\r\ncharcoal_kiln=Wood,charcoal_kiln=FineWood,charcoal_kiln=RoundLog,smelter=CopperOre"));
            MonsterAIController.ChekPath = Config.Bind("MonsterAI", "ChekPath", true, "").Value;
            MonsterAIController.LimitFood = ParseConfigNameValue(Config.Bind("MonsterAI", "LimitFood", "Dandelion=200,Barley=300,Flax=300,BeechSeeds=0,BirchSeeds=0,CarrotSeeds=0,OnionSeeds=0,TurnipSeeds=0,Cloudberry=0", "Stops adding to animal if nearby chests don't have enough\r\nOnionSeeds=0,TurnipSeeds=0\r\n0 skip, null unlimit"));
            Logs = Config.Bind("", "Logs", false, "Turn on logs").Value;
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        private static HashSet<string> ParseHashSet(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return new HashSet<string>();
            return value.Split(',').Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).ToHashSet();
        }

        private Dictionary<string, HashSet<string>> ParseConfigNameNames(ConfigEntry<string> configEntry)
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
    }
}
