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
            for (int i = 0; i < item.m_itemData.m_stack;)
            {
                ItemDrop itemNew = Object.Instantiate(item);
                if (item.m_itemData.m_shared.m_maxStackSize < item.m_itemData.m_stack)
                    itemNew.m_itemData.m_stack = item.m_itemData.m_shared.m_maxStackSize;
                else
                    itemNew.m_itemData.m_stack = item.m_itemData.m_stack;
                i += itemNew.m_itemData.m_stack;
                Object.Instantiate(itemNew, position, Quaternion.identity);
            }
        }
    }
}
