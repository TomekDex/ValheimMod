using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace TomekDexValheimMod
{
    internal class ContainerQuickDistributionQueue
    {
        public static LinkedList<(MonoBehaviour mb, Type type)> Queue { get; } = new LinkedList<(MonoBehaviour mb, Type type)>();
        public static int ProcessingTimeMilliseconds { get; internal set; }
        public static float TimeSecondsToProcess { get; internal set; }
        public static float DelayedWhenStart { get; internal set; }

        internal static void ProcessElements()
        {
            if (Queue.Count == 0)
                return;
            (MonoBehaviour mb, Type type) element;
            Stopwatch sw = Stopwatch.StartNew();
            int count = 0;
            do
            {
                element = Queue.First.Value;
                Queue.RemoveFirst();
                if (element.mb != null)
                {
                    count++;
                    Queue.AddLast(element);
                    dynamic a = element.mb.GetComponent(element.type);
                    a.UpdateOnTime();
                    if (Queue.Count <= count)
                        break;
                }
            } while (Queue.Count != 0 && sw.ElapsedMilliseconds <= ProcessingTimeMilliseconds);
        }
    }
}
