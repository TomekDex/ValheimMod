using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TomekDexValheimMod
{
    [BepInPlugin("TomekDexValheimMod.Obliterator", "Obliterator", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class ObliteratorConfig : BaseUnityPlugin
    {
        public static ConfigEntry<string> NameAndCost { get; private set; }
        public static ConfigEntry<string> NameAndValue { get; private set; }
        public static ConfigEntry<string> TypeAndValue { get; private set; }
        public static ConfigEntry<string> NameAndRewardChance { get; private set; }
        public static ConfigEntry<string> Ignore { get; private set; }
        public static ConfigEntry<bool> Sacrifice { get; private set; }
        public static ConfigEntry<bool> TryProcessEpicLoot { get; private set; }
        public static ConfigEntry<bool> ConvertEpicLootYunk { get; private set; }
        public static ConfigEntry<bool> LeaveRestAfterCnversion { get; private set; }
        public static Dictionary<string, int> NameAndCostDic { get; private set; }
        public static Dictionary<string, int> NameAndValueDic { get; private set; }
        public static Dictionary<string, int> TypeAndValueDic { get; private set; }
        public static Dictionary<string, int> NameAndRewardChanceDic { get; private set; }
        public static HashSet<string> IgnoreHash { get; private set; }

        public void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            NameAndCost = Config.Bind("Convert", "NameAndCost", "Coins=10,Coal=2", "The cost of individual elements");
            NameAndValue = Config.Bind("Convert", "NameAndValue", "SilverNecklace=300,Ruby=200,AmberPearl=100,Amber=50,Coins=10,Eitr=5", "Default 1");
            TypeAndValue = Config.Bind("Convert", "TypeAndValue", "Trophie=10", "If an item is not listed in NameAndValue has a specific type then its value is as listed here (None,Material,Consumable,OneHandedWeapon,Bow,Shield,Helmet,Chest,Ammo,Customization,Legs,Hands,Trophie,TwoHandedWeapon,Torch,Misc,Shoulder,Utility,Tool,Attach_Atgeir,Fish,TwoHandedWeaponLeft,AmmoNonEquipable)");
            NameAndRewardChance = Config.Bind("Convert", "NameAndRewardChance", "Coins=10", "Default 1");
            Ignore = Config.Bind("Convert", "Ignore", "Coins,DustEpic,DustLegendary,DustMagic,DustRare,EssenceEpic,EssenceLegendary,EssenceMagic,EssenceRare,ReagentEpic,ReagentLegendary,ReagentMagic,ReagentRare,RunestoneEpic,RunestoneLegendary,RunestoneMagic,RunestoneRare,ShardEpic,ShardLegendary,ShardMagic,ShardRare", "Do not convert and dont cout to value item will stay in chest");
            TryProcessEpicLoot = Config.Bind("EpicLoot", "TryProcessEpicLoot", true, "If you use the Epic Loot mod, take it into account");
            Sacrifice = Config.Bind("EpicLoot", "Sacrifice", true, "Sacrifice Epic loot");
            ConvertEpicLootYunk = Config.Bind("EpicLoot", "ConvertEpicLootYunk", true, "Convert scrap into materials");
            LeaveRestAfterCnversion = Config.Bind("EpicLoot", "LeaveRestAfterCnversion", true, "What has not been converted and may be, leave it");
        }

        public static void Inicialize()
        {
            if (NameAndCostDic == null)
                NameAndCostDic = ParseConfigNameValue(NameAndCost);
            if (NameAndValueDic == null)
                NameAndValueDic = ParseConfigNameValue(NameAndValue);
            if (TypeAndValueDic == null)
                TypeAndValueDic = ParseConfigNameValue(TypeAndValue);
            if (NameAndRewardChanceDic == null)
                NameAndRewardChanceDic = ParseConfigNameValue(NameAndRewardChance);
            if (IgnoreHash == null)
                IgnoreHash = Ignore.Value.Split(',').Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).ToHashSet();
        }

        private static Dictionary<string, int> ParseConfigNameValue(ConfigEntry<string> nameAndCostConf)
        {
            Dictionary<string, int> nameAndCostDic = new Dictionary<string, int>();
            foreach (string nameAndCost in nameAndCostConf.Value.Split(','))
            {
                string[] values = nameAndCost.Split('=');
                if (values.Length == 2)
                    if (int.TryParse(values[1], out int result))
                        nameAndCostDic.Add(values[0], result);
            }
            return nameAndCostDic;
        }
    }
}
