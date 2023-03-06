using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TomekDexValheimMod.Controllers
{
    public class MonsterAIController : ContainerQuickDistributionObject<MonsterAI>
    {
        public static bool ChekPath { get; set; }
        public static bool OnlyTamed { get; set; }
        public static Dictionary<string, int> LimitFood { get; set; }

        private Vector3 lastPosition;

        public override void UpdateOnTime()
        {
            if (OnlyTamed && !MBComponet.m_tamable.m_character.IsTamed())
                return;
            if (!MBComponet.m_attackPlayerObjects || MBComponet.m_consumeItems == null || MBComponet.m_consumeItems.Count == 0 || MBComponet.m_onConsumedItem == null)
                return;
            if (MBComponet.m_tamable && !MBComponet.m_tamable.IsHungry())
                return;
            if (MBComponet.IsAlerted() || MBComponet.m_targetStatic != null || MBComponet.m_targetCreature != null)
                return;
            Feed();
        }

        private void Feed()
        {
            if (lastPosition == default)
                lastPosition = new Vector3(MBComponet.transform.position.x, MBComponet.transform.position.y, MBComponet.transform.position.z);
            else
            {
                float distance = Vector3.Distance(lastPosition, MBComponet.transform.position);
                if (distance >= 4)
                {
                    ContainerQuickAccess.UnRegistertNearbyContainer(lastPosition, WorkingArea);
                    lastPosition = new Vector3(MBComponet.transform.position.x, MBComponet.transform.position.y, MBComponet.transform.position.z);
                }
            }
            List<KeyValuePair<ItemDrop, int?>> consumeItems = MBComponet.m_consumeItems
                .Select(a => new KeyValuePair<ItemDrop, int?>(a, LimitFood.TryGetValue(a.name, out int limit) ? (int?)limit : null)).ToList();

            IEnumerable<ContainerIdDistance> contaniers = ContainerQuickAccess.GetRegistertNearbyContainerWithItemWithLimit(lastPosition, WorkingArea, consumeItems);
            foreach (ContainerIdDistance contanier in contaniers)
            {
                if (ChekPath)
                {
                    Vector3 moveContainerVector = Vector3.MoveTowards(contanier.Container.transform.position, MBComponet.gameObject.transform.position, 2);
                    if (!Pathfinding.instance.HavePath(MBComponet.gameObject.transform.position, moveContainerVector, MBComponet.m_pathAgentType))
                        continue;
                }
                foreach (KeyValuePair<ItemDrop, int?> item in consumeItems)
                {
                    if (!ContainerQuickAccess.TryRemoveItem(contanier.Container.m_inventory, item.Key.m_itemData, 1).Any())
                        continue;
                    MBComponet.m_onConsumedItem?.Invoke(MBComponet.m_consumeTarget);
                    (MBComponet.m_character as Humanoid).m_consumeItemEffects.Create(MBComponet.transform.position, Quaternion.identity);
                    return;
                }
            }
        }
    }
}