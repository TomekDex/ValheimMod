using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace TomekDexValheimMod
{
    [BepInPlugin("TomekDexValheimMod.ContainerQuickAccess", "ContainerQuickAccess", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class ContainerQuickAccessConfig : BaseUnityPlugin
    {
        public static bool UseCart { get; private set; }
        public static bool UseShip { get; private set; }
        public static bool UseObliterator { get; private set; }
        public static bool UseCheckAccess { get; private set; }
        public static bool Logs { get; private set; }

        public void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            UseCart = Config.Bind("ContainerQuickAccessConfig", "UseCart", true, "").Value;
            UseShip = Config.Bind("ContainerQuickAccessConfig", "UseShip", true, "").Value;
            UseObliterator = Config.Bind("ContainerQuickAccessConfig", "UseObliterator", true, "").Value;
            UseCheckAccess = Config.Bind("ContainerQuickAccessConfig", "UseCheckAccess", false, "").Value;
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
