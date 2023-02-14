using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace TomekDexValheimMod
{
    [HarmonyPatch]
    public class ContainerQuickDistributionInicializer
    {
        private static readonly Dictionary<Type, Type> controllers = new Dictionary<Type, Type>();

        private static IEnumerable<MethodBase> TargetMethods()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(ContainerQuickDistributionObject<>));
            foreach (Type type in assembly.GetTypes())
            {
                if (type.BaseType.IsGenericType && type.BaseType?.GetGenericTypeDefinition() == typeof(ContainerQuickDistributionObject<>))
                {
                    Type baseTypeGenericArgument = type.BaseType.GetGenericArguments()[0];
                    controllers[baseTypeGenericArgument] = type;
                    MethodBase methd = GetAwakeMethod(baseTypeGenericArgument);
                    if (methd != null)
                        yield return methd;
                }
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

        public static void Postfix(object[] __args, MethodBase __originalMethod, dynamic __instance, ZNetView ___m_nview)
        {
            if (!ContainerQuickDistribution.ConfigOn[__originalMethod.ReflectedType])
                return;
            if (___m_nview?.isActiveAndEnabled == true)
                __instance.gameObject.AddComponent(controllers[__originalMethod.ReflectedType]);
        }
    }
}
