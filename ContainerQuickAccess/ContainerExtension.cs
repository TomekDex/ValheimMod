using System;
using System.Collections.Generic;
using System.Linq;
using TomekDexValheimModHelper;
using UnityEngine;

namespace TomekDexValheimMod
{
    public enum SortMode
    {
        None,
        Free,
        Restrictive,
        Lock
    }

    public class ContainerExtension : MonoBehaviour
    {
        private Container container;

        public static ContainerExtension LastUse { get; set; }
        public SortMode Mode { get; set; }
        public HashSet<int> ItemsFilter { get; } = new HashSet<int>();

        public void Awake()
        {
            container = GetComponent<Container>();
            Load();
        }

        internal void Assign()
        {
            ItemsFilter.Clear();
            foreach (int item in container.GetInventory().m_inventory.Select(a => ItemsHelper.GetItemDropBySharedNameOrName(a.m_shared.m_name).name.GetStableHashCode()).Distinct())
                ItemsFilter.Add(item);
            Save();
        }

        internal void Reset()
        {
            Mode = SortMode.None;
            ItemsFilter.Clear();
            Save();
        }

        internal void Save()
        {
            container.m_nview.GetZDO().Set("CQSMode", (int)Mode);

            byte[] byteArray = new byte[ItemsFilter.Count * sizeof(int)];
            int index = 0;
            foreach (int intValue in ItemsFilter)
            {
                byte[] intBytes = BitConverter.GetBytes(intValue);
                Buffer.BlockCopy(intBytes, 0, byteArray, index, intBytes.Length);
                index += intBytes.Length;
            }
            container.m_nview.GetZDO().Set("CQSItemsFilter", byteArray);
        }

        private void Load()
        {
            int sm = container.m_nview.GetZDO().GetInt("CQSMode", -1);
            Mode = sm == -1 ? ContainerQuickAccessConfig.DefaultSortMode : (SortMode)sm;
            byte[] byteArray = container.m_nview.GetZDO().GetByteArray("CQSItemsFilter");
            if (byteArray != null)
                for (int i = 0; i < byteArray.Length; i += sizeof(int))
                {
                    int intValue = BitConverter.ToInt32(byteArray, i);
                    ItemsFilter.Add(intValue);
                }
        }

        public void OnDestroy()
        {
            ContainerQuickAccess.RemoveContainer(container);
        }
    }
}
