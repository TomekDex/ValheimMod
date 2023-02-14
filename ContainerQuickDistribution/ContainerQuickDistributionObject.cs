using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TomekDexValheimMod
{
    public abstract class ContainerQuickDistributionObject<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        public static bool On { get; set; }
        public static int WorkingArea { get; set; }
        public static float MinSecUpdate { get; set; }
        public static float MaxSecUpdate { get; set; }
        public static HashSet<string> Disable { get; set; }
        public static HashSet<string> DisableAllExcept { get; set; }
        public T MBComponet { get; private set; }

        private bool registerted;

        public void Awake()
        {
            MBComponet = GetComponent<T>();
            string name = MBComponet.name.Replace("(Clone)", "").Trim();
            if (DisableAllExcept.Count > 0)
            {
                if (!DisableAllExcept.Contains(name))
                {
                    Destroy(this);
                    return;
                }
            }
            else if (Disable.Contains(name))
            {
                Destroy(this);
                return;
            }
            StartCoroutine(Coroutine());
            if (ContainerQuickDistribution.Logs)
                Debug.Log($"Awake {MBComponet.GetType().Name} || {MBComponet.name}");
        }

        private IEnumerator Coroutine()
        {
            while (true)
            {
                float secondsToWait = Random.Range(MinSecUpdate, MaxSecUpdate);
                yield return new WaitForSeconds(secondsToWait);
                registerted = registerted || ContainerQuickAccess.RegistertNearbyContainer(MBComponet.transform.position, WorkingArea);
                System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                UpdateOnTime();
                sw.Stop();
                if (ContainerQuickDistribution.Logs)
                    Debug.Log($"Update {MBComponet.GetType().Name} ||| {sw.Elapsed} ||| {MBComponet.name}");
            }
        }

        public abstract void UpdateOnTime();

        private void OnDestroy()
        {
            ContainerQuickAccess.UnRegistertNearbyContainer(MBComponet.transform.position, WorkingArea);
        }
    }
}
