using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TomekDexValheimModHelper
{
    public class ItemsHelper
    {
        private static Dictionary<string, ItemDrop> ItemDropsName { get; set; } = new Dictionary<string, ItemDrop>();

        public static ItemDrop GetItemDropBySharedNameOrName(string name)
        {
            if (!ItemDropsName.ContainsKey(name))
            {
                GameObject prefab = ObjectDB.instance.m_items.FirstOrDefault(i => i.GetComponent<ItemDrop>().m_itemData.m_shared.m_name == name);
                if (prefab != null)
                    ItemDropsName.Add(name, prefab.GetComponent<ItemDrop>());
            }
            if (!ItemDropsName.ContainsKey(name))
            {
                GameObject prefab = ZNetScene.instance.GetPrefab(name);
                ItemDropsName.Add(name, prefab?.GetComponent<ItemDrop>());
            }
            return ItemDropsName[name];
        }

        public static void Drop(ItemDrop item, Vector3 position)
        {
            if (item == null)
                return;
            Drop(item.gameObject, item.m_itemData.m_stack, position);
        }

        public static void Drop(GameObject prefab, int stack, Vector3 position)
        {
            if (prefab == null || stack <= 0)
                return;
            ItemDrop prefabItem = prefab.GetComponent<ItemDrop>();
            if (prefabItem == null)
                return;
            int remaining = stack;
            while (remaining > 0)
            {
                int currentStack = remaining;
                if (prefabItem.m_itemData.m_shared.m_maxStackSize < currentStack)
                    currentStack = prefabItem.m_itemData.m_shared.m_maxStackSize;

                GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
                ItemDrop itemNew = go.GetComponent<ItemDrop>();
                if (itemNew != null)
                {
                    itemNew.m_itemData.m_stack = currentStack;
                }

                remaining -= currentStack;
            }
        }

        public static void Drop(ItemDrop prefabItem, int stack, Vector3 position)
        {
            if (prefabItem == null)
                return;
            Drop(prefabItem.gameObject, stack, position);
        }
    }
}
