using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TomekDexValheimMod
{
    [HarmonyPatch]
    public class SkanOnJump
    {
        static HashSet<string> raported = new HashSet<string>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "OnJump")]
        public static void Prefix(Player __instance)
        {
            
            foreach (var item in Skan.Skanned.OrderBy(a => a.Key))
            {
                string log = $"AwakeAll {item.Key} - {string.Join(" ", item.Value.OrderBy(a => a))}";
                if (raported.Add(log))
                    Debug.Log(log);
            }
        }



        private static void Skaning(Player __instance)
        {
            Collider[] source = Physics.OverlapSphere(__instance.transform.position, 5);
            IOrderedEnumerable<Collider> orderedEnumerable = source.OrderBy((Collider x) => Vector3.Distance(x.gameObject.transform.position, __instance.transform.position));
            List<Container> list = new List<Container>();
            HashSet<string> uniq = new HashSet<string>();
            foreach (Collider item in orderedEnumerable)
            {
                try
                {
                    if (uniq.Add(item.gameObject.name))
                        Log(item.gameObject);
                }
                catch
                {
                }
            }
        }

        public static void Log(object obj)
        {
            if (obj == null)
                return;

            foreach (PropertyInfo item in obj.GetType().GetProperties())
            {
                object v = item.GetValue(obj);
                if (v != default)
                    Debug.Log($"{item.Name} {v}");
            }
            foreach (FieldInfo item in obj.GetType().GetFields())
            {
                object v = item.GetValue(obj);
                if (v != default)
                    Debug.Log($"{item.Name} {v}");
            }
        }
    }
}
