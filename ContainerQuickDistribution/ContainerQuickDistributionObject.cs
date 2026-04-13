using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TomekDexValheimMod
{
    public interface IUpdatedOnTime
    {
        void UpdateOnTime();
    }

    public abstract class ContainerQuickDistributionObject<T> : MonoBehaviour, IUpdatedOnTime
        where T : MonoBehaviour
    {
        public static bool On { get; set; }
        public static bool UseQueue { get; set; }
        public static int WorkingArea { get; set; }
        public static float MinSecUpdate { get; set; }
        public static float MaxSecUpdate { get; set; }
        public static HashSet<string> Disable { get; set; }
        public static HashSet<string> DisableAllExcept { get; set; }
        public T MBComponet { get; private set; }

        private (MonoBehaviour mb, System.Type type) element;

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

            if (UseQueue)
            {
                element = (MBComponet, GetType());
                ContainerQuickDistributionQueue.Queue.AddLast(element);
            }
            else
                StartCoroutine(Coroutine());
            if (ContainerQuickDistributionConfig.Logs)
                Debug.Log($"Awake {MBComponet.GetType().Name} || {MBComponet.name} || {MBComponet.transform.position}");
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
                if (ContainerQuickDistributionConfig.Logs)
                    Debug.Log($"Update {MBComponet.GetType().Name} ||| {sw.Elapsed} ||| {MBComponet.name}");
            }
        }

        public abstract void UpdateOnTime();

        // Helper to create ItemData from a prefab without instantiating the prefab in the scene.
        public ItemDrop.ItemData CreateItemData(GameObject prefab, int stack)
        {
            if (prefab == null)
            {
                ZLog.Log("No prefab provided to CreateItemData");
                return null;
            }
            ItemDrop prefabItem = prefab.GetComponent<ItemDrop>();
            if (prefabItem == null)
            {
                ZLog.Log($"Prefab does not contain ItemDrop: {prefab.name}");
                return null;
            }
            ItemDrop.ItemData data = prefabItem.m_itemData.Clone();
            data.m_stack = stack;
            return data;
        }

        public ItemDrop.ItemData CreateItemData(ItemDrop prefabItem, int stack)
        {
            if (prefabItem == null)
            {
                ZLog.Log("No item prefab provided to CreateItemData");
                return null;
            }
            ItemDrop.ItemData data = prefabItem.m_itemData.Clone();
            data.m_stack = stack;
            return data;
        }

        private void OnDestroy()
        {
            ContainerQuickDistributionQueue.Queue.Remove(element);
            ContainerQuickAccess.UnRegistertNearbyContainer(MBComponet.transform.position, WorkingArea);
        }
    }
}
