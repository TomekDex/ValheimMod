using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;

namespace TomekDexValheimMod
{
    [BepInPlugin("TomekDexValheimMod.ContainerQuickAccess", "ContainerQuickAccess", "2.0.0")]
    [BepInProcess("valheim.exe")]
    public class ContainerQuickAccessConfig : BaseUnityPlugin
    {
        public static bool UseCart { get; private set; }
        public static bool UseShip { get; private set; }
        public static bool UseObliterator { get; private set; }
        public static bool UseCheckAccess { get; private set; }
        public static bool Logs { get; private set; }
        public static SortMode DefaultSortMode { get; private set; }
        public static bool HideText { get; internal set; }
        public static bool BlockChange { get; internal set; }
        public static bool UseAutoClear { get; internal set; }
        public static int WorkingArea { get; private set; }
        public void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            UseCart = Config.Bind("ContainerQuickAccessConfig", "UseCart", true, "").Value;
            UseShip = Config.Bind("ContainerQuickAccessConfig", "UseShip", true, "").Value;
            UseObliterator = Config.Bind("ContainerQuickAccessConfig", "UseObliterator", true, "").Value;
            UseCheckAccess = Config.Bind("ContainerQuickAccessConfig", "UseCheckAccess", false, "").Value;
            DefaultSortMode = (SortMode)Enum.Parse(typeof(SortMode), Config.Bind("Access", "DefaultSortMode", "None", "Shows and hides extra text on chests\r\nNone, Free, Restrictive, Lock").Value);
            HideText = Config.Bind("Access", "HideText", false, "Shows and hides extra text on chests").Value;
            BlockChange = Config.Bind("Access", "BlockChange", false, "Blockade of changing settings so that you do not change anything by accident.").Value;
            WorkingArea = Config.Bind("Sort", "WorkingArea", 50, "Sort range").Value;
            UseAutoClear = Config.Bind("Sort", "UseAutoClear", false, "It automatically calls the clear command before open container\r\nIf true DefaultSortMode should be Lock").Value;
            Logs = Config.Bind("Debug", "Logs", false, "").Value;
            if (UseCart || UseShip)
                InvokeRepeating("RefreshContainerPosition", 30, 30);
        }

        public void RefreshContainerPosition()
        {
            ContainerQuickAccess.RefreshContainerPosition();
        }
    }
}
