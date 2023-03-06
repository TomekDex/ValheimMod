using HarmonyLib;
using UnityEngine;

namespace TomekDexValheimMod
{
    [HarmonyPatch]
    public class TerminalPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Terminal), "TryRunCommand")]
        public static bool PrefixTerminalTryRunCommand(Terminal __instance, string text)
        {
            text = text.Trim().ToLower();
            if (text.StartsWith("cq"))
            {
                if (text.Contains("text"))
                    ContainerQuickAccessConfig.HideText = !ContainerQuickAccessConfig.HideText;
                if (text.Contains("block"))
                    ContainerQuickAccessConfig.BlockChange = !ContainerQuickAccessConfig.BlockChange;
                bool force = text.Contains("force");
                if (text.Contains("all"))
                    foreach (ContainerExtension container in Object.FindObjectsOfType<ContainerExtension>())
                        ProcessCommand(ref text, force, container);
                else if (ContainerExtension.LastUse != null)
                    ProcessCommand(ref text, force, ContainerExtension.LastUse);
                return false;
            }
            return true;
        }

        private static void ProcessCommand(ref string text, bool force, ContainerExtension container)
        {
            if (text.Contains("reset"))
                container.Reset();
            if (text.Contains("clear"))
                ContainerQuickAccess.Clear(container.GetComponent<Container>(), container);
            if (text.Contains("setmode") && (force || container.Mode == SortMode.None))
                SetMode(ref text, container, force);
            if (text.Contains("assign") && (force || container.Mode == SortMode.None))
                container.Assign();
        }

        private static void SetMode(ref string text, ContainerExtension container, bool force)
        {
            if (text.Contains("free"))
                container.Mode = SortMode.Free;
            else if (text.Contains("lock"))
                container.Mode = SortMode.Lock;
            else if (text.Contains("none"))
                container.Mode = SortMode.None;
            else if (text.Contains("restrictive"))
                container.Mode = SortMode.Restrictive;
            container.Save();
        }
    }
}
