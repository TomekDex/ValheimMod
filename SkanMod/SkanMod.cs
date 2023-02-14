using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace TomekDexValheimMod
{
    [BepInPlugin("TomekDexValheimMod.SkanMod", "SkanMod", "1.0.0")]
    [BepInProcess("valheim.exe")]
    public class SkanMod : BaseUnityPlugin
    {
        public void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        public static void Log(object obj)
        {
            if (obj == null)
                return;

            foreach (PropertyInfo item in obj.GetType().GetProperties())
            {
                var v = item.GetValue(obj);
                if (v != default)
                    Debug.Log($"{item.Name} {v}");
            }
            foreach (var item in obj.GetType().GetFields())
            {
                var v = item.GetValue(obj);
                if (v != default)
                    Debug.Log($"{item.Name} {v}");
            }
        }

        public static void LightningAOE(Transform t)
        {
            GameObject gO = ZNetScene.instance.GetPrefab("lightningAOE");
            gO = Instantiate(gO);
            gO.name = "Cloned";
            Transform childToRemove = gO.transform.Find("AOE_ROD");
            if (childToRemove != null)
                childToRemove.parent = null;
            Transform childToRemove2 = gO.transform.Find("AOE_AREA");
            if (childToRemove2 != null)
                childToRemove2.parent = null;
            Instantiate(gO, t.position, t.rotation);
        }
    }
}
