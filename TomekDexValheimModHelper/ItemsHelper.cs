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
    }
}
