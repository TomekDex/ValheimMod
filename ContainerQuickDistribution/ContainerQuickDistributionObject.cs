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

        public ItemDrop.ItemData CreateItemData(GameObject prefab, int stack)
        {
            if (prefab == null)
            {
                ZLog.Log("No prefab provided to CreateItemData");
                return null;
            }

            ItemDrop itemDropPrefab = prefab.GetComponent<ItemDrop>();
            if (itemDropPrefab == null)
            {
                ZLog.Log($"Prefab {prefab.name} has no ItemDrop");
                return null;
            }
            // 🔥 KLUCZOWA ZMIANA — zamiast Clone()
            ZNetView.m_forceDisableInit = true;
            GameObject go = UnityEngine.Object.Instantiate(prefab);
            ZNetView.m_forceDisableInit = false;

            ItemDrop itemDrop = go.GetComponent<ItemDrop>();
            if (itemDrop == null)
            {
                Destroy(go);
                return null;
            }

            ItemDrop.ItemData data = itemDrop.m_itemData;

            // ustaw stack
            data.m_stack = Mathf.Min(stack, data.m_shared.m_maxStackSize);

            // 🔥 bardzo ważne rzeczy dla stabilności
            data.m_durability = data.GetMaxDurability();
            data.m_dropPrefab = prefab;

            // 🧹 usuwamy tymczasowy obiekt ze świata
            Destroy(go);

            return data;
        }

        public ItemDrop.ItemData CreateItemData(ItemDrop prefabItem, int stack)
        {
            if (prefabItem == null)
            {
                ZLog.Log("No item prefab provided to CreateItemData");
                return null;
            }

            return CreateItemData(prefabItem.gameObject, stack);
        }

        private void OnDestroy()
        {
            ContainerQuickDistributionQueue.Queue.Remove(element);
            ContainerQuickAccess.UnRegistertNearbyContainer(MBComponet.transform.position, WorkingArea);
        }
    }
}
