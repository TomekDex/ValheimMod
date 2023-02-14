using UnityEngine;

namespace TomekDexValheimMod
{
    internal class ItemHelper
    {
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
