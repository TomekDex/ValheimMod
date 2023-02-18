using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace TomekDexValheimMod
{
    [HarmonyPatch]
    public class Skan
    {
        public static Dictionary<string, HashSet<string>> Skanned = new Dictionary<string, HashSet<string>>();

        static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (Type type in Assembly.GetAssembly(typeof(Turret)).GetTypes())
            {
                var method = GetAwakeMethod(type);
                if (method != null)
                    yield return method;
            }
        }

        private static MethodBase GetAwakeMethod(Type type)
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                if (method.Name == "Awake")
                    return method;
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                if (method.Name == "Awake")
                    return method;
            foreach (MethodInfo method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                if (method.Name == "Start")
                    return method;
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                if (method.Name == "Start")
                    return method;
            return null;
        }

        static HashSet<string> hh = new HashSet<string>();

        public static void Postfix(object[] __args, MethodBase __originalMethod, MonoBehaviour __instance)
        {
            if (!Skanned.ContainsKey(__instance.name))
                Skanned[__instance.name] = new HashSet<string>();
            Skanned[__instance.name].Add(__originalMethod.ReflectedType.Name);
            string log = $"Log {__originalMethod.ReflectedType.Name} {__instance.name}";
            if (hh.Add(log))
                Debug.Log(log);
        }
    }
}
