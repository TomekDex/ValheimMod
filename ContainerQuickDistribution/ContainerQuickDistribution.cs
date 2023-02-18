using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace TomekDexValheimMod
{
    [BepInPlugin("TomekDexValheimMod.ContainerQuickDistribution", "ContainerQuickDistribution", "1.0.2")]
    [BepInProcess("valheim.exe")]
    public class ContainerQuickDistribution : BaseUnityPlugin
    {
        public void Awake()
        {
            ContainerQuickDistributionConfig.GetConfig(Config);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            if (ContainerQuickDistributionConfig.StartQueue)
                InvokeRepeating("QueueLoop", ContainerQuickDistributionQueue.DelayedWhenStart, ContainerQuickDistributionQueue.TimeSecondsToProcess);
        }

        public void QueueLoop()
        {
            ContainerQuickDistributionQueue.ProcessElements();
        }
    }
}
